// <copyright file="Profile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using Conditions;
    using GameHelper;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    ///     A class containing the rules for drinking the flask.
    /// </summary>
    public class Profile
    {
        private ConditionHelper.ConditionEnum newConditionType;
        private int ruleIndexToDelete = -1;

        /// <summary>
        ///     Gets the rules to trigger the flasks on.
        /// </summary>
        public List<RuleStruct> Rules { get; } = new();

        /// <summary>
        ///     A helper function to draw profile settings on ImGui window
        ///     so that user can modify it.
        /// </summary>
        public void DrawSettings()
        {
            if (ImGui.Button("+"))
            {
                this.Rules.Add(RuleStruct.GetNewRule(this.Rules.Count));
            }

            ImGui.SameLine();
            if (ImGui.BeginTabBar("Profile Rules", ImGuiTabBarFlags.AutoSelectNewTabs | ImGuiTabBarFlags.Reorderable))
            {
                for (var i = 0; i < this.Rules.Count; i++)
                {
                    var currRule = this.Rules[i];
                    var shouldNotDelete = true;
                    if (ImGui.BeginTabItem($"{currRule.Name}###Rule{i}", ref shouldNotDelete))
                    {
                        if (ImGui.Checkbox($"Enable##{i}", ref currRule.Enable))
                        {
                            this.Rules[i] = currRule;
                        }

                        if (ImGui.InputText($"Name##{i}", ref currRule.Name, 20))
                        {
                            this.Rules[i] = currRule;
                        }

                        if (ImGuiHelper.NonContinuousEnumComboBox("Key", ref currRule.Key))
                        {
                            this.Rules[i] = currRule;
                        }

                        if (ImGui.TreeNodeEx("Add New condition", ImGuiTreeNodeFlags.NoTreePushOnOpen))
                        {
                            ImGuiHelper.EnumComboBox("Condition", ref this.newConditionType);
                            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 6);
                            ImGui.Separator();
                            var newCondition = ConditionHelper.EnumToObject(this.newConditionType);
                            if (newCondition != null)
                            {
                                if (currRule.Condition == null)
                                {
                                    currRule.Condition = newCondition;
                                }
                                else
                                {
                                    currRule.Condition.Append(newCondition);
                                }

                                this.Rules[i] = currRule;
                            }

                            ImGui.PopItemWidth();
                            ImGui.Separator();
                        }

                        if (ImGui.TreeNodeEx("Existing Conditions (all of them have to be true)",
                            ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 6);
                            this.Rules[i].Condition?.Display();
                            ImGui.PopItemWidth();
                        }

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
                    this.Rules[this.ruleIndexToDelete].Condition?.Delete();
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

        /// <summary>
        ///     A struct for storing condition and the key to press.
        /// </summary>
        public struct RuleStruct
        {
            /// <summary>
            ///     Enable/Disable the rule.
            /// </summary>
            public bool Enable;

            /// <summary>
            ///     User friendly name given to a rule.
            /// </summary>
            public string Name;

            /// <summary>
            ///     Rule condition to evaluate.
            /// </summary>
            public ICondition Condition;

            /// <summary>
            ///     Rule key to press on success.
            /// </summary>
            public ConsoleKey Key;

            /// <summary>
            ///     Helper function to create a new rule.
            /// </summary>
            /// <param name="ruleCounter">Rule number in the profile.</param>
            /// <returns>returns a new rule.</returns>
            public static RuleStruct GetNewRule(int ruleCounter)
            {
                var newrule = default(RuleStruct);
                newrule.Name = $"{ruleCounter}";
                return newrule;
            }
        }
    }
}