// <copyright file="StatusEffectCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

﻿namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using Enums;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using GameOffsets.Objects.Components;
    using ImGuiNET;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    ///     For triggering a flask on player Status Effect duration/charges.
    /// </summary>
    public class StatusEffectCondition : ICondition
    {
        private static readonly OperatorType[] SupportedOperatorTypes =
        {
            OperatorType.BIGGER_THAN,
            OperatorType.LESS_THAN,
            OperatorType.CONTAINS,
            OperatorType.NOT_CONTAINS,
        };

        private static readonly StatusEffectCondition ConfigurationInstance
            = new(OperatorType.BIGGER_THAN, "", 1, CheckType.CHARGES);

        [JsonProperty] private string buffId;
        [JsonProperty] private CheckType checkType;
        [JsonProperty] private OperatorType @operator;
        [JsonProperty] private float threshold;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StatusEffectCondition" /> class.
        /// </summary>
        /// <param name="operator"><see cref="OperatorType" /> to use in this condition.</param>
        /// <param name="buffId">Player buff/debuff to use in this condition.</param>
        /// <param name="threshold"></param>
        /// <param name="checkType">Type of buff value to check</param>
        public StatusEffectCondition(OperatorType @operator, string buffId, float threshold, CheckType checkType)
        {
            this.@operator = @operator;
            this.buffId = buffId;
            this.threshold = threshold;
            this.checkType = checkType;
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public static StatusEffectCondition Add()
        {
            ConfigurationInstance.ToImGui();
            ImGui.SameLine();
            if (ImGui.Button("Add##StatusEffect"))
            {
                return new StatusEffectCondition(
                    ConfigurationInstance.@operator,
                    ConfigurationInstance.buffId,
                    ConfigurationInstance.threshold,
                    ConfigurationInstance.checkType);
            }

            return null;
        }

        /// <inheritdoc />
        public void Display()
        {
            this.ToImGui();
        }

        /// <inheritdoc />
        public bool Evaluate()
        {
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Buffs>(out var buffComponent))
            {
                var exists = buffComponent.StatusEffects.TryGetValue(this.buffId, out var buff);
                return this.@operator switch
                {
                    OperatorType.BIGGER_THAN => (exists ? this.GetValue(buff) : 0f) > this.threshold,
                    OperatorType.LESS_THAN => (exists ? this.GetValue(buff) : 0f) < this.threshold,
                    OperatorType.CONTAINS => exists,
                    OperatorType.NOT_CONTAINS => !exists,
                    _ => throw new Exception($"BuffCondition doesn't support {this.@operator}.")
                };
            }

            return false;
        }

        private float GetValue(StatusEffectStruct buffDetails)
        {
            return this.checkType switch
            {
                CheckType.CHARGES => buffDetails.Charges,
                CheckType.DURATION => buffDetails.TimeLeft,
                CheckType.DURATION_PERCENT => float.IsInfinity(buffDetails.TimeLeft) ? 100f :
                                              (buffDetails.TimeLeft / buffDetails.TotalTime) * 100,
                _ => throw new Exception($"Invalid check type {this.checkType}")
            };
        }

        private void ToImGui()
        {
            ImGui.PushID("StatusEffectDuration");
            if (this.@operator != OperatorType.CONTAINS && this.@operator != OperatorType.NOT_CONTAINS)
            {
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 7);
                ImGui.Text("Player has (de)buff");
                ImGui.SameLine();
                ImGui.InputText("with", ref this.buffId, 200);
                ImGui.SameLine();
                ImGuiHelper.EnumComboBox("##comparison", ref this.@operator, SupportedOperatorTypes);
                ImGui.SameLine();
                ImGui.InputFloat("##threshold", ref this.threshold);
                ImGui.SameLine();
                ImGuiHelper.EnumComboBox("##checkType", ref this.checkType);
                ImGuiHelper.ToolTip($"What to compare. {CheckType.DURATION_PERCENT} ranges from " +
                    $"0 to 100, 0 being buff will expire imminently and 100 meaning " +
                    $"it was just applied");
                ImGui.PopItemWidth();
            }
            else
            {
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 3);
                ImGui.Text("Player");
                ImGui.SameLine();
                ImGuiHelper.EnumComboBox("##comparison", ref this.@operator, SupportedOperatorTypes);
                ImGui.SameLine();
                ImGui.InputText("(de)buff", ref this.buffId, 200);
                ImGui.PopItemWidth();
            }

            ImGui.PopID();
        }

        /// <summary>
        /// Check type for the condition
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CheckType
        {
            /// <summary>
            /// Check remaining buff duration
            /// </summary>
            DURATION,

            /// <summary>
            /// Check remaning buff duration in percent
            /// </summary>
            DURATION_PERCENT,

            /// <summary>
            /// Check buff charges
            /// </summary>
            CHARGES
        }
    }
}
