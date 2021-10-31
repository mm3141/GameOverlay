// <copyright file="SimpleFlaskManagerCore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager {
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
    using ProfileManager;

    /// <summary>
    ///     <see cref="SimpleFlaskManager" /> plugin.
    /// </summary>
    public sealed class SimpleFlaskManagerCore : PCore<SimpleFlaskManagerSettings> {
        private readonly List<string> keyPressInfo = new();
        private string debugMessage = "None";
        private string newProfileName = string.Empty;
        private readonly Vector2 size = new(400, 200);

        private string SettingPathname => Path.Join(DllDirectory, "config", "settings.txt");

        /// <inheritdoc />
        public override void DrawSettings() {
            ImGui.TextWrapped("WARNING: Do not trust FlaskManager Settings.txt file from unknown source. " +
                              "Bad profiles may get your account banned. Also, they can contain malicious code. " +
                              "Google SCS0028 and CA2328 for more information.");

            ImGui.NewLine();
            ImGui.NewLine();
            ImGui.TextWrapped("WARNING: All the flask rules in all the profiles must have " +
                              "FLASK_EFFECT and FLASK_CHARGES condition, otherwise Flask Manager will spam " +
                              "the flask and you might get kicked or banned.");
            ImGui.NewLine();
            ImGui.NewLine();
            ImGui.TextWrapped("Debug mode will help you figure out why flask manager is not drinking the flask. " +
                              "It will also help you figure out if flask manager is spamming the flask or not. " +
                              "So create all new profiles with debug mode turned on.");
            ImGui.Checkbox("Debug Mode", ref Settings.DebugMode);
            ImGui.Checkbox("Should Run In Hideout", ref Settings.ShouldRunInHideout);
            UiHelper.IEnumerableComboBox("Profile", Settings.Profiles.Keys, ref Settings.CurrentProfile);
            ImGui.NewLine();
            if (ImGui.CollapsingHeader("Add New Profile")) {
                ImGui.InputText("Name", ref newProfileName, 50);
                ImGui.SameLine();
                if (ImGui.Button("Add")) {
                    if (!string.IsNullOrEmpty(newProfileName)) {
                        Settings.Profiles.Add(newProfileName, new Profile());
                        newProfileName = string.Empty;
                    }
                }
            }

            if (ImGui.CollapsingHeader("Profiles")) {
                foreach (var (key, profile) in Settings.Profiles) {
                    if (ImGui.TreeNode($"{key}")) {
                        ImGui.SameLine();
                        if (ImGui.SmallButton("Delete Profile")) {
                            Settings.Profiles.Remove(key);
                            if (Settings.CurrentProfile == key) {
                                Settings.CurrentProfile = string.Empty;
                            }
                        }

                        profile.DrawSettings();
                        ImGui.TreePop();
                    }
                }
            }

            if (ImGui.CollapsingHeader("Auto Quit")) {
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 6);
                Settings.AutoQuitCondition.Display(int.MaxValue - 1);
                ImGui.PopItemWidth();
            }
        }

        /// <inheritdoc />
        public override void DrawUI() {
            if (Settings.DebugMode) {
                ImGui.SetNextWindowSizeConstraints(size, size * 2);
                ImGui.Begin("Debug Mode Window");
                ImGui.TextWrapped($"Current Issue: {debugMessage}");
                if (ImGui.Button("Clear History")) {
                    keyPressInfo.Clear();
                }

                ImGui.BeginChild("KeyPressesInfo");
                for (var i = 0; i < keyPressInfo.Count; i++) {
                    ImGui.Text($"{i}-{keyPressInfo[i]}");
                }

                ImGui.SetScrollHereY();
                ImGui.EndChild();
                ImGui.End();
            }

            if (!ShouldExecutePlugin()) {
                return;
            }

            if (Settings.AutoQuitCondition.Evaluate()) {
                MiscHelper.KillTCPConnectionForProcess(Core.Process.Pid);
            }

            if (string.IsNullOrEmpty(Settings.CurrentProfile)) {
                debugMessage = "No Profile Selected.";
                return;
            }

            if (!Settings.Profiles.ContainsKey(Settings.CurrentProfile)) {
                debugMessage = $"{Settings.CurrentProfile} not found.";
                return;
            }

            foreach (var rule in Settings.Profiles[Settings.CurrentProfile].Rules) {
                if (rule.Condition != null && rule.Enable && rule.Condition.Evaluate()) {
                    if (MiscHelper.KeyUp(rule.Key) && Settings.DebugMode) {
                        keyPressInfo.Add($"{DateTime.Now.TimeOfDay}: I pressed {rule.Key} key.");
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void OnDisable() { }

        /// <inheritdoc />
        public override void OnEnable(bool isGameOpened) {
            var jsonData = File.ReadAllText(DllDirectory + @"/FlaskNameToBuff.json");
            JsonDataHelper.FlaskNameToBuffGroups =
                JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonData);

            var jsonData2 = File.ReadAllText(DllDirectory + @"/StatusEffectGroup.json");
            JsonDataHelper.StatusEffectGroups = JsonConvert.DeserializeObject<
                Dictionary<string, List<string>>>(jsonData2);

            if (File.Exists(SettingPathname)) {
                var content = File.ReadAllText(SettingPathname);
                Settings = JsonConvert.DeserializeObject<SimpleFlaskManagerSettings>(
                    content,
                    new JsonSerializerSettings {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
            }
        }

        /// <inheritdoc />
        public override void SaveSettings() {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingPathname));
            var settingsData = JsonConvert.SerializeObject(
                Settings,
                Formatting.Indented,
                new JsonSerializerSettings {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            File.WriteAllText(SettingPathname, settingsData);
        }

        private bool ShouldExecutePlugin() {
            var cgs = Core.States.GameCurrentState;
            if (cgs != GameStateTypes.InGameState) {
                debugMessage = $"Current game state isn't InGameState, it's {cgs}.";
                return false;
            }

            if (!Core.Process.Foreground) {
                debugMessage = "Game is minimized.";
                return false;
            }

            var areaDetails = Core.States.InGameStateObject.CurrentAreaInstance.AreaDetails;
            if (areaDetails.IsTown) {
                debugMessage = "Player is in town.";
                return false;
            }

            if (!Settings.ShouldRunInHideout && areaDetails.IsHideout) {
                debugMessage = "Player is in hideout.";
                return false;
            }

            if (Core.States.InGameStateObject.CurrentAreaInstance.Player.TryGetComponent<Life>(out var lifeComp)) {
                if (lifeComp.Health.Current <= 0) {
                    debugMessage = "Player is dead.";
                    return false;
                }
            }
            else {
                debugMessage = "Can not find player Life component.";
                return false;
            }

            if (Core.States.InGameStateObject.CurrentAreaInstance.Player.TryGetComponent<Buffs>(out var buffComp)) {
                if (buffComp.StatusEffects.ContainsKey("grace_period")) {
                    debugMessage = "Player has Grace Period.";
                    return false;
                }
            }
            else {
                debugMessage = "Can not find player Buffs component.";
                return false;
            }

            if (!Core.States.InGameStateObject.CurrentAreaInstance.Player.TryGetComponent<Actor>(out var _)) {
                debugMessage = "Can not find player Actor component.";
                return false;
            }

            debugMessage = "None";
            return true;
        }
    }
}