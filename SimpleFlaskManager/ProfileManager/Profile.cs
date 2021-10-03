// <copyright file="Profile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager
{
    using System;
    using System.Collections.Generic;
    using GameHelper.Utils;
    using ImGuiNET;
    using SimpleFlaskManager.ProfileManager.Conditions;

    /// <summary>
    /// A class containing the rules for drinking the flask.
    /// </summary>
    public class Profile
    {
        private ConditionHelper.ConditionEnum newConditionType = default;
        private ConsoleKey newKey = ConsoleKey.D0;
        private int index = -1;

        /// <summary>
        /// Gets the rules to trigger the flasks on.
        /// </summary>
        public List<RuleStruct> Rules { get; private set; }
            = new List<RuleStruct>();

        /// <summary>
        /// A helper function to draw profile settings on ImGui window
        /// so that user can modify it.
        /// </summary>
        public void DrawSettings()
        {
            if (ImGui.Button("+"))
            {
                this.Rules.Add(default(RuleStruct));
            }

            ImGui.SameLine();
            if (ImGui.BeginTabBar("Profile Rules", ImGuiTabBarFlags.AutoSelectNewTabs | ImGuiTabBarFlags.Reorderable))
            {
                for (int i = 0; i < this.Rules.Count; i++)
                {
                    var currRule = this.Rules[i];
                    bool shouldNotDelete = true;
                    if (ImGui.BeginTabItem($"Rule {i}", ref shouldNotDelete))
                    {
                        if (UiHelper.NonContinuousEnumComboBox("Key", ref currRule.Key))
                        {
                            this.Rules[i] = currRule;
                        }

                        if (ImGui.TreeNodeEx("Add New condition", ImGuiTreeNodeFlags.NoTreePushOnOpen))
                        {
                            UiHelper.EnumComboBox("Condition", ref this.newConditionType);
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

                        if (ImGui.TreeNodeEx("Existing Conditions (all of them have to be true)", ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 6);
                            this.Rules[i].Condition?.Display();
                            ImGui.PopItemWidth();
                        }

                        ImGui.EndTabItem();
                    }

                    if (!shouldNotDelete)
                    {
                        this.Rules[i].Condition?.Delete();
                        this.Rules.RemoveAt(i);
                    }
                }

                ImGui.EndTabBar();
            }
        }

        /// <summary>
        /// A struct for storing condition and the key to press.
        /// </summary>
        public struct RuleStruct
        {
            /// <summary>
            /// Rule condition to evaluate.
            /// </summary>
            public ICondition Condition;

            /// <summary>
            /// Rule key to press on success.
            /// </summary>
            public ConsoleKey Key;
        }
    }
}