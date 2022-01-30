// <copyright file="Profile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager
{
    using System.Collections.Generic;
    using System.Numerics;
    using GameHelper;
    using ImGuiNET;

    /// <summary>
    ///     A class containing the rules for drinking the flask.
    /// </summary>
    public class Profile
    {
        private int ruleIndexToDelete = -1;

        /// <summary>
        ///     Gets the rules to trigger the flasks on.
        /// </summary>
        public List<Rule> Rules { get; } = new();

        /// <summary>
        ///     A helper function to draw profile settings on ImGui window so that user can modify it.
        /// </summary>
        public void DrawSettings()
        {
            if (ImGui.BeginTabBar("Profile Rules", ImGuiTabBarFlags.AutoSelectNewTabs | ImGuiTabBarFlags.Reorderable))
            {
                if (ImGui.TabItemButton("+", ImGuiTabItemFlags.Leading))
                {
                    this.Rules.Add(new Rule(this.Rules.Count.ToString()));
                }

                for (var i = 0; i < this.Rules.Count; i++)
                {
                    var currRule = this.Rules[i];
                    var shouldNotDelete = true;
                    if (ImGui.BeginTabItem($"{currRule.Name}###Rule{i}", ref shouldNotDelete))
                    {
                        currRule.DrawSettings();
                        ImGui.EndTabItem();
                    }

                    if (!shouldNotDelete)
                    {
                        this.ruleIndexToDelete = i;
                        ImGui.OpenPopup("RuleDeleteConfirmation");
                    }
                }

                this.DrawConfirmationPopup();
                ImGui.EndTabBar();
            }
        }

        private void DrawConfirmationPopup()
        {
            ImGui.SetNextWindowPos(new Vector2(Core.Overlay.Size.X / 3f, Core.Overlay.Size.Y / 3f));
            if (ImGui.BeginPopup("RuleDeleteConfirmation"))
            {
                ImGui.Text($"Do you want to delete rule with name: {this.Rules[this.ruleIndexToDelete].Name}?");
                ImGui.Separator();
                if (ImGui.Button("Yes",
                    new Vector2(ImGui.GetContentRegionAvail().X / 2f, ImGui.GetTextLineHeight() * 2)))
                {
                    this.Rules.RemoveAt(this.ruleIndexToDelete);
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();
                if (ImGui.Button("No", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetTextLineHeight() * 2)))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }
    }
}