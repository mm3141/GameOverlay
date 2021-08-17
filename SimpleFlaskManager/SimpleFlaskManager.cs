// <copyright file="SimpleFlaskManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager
{
    using GameHelper;
    using GameHelper.Plugin;
    using GameHelper.RemoteEnums;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// <see cref="SimpleFlaskManager"/> plugin.
    /// </summary>
    public sealed class SimpleFlaskManager : PCore<SimpleFlaskManagerSettings>
    {
        private string newProfileName = string.Empty;

        /// <inheritdoc/>
        public override void DrawSettings()
        {
            ImGui.Checkbox("Debug Mode", ref this.Settings.DebugMode);
            ImGui.Checkbox("Should Run In Hideout", ref this.Settings.ShouldRunInHideout);
            ImGui.InputText("Profile Name", ref this.newProfileName, 50);
            if (ImGui.Button("Add Profile"))
            {
                this.Settings.Profiles.Add(this.newProfileName, new Profile());
                this.newProfileName = string.Empty;
            }

            ImGui.NewLine();
            if (ImGui.BeginCombo("Select Profile", this.Settings.CurrentlySelectedProfile))
            {
                foreach (var profile in this.Settings.Profiles)
                {
                    bool selected = profile.Key == this.Settings.CurrentlySelectedProfile;
                    if (ImGui.IsWindowAppearing() && selected)
                    {
                        ImGui.SetScrollHereY();
                    }

                    if (ImGui.Selectable(profile.Key, selected))
                    {
                        this.Settings.CurrentlySelectedProfile = profile.Key;
                    }
                }

                ImGui.EndCombo();
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

            foreach (var rule in this.Settings.Profiles[this.Settings.CurrentlySelectedProfile].Rules)
            {
                if (rule.Condition.Evaluate())
                {
                    MiscHelper.KeyUp(rule.ActionKey);
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

            if (!this.Settings.Profiles.ContainsKey(this.Settings.CurrentlySelectedProfile))
            {
                debugMessage = $"{this.Settings.CurrentlySelectedProfile} not found.";
                return false;
            }

            return true;
        }
    }
}
