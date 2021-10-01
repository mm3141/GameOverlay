﻿// <copyright file="Profile.cs" company="PlaceholderCompany">
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
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 4);
            ImGui.TextWrapped("Index -1 means add condition as a new Rule. " +
                "Index greater than -1 means append condition to the existing rule.");
            ImGui.InputInt("Index", ref this.index, 1, 1);
            UiHelper.NonContinuousEnumComboBox("Key", ref this.newKey);
            UiHelper.EnumComboBox("Condition", ref this.newConditionType);
            ImGui.Separator();
            var newCondition = ConditionHelper.EnumToObject(this.newConditionType);
            ImGui.Separator();
            if (newCondition != null)
            {
                if (this.index == -1)
                {
                    this.Rules.Add(new RuleStruct() { Condition = newCondition, Key = this.newKey });
                }
                else if (this.index < this.Rules.Count)
                {
                    this.Rules[this.index].Condition.Append(newCondition);
                }
            }

            if (ImGui.TreeNode("Rules"))
            {
                for (int i = 0; i < this.Rules.Count; i++)
                {
                    ImGui.Text($"Rule: {i}, Key: {this.Rules[i].Key}");
                    ImGui.SameLine();
                    if (ImGui.SmallButton($"Delete Rule##{i}"))
                    {
                        this.Rules[i].Condition.Delete();
                        this.Rules.RemoveAt(i);
                        continue;
                    }

                    if (ImGui.TreeNode($"Conditions (all of them have to be true)##{i}"))
                    {
                        ImGui.Separator();
                        this.Rules[i].Condition.Display();
                        ImGui.TreePop();
                    }
                }

                ImGui.TreePop();
            }

            ImGui.PopItemWidth();
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