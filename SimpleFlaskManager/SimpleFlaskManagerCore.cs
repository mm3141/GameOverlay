﻿// <copyright file="SimpleFlaskManagerCore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using GameHelper;
    using GameHelper.Plugin;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using SimpleFlaskManager.ProfileManager;

    /// <summary>
    /// <see cref="SimpleFlaskManager"/> plugin.
    /// </summary>
    public sealed class SimpleFlaskManagerCore : PCore<SimpleFlaskManagerSettings>
    {
        private Vector2 size = new Vector2(400, 200);
        private string debugMessage = "None";
        private string newProfileName = string.Empty;
        private List<string> keyPressInfo = new List<string>();

        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");

        /// <inheritdoc/>
        public override void DrawSettings()
        {
            ImGui.TextWrapped($"WARNING: Do not trust FlaskManager Settings.txt file from unknown source. " +
                $"Bad profiles may get your account banned. Also, they can contain malicious code. " +
                $"Google SCS0028 and CA2328 for more information.");

            ImGui.NewLine();
            ImGui.NewLine();
            ImGui.TextWrapped("WARNING: All the flask rules in all the profiles must have " +
                "FLASK_EFFECT and FLASK_CHARGES condition, otherwise Flask Manager will spam " +
                "the flask and you might get kicked or banned.");
            ImGui.NewLine();
            ImGui.NewLine();
            ImGui.TextWrapped("Debug mode will help you figure out why flask manager is not drinking the flask. " +
                "It will also help you figure out if flask manager is spamming the flask or not. So create all new " +
                "profiles with debug mode turned on.");
            ImGui.Checkbox("Debug Mode", ref this.Settings.DebugMode);
            ImGui.Checkbox("Should Run In Hideout", ref this.Settings.ShouldRunInHideout);
            if (ImGui.BeginCombo("Profile", this.Settings.CurrentProfile))
            {
                foreach (var profile in this.Settings.Profiles)
                {
                    bool selected = profile.Key == this.Settings.CurrentProfile;
                    if (ImGui.IsWindowAppearing() && selected)
                    {
                        ImGui.SetScrollHereY();
                    }

                    if (ImGui.Selectable(profile.Key, selected))
                    {
                        this.Settings.CurrentProfile = profile.Key;
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.NewLine();
            if (ImGui.CollapsingHeader("Add New Profile"))
            {
                ImGui.InputText("Name", ref this.newProfileName, 50);
                ImGui.SameLine();
                if (ImGui.Button("Add"))
                {
                    if (!string.IsNullOrEmpty(this.newProfileName))
                    {
                        this.Settings.Profiles.Add(this.newProfileName, new Profile());
                        this.newProfileName = string.Empty;
                    }
                }
            }

            if (ImGui.CollapsingHeader("Profiles"))
            {
                foreach (var profile in this.Settings.Profiles)
                {
                    if (ImGui.TreeNode($"{profile.Key}"))
                    {
                        ImGui.SameLine();
                        if (ImGui.SmallButton("Delete Me"))
                        {
                            this.Settings.Profiles.Remove(profile.Key);
                            if (this.Settings.CurrentProfile == profile.Key)
                            {
                                this.Settings.CurrentProfile = string.Empty;
                            }
                        }

                        profile.Value.DrawSettings();
                        ImGui.TreePop();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void DrawUI()
        {
            if (this.Settings.DebugMode)
            {
                ImGui.SetNextWindowSizeConstraints(this.size, this.size * 2);
                ImGui.Begin("Debug Mode Window");
                ImGui.TextWrapped($"Current Issue: {this.debugMessage}");
                if (ImGui.Button("Clear History"))
                {
                    this.keyPressInfo.Clear();
                }

                ImGui.BeginChild("KeyPressesInfo");
                for (int i = 0; i < this.keyPressInfo.Count; i++)
                {
                    ImGui.Text($"{i}-{this.keyPressInfo[i]}");
                }

                ImGui.SetScrollHereY();
                ImGui.EndChild();
                ImGui.End();
            }

            if (!this.ShouldExecutePlugin())
            {
                return;
            }

            foreach (var rule in this.Settings.Profiles[this.Settings.CurrentProfile].Rules)
            {
                if (rule.Condition.Evaluate())
                {
                    if (MiscHelper.KeyUp(rule.Key) && this.Settings.DebugMode)
                    {
                        this.keyPressInfo.Add($"{DateTime.Now.TimeOfDay}: I pressed {rule.Key} key.");
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
        }

        /// <inheritdoc/>
        public override void OnEnable(bool isGameOpened)
        {
            var jsonData = File.ReadAllText(this.DllDirectory + @"/FlaskNameToBuff.json");
            JsonDataHelper.FlaskNameToBuff = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);

            if (File.Exists(this.SettingPathname))
            {
                var content = File.ReadAllText(this.SettingPathname);
                this.Settings = JsonConvert.DeserializeObject<SimpleFlaskManagerSettings>(
                    content,
                    new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                    });
            }
        }

        /// <inheritdoc/>
        public override void SaveSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.SettingPathname));
            var settingsData = JsonConvert.SerializeObject(
                this.Settings,
                Formatting.Indented,
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                });
            File.WriteAllText(this.SettingPathname, settingsData);
        }

        private bool ShouldExecutePlugin()
        {
            var cgs = Core.States.GameCurrentState;
            if (cgs != GameStateTypes.InGameState)
            {
                this.debugMessage = $"Current game state isn't InGameState, it's {cgs}.";
                return false;
            }

            if (!Core.Process.Foreground)
            {
                this.debugMessage = $"Game is minimized.";
                return false;
            }

            var areaDetails = Core.States.InGameStateObject.CurrentAreaInstance.AreaDetails;
            if (areaDetails.IsTown)
            {
                this.debugMessage = $"Player is in town.";
                return false;
            }

            if (!this.Settings.ShouldRunInHideout && areaDetails.IsHideout)
            {
                this.debugMessage = $"Player is in hideout.";
                return false;
            }

            if (string.IsNullOrEmpty(this.Settings.CurrentProfile))
            {
                this.debugMessage = $"No Profile Selected.";
                return false;
            }

            if (!this.Settings.Profiles.ContainsKey(this.Settings.CurrentProfile))
            {
                this.debugMessage = $"{this.Settings.CurrentProfile} not found.";
                return false;
            }

            if (Core.States.InGameStateObject.CurrentAreaInstance.Player.TryGetComponent<Life>(out var lifeComp))
            {
                if (lifeComp.Health.Current <= 0)
                {
                    this.debugMessage = $"Player is dead.";
                    return false;
                }
            }
            else
            {
                this.debugMessage = $"Can not find player Life component.";
            }

            this.debugMessage = "None";
            return true;
        }
    }
}
