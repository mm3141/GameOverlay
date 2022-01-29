// <copyright file="SimpleFlaskManagerCore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using ClickableTransparentOverlay;
    using Coroutine;
    using GameHelper;
    using GameHelper.CoroutineEvents;
    using GameHelper.Plugin;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using ProfileManager;

    /// <summary>
    ///     <see cref="SimpleFlaskManager" /> plugin.
    /// </summary>
    public sealed class SimpleFlaskManagerCore : PCore<SimpleFlaskManagerSettings>
    {
        private readonly Vector4 impTextColor = new(255, 255, 0, 255);
        private readonly List<string> keyPressInfo = new();
        private readonly Vector2 size = new(400, 200);
        private ActiveCoroutine onAreaChange;
        private string debugMessage = "None";
        private string newProfileName = string.Empty;
        private bool stopShowingAutoQuitWarning = false;

        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");
        private bool ShouldExecuteAutoQuit =>
            this.Settings.EnableAutoQuit &&
            this.Settings.AutoQuitCondition.Evaluate();

        /// <inheritdoc />
        public override void DrawSettings()
        {
            ImGui.PushTextWrapPos(ImGui.GetContentRegionMax().X);
            ImGui.TextColored(this.impTextColor, "Do not trust FlaskManager Settings.txt file from unknown source. " +
                              "Bad profiles may get your account banned. Also, they can contain malicious code" +
                              "that can f**k you up. ");
            ImGui.NewLine();
            ImGui.TextColored(this.impTextColor, "All the flask rules in all the profiles must have " +
                              "FLASK_EFFECT and FLASK_CHARGES condition, otherwise Flask Manager will spam " +
                              "the flask and you might get kicked or banned.");
            ImGui.NewLine();
            ImGui.TextUnformatted("Debug mode will help you figure out why flask manager is not drinking the flask. " +
                              "It will also help you figure out if flask manager is spamming the flask or not. " +
                              "So create all new profiles with debug mode turned on.");
            ImGui.PopTextWrapPos();
            ImGui.Checkbox("Debug Mode", ref this.Settings.DebugMode);
            ImGui.Checkbox("Should Run In Hideout", ref this.Settings.ShouldRunInHideout);
            ImGuiHelper.IEnumerableComboBox("Profile", this.Settings.Profiles.Keys, ref this.Settings.CurrentProfile);
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
                foreach (var (key, profile) in this.Settings.Profiles)
                {
                    if (ImGui.TreeNode($"{key}"))
                    {
                        ImGui.SameLine();
                        if (ImGui.SmallButton("Delete Profile"))
                        {
                            this.Settings.Profiles.Remove(key);
                            if (this.Settings.CurrentProfile == key)
                            {
                                this.Settings.CurrentProfile = string.Empty;
                            }
                        }

                        profile.DrawSettings();
                        ImGui.TreePop();
                    }
                }
            }

            if (ImGui.CollapsingHeader("Auto Quit"))
            {
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 6);
                ImGui.Checkbox("Enable AutoQuit", ref this.Settings.EnableAutoQuit);
                this.Settings.AutoQuitCondition.Display();
                ImGui.TextWrapped($"Current AutoQuit Condition Evaluates to {this.Settings.AutoQuitCondition.Evaluate()}");
                ImGui.Separator();
                ImGui.Text("Hotkey to manually quit game connection: ");
                ImGui.SameLine();
                ImGuiHelper.NonContinuousEnumComboBox("##Manual Quit HotKey", ref this.Settings.AutoQuitKey);
                ImGui.PopItemWidth();
            }
        }

        /// <inheritdoc />
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
                for (var i = 0; i < this.keyPressInfo.Count; i++)
                {
                    ImGui.Text($"{i}-{this.keyPressInfo[i]}");
                }

                ImGui.SetScrollHereY();
                ImGui.EndChild();
                ImGui.End();
            }

            this.AutoQuitWarningUi();
            if (!this.ShouldExecutePlugin())
            {
                return;
            }

            if (this.ShouldExecuteAutoQuit || NativeMethods.IsKeyPressedAndNotTimeout(
                (int)this.Settings.AutoQuitKey))
            {
                MiscHelper.KillTCPConnectionForProcess(Core.Process.Pid);
            }

            if (string.IsNullOrEmpty(this.Settings.CurrentProfile))
            {
                this.debugMessage = "No Profile Selected.";
                return;
            }

            if (!this.Settings.Profiles.ContainsKey(this.Settings.CurrentProfile))
            {
                this.debugMessage = $"{this.Settings.CurrentProfile} not found.";
                return;
            }

            foreach (var rule in this.Settings.Profiles[this.Settings.CurrentProfile].Rules)
            {
                rule.Execute(this.DebugLog);
            }
        }

        private void DebugLog(string logText)
        {
            if (this.Settings.DebugMode)
            {
                this.keyPressInfo.Add($"{DateTime.Now.TimeOfDay}: {logText}");
            }
        }

        /// <inheritdoc />
        public override void OnDisable()
        {
            this.onAreaChange?.Cancel();
            this.onAreaChange = null;
        }

        /// <inheritdoc />
        public override void OnEnable(bool isGameOpened)
        {
            var jsonData = File.ReadAllText(this.DllDirectory + @"/FlaskNameToBuff.json");
            JsonDataHelper.FlaskNameToBuffGroups =
                JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonData);

            var jsonData2 = File.ReadAllText(this.DllDirectory + @"/StatusEffectGroup.json");
            JsonDataHelper.StatusEffectGroups = JsonConvert.DeserializeObject<
                Dictionary<string, List<string>>>(jsonData2);

            if (File.Exists(this.SettingPathname))
            {
                var content = File.ReadAllText(this.SettingPathname);
                this.Settings = JsonConvert.DeserializeObject<SimpleFlaskManagerSettings>(
                    content,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
            }
            else
            {
                this.CreateDefaultProfile();
            }

            this.onAreaChange = CoroutineHandler.Start(this.EnableAutoQuitWarningUiOnAreaChange());
        }

        /// <inheritdoc />
        public override void SaveSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.SettingPathname));
            var settingsData = JsonConvert.SerializeObject(this.Settings,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
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
                this.debugMessage = "Game is minimized.";
                return false;
            }

            var areaDetails = Core.States.InGameStateObject.CurrentAreaInstance.AreaDetails;
            if (areaDetails.IsTown)
            {
                this.debugMessage = "Player is in town.";
                return false;
            }

            if (!this.Settings.ShouldRunInHideout && areaDetails.IsHideout)
            {
                this.debugMessage = "Player is in hideout.";
                return false;
            }

            if (Core.States.InGameStateObject.CurrentAreaInstance.Player.TryGetComponent<Life>(out var lifeComp))
            {
                if (lifeComp.Health.Current <= 0)
                {
                    this.debugMessage = "Player is dead.";
                    return false;
                }
            }
            else
            {
                this.debugMessage = "Can not find player Life component.";
                return false;
            }

            if (Core.States.InGameStateObject.CurrentAreaInstance.Player.TryGetComponent<Buffs>(out var buffComp))
            {
                if (buffComp.StatusEffects.ContainsKey("grace_period"))
                {
                    this.debugMessage = "Player has Grace Period.";
                    return false;
                }
            }
            else
            {
                this.debugMessage = "Can not find player Buffs component.";
                return false;
            }

            if (!Core.States.InGameStateObject.CurrentAreaInstance.Player.TryGetComponent<Actor>(out var _))
            {
                this.debugMessage = "Can not find player Actor component.";
                return false;
            }

            this.debugMessage = "None";
            return true;
        }

        /// <summary>
        ///     Creates a default profile that is only valid for flasks on newly created character.
        /// </summary>
        private void CreateDefaultProfile()
        {
            Profile profile = new();
            foreach (var rule in Rule.CreateDefaultRules())
            {
                profile.Rules.Add(rule);
            }

            this.Settings.Profiles.Add("LeagueStartNewPlayerProfile", profile);
        }

        private void AutoQuitWarningUi()
        {

            if (!this.stopShowingAutoQuitWarning &&
                Core.States.InGameStateObject.CurrentAreaInstance.AreaDetails.IsTown &&
                this.ShouldExecuteAutoQuit)
            {
                ImGui.OpenPopup("AutoQuitWarningUi");
            }

            if (ImGui.BeginPopup("AutoQuitWarningUi"))
            {
                ImGui.Text("Please fix your AutoQuit Condition, it's evaluating to true in town.\n" +
                    "You will logout automatically as soon as you leave town.");
                if (ImGui.Button("Ok", new Vector2(400f, 50f)))
                {
                    this.stopShowingAutoQuitWarning = true;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }

        private IEnumerator<Wait> EnableAutoQuitWarningUiOnAreaChange()
        {
            while (true)
            {
                yield return new Wait(RemoteEvents.AreaChanged);
                this.stopShowingAutoQuitWarning = false;
            }
        }
    }
}