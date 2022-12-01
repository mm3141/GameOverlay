// <copyright file="AutoHotKeyTriggerCore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger
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
    using ProfileManager.Conditions.DynamicCondition;

    /// <summary>
    ///     <see cref="AutoHotKeyTrigger" /> plugin.
    /// </summary>
    public sealed class AutoHotKeyTriggerCore : PCore<AutoHotKeyTriggerSettings>
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
            ImGui.TextColored(this.impTextColor, "Do not trust Settings.txt files for Auto Hokey Trigger from sources you have not personally verified. " + 
                              "They may contain malicious content that can compromise your computer. " +
                              "Using profiles with incorrectly configured rules may also lead to you being kicked from the server, " +
                              "or your account being banned as a result of preforming to many actions repeatedly.") ;
            ImGui.NewLine();
            ImGui.TextColored(this.impTextColor, "Again, all profiles/rules created to use a specified flask(s) should have at a minimum " +
                              "the FLASK_EFFECT and an appropriate number of FLASK_CHARGES defined as part of the use condition of a given profile rule. " +
                              "Failing to to include these two conditions as part of a rule will likely result in Auto Hotkey Trigger spamming the flask(s), " + 
                              "resulting in a possible kick or ban from the game servers because of sending to many actions to the server. " +
                              "You have been warrned, use common sense when creating profiles/rulse with this tool.");
            ImGui.NewLine();
            ImGui.PopTextWrapPos();
            ImGui.Checkbox("Debug Mode", ref this.Settings.DebugMode);
            ImGuiHelper.ToolTip("The debug mode may prove to be a helpful tool in troubleshooting Auto HotKey Trigger profile rules that are not preforming as expected. " +
                                "It can also be used to verify if AutoHotKeyTrigger is spamming the profile rule action or not based on the included conditions of a given profile rule. " +
                                "It is highly suggested to create and test all new profiles/rules with the debug mode turned on to insure that all rules are preforming as expected.");
            ImGui.NewLine();
            ImGuiHelper.NonContinuousEnumComboBox("Dump Player Status Effects",
                ref this.Settings.DumpStatusEffectOnMe);
            ImGuiHelper.ToolTip($"This hotkey will dump the current active player's buff(s), debuff(s) into a text file in the GameHelper -> Plugins -> " +
                                $"AutoHotKeyTrigger folder. Use this hotkey if the AutoHotKeyTrigger plugin fails to detect for example: " +
                                $"bleeds, corrupting blood, poison, freeze, ignites or other de(buffs) currently active on the character.");

            ImGui.NewLine();
            ImGui.Checkbox("Should Run In Hideout", ref this.Settings.ShouldRunInHideout);
            ImGuiHelper.IEnumerableComboBox("Profile", this.Settings.Profiles.Keys, ref this.Settings.CurrentProfile);
            ImGui.NewLine();
            if (ImGui.Button("Add/Reset and Activate League Start Default Profile"))
            {
                this.CreateDefaultProfile();
            }

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

            //separate update to allow settings to draw correctly, does not really hurt performance and only called when the settings window is open
            DynamicCondition.UpdateState();
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
                this.Settings.AutoQuitCondition.Display(true);
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
                ImGui.Begin("Debug Mode Window", ref this.Settings.DebugMode);
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

            DynamicCondition.UpdateState();
            if (this.ShouldExecuteAutoQuit || NativeMethods.IsKeyPressedAndNotTimeout(
                (int)this.Settings.AutoQuitKey)){
                MiscHelper.KillTCPConnectionForProcess(Core.Process.Pid);
            }

            if (NativeMethods.IsKeyPressedAndNotTimeout(
                (int)this.Settings.DumpStatusEffectOnMe))
            {
                if (Core.States.InGameStateObject.CurrentAreaInstance.Player.GetComp<Buffs>(out var buff))
                {
                    var data = string.Empty;
                    foreach (var statusEffect in buff.StatusEffects)
                    {
                        data += $"{statusEffect.Key} {statusEffect.Value}\n";
                    }

                    if (!string.IsNullOrEmpty(data))
                    {
                        File.AppendAllText(Path.Join(this.DllDirectory, "player_status_effect.txt"), data);
                    }
                }
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
            JsonDataHelper.FlaskNameToBuffGroups = JsonConvert.DeserializeObject<
                Dictionary<string, List<string>>>(jsonData);

            var jsonData2 = File.ReadAllText(this.DllDirectory + @"/StatusEffectGroup.json");
            JsonDataHelper.StatusEffectGroups = JsonConvert.DeserializeObject<
                Dictionary<string, List<string>>>(jsonData2);

            if (File.Exists(this.SettingPathname))
            {
                var content = File.ReadAllText(this.SettingPathname);
                this.Settings = JsonConvert.DeserializeObject<AutoHotKeyTriggerSettings>(
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

            var areaDetails = Core.States.InGameStateObject.CurrentWorldInstance.AreaDetails;
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

            if (Core.States.InGameStateObject.CurrentAreaInstance.Player.GetComp<Life>(out var lifeComp))
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

            if (Core.States.InGameStateObject.CurrentAreaInstance.Player.GetComp<Buffs>(out var buffComp))
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

            if (!Core.States.InGameStateObject.CurrentAreaInstance.Player.GetComp<Actor>(out var _))
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

            this.Settings.Profiles["LeagueStartDefaultProfile"] = profile;
            this.Settings.CurrentProfile = "LeagueStartDefaultProfile";

        }

        private void AutoQuitWarningUi()
        {

            if (!this.stopShowingAutoQuitWarning &&
                Core.States.InGameStateObject.CurrentWorldInstance.AreaDetails.IsTown &&
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