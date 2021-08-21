// <copyright file="SimpleFlaskManagerCore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager
{
    using System.Collections.Generic;
    using System.IO;
    using GameHelper;
    using GameHelper.Plugin;
    using GameHelper.RemoteEnums;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using SimpleFlaskManager.ProfileManager;

    /// <summary>
    /// <see cref="SimpleFlaskManager"/> plugin.
    /// </summary>
    public sealed class SimpleFlaskManagerCore : PCore<SimpleFlaskManagerSettings>
    {
        private string newProfileName = string.Empty;

        /// <inheritdoc/>
        public override void DrawSettings()
        {
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
            if (!this.ShouldExecutePlugin(out var debugMessage))
            {
                this.PrintDebugMessage(debugMessage);
                return;
            }

            foreach (var rule in this.Settings.Profiles[this.Settings.CurrentProfile].Rules)
            {
                if (rule.Condition.Evaluate())
                {
                    MiscHelper.KeyUp(rule.Key);
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
            JsonDataHelper.FlaskNameToBuff =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
        }

        /// <inheritdoc/>
        public override void SaveSettings()
        {
        }

        private void PrintDebugMessage(string debugMessage)
        {
            if (this.Settings.DebugMode)
            {
                ImGui.Begin(
                    "SimpleFlaskManager Debug Mode",
                    ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.Text($"Debug Message: {debugMessage}");
                ImGui.End();
            }
        }

        private bool ShouldExecutePlugin(out string debugMessage)
        {
            debugMessage = string.Empty;
            var cgs = Core.States.GameCurrentState;
            if (cgs != GameStateTypes.InGameState)
            {
                debugMessage = $"Current game state isn't InGameState, it's {cgs}.";
                return false;
            }

            if (!Core.Process.Foreground)
            {
                debugMessage = $"Game is minimized.";
                return false;
            }

            var areaDetails = Core.States.InGameStateObject.CurrentAreaInstance.AreaDetails;
            if (areaDetails.IsTown)
            {
                debugMessage = $"Player is in town.";
                return false;
            }

            if (!this.Settings.ShouldRunInHideout && areaDetails.IsHideout)
            {
                debugMessage = $"Player is in hideout.";
                return false;
            }

            if (string.IsNullOrEmpty(this.Settings.CurrentProfile))
            {
                debugMessage = $"No Profile Selected.";
                return false;
            }

            if (!this.Settings.Profiles.ContainsKey(this.Settings.CurrentProfile))
            {
                debugMessage = $"{this.Settings.CurrentProfile} not found.";
                return false;
            }

            return true;
        }
    }
}
