// <copyright file="StatusEffectCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

﻿namespace AutoHotKeyTrigger.ProfileManager.Conditions
{
    using System;
    using AutoHotKeyTrigger.ProfileManager.Component;
    using Enums;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using GameOffsets.Objects.Components;
    using ImGuiNET;
    using Newtonsoft.Json;

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
            = new(OperatorType.BIGGER_THAN, "", 1, StatusEffectCheckType.CHARGES);

        [JsonProperty] private string buffId;
        [JsonProperty] private StatusEffectCheckType checkType;
        [JsonProperty] private OperatorType @operator;
        [JsonProperty] private float threshold;
        [JsonProperty] private IComponent component;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StatusEffectCondition" /> class.
        /// </summary>
        /// <param name="operator"><see cref="OperatorType" /> to use in this condition.</param>
        /// <param name="buffId">Player buff/debuff to use in this condition.</param>
        /// <param name="threshold"></param>
        /// <param name="checkType">Type of buff value to check</param>
        public StatusEffectCondition(OperatorType @operator, string buffId, float threshold, StatusEffectCheckType checkType)
        {
            this.@operator = @operator;
            this.buffId = buffId;
            this.threshold = threshold;
            this.checkType = checkType;
            this.component = null;
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
        public void Display(bool expand)
        {
            this.ToImGui(expand);
            this.component?.Display(expand);
        }

        /// <inheritdoc/>
        public void Add(IComponent component)
        {
            this.component = component;
        }

        /// <inheritdoc />
        public bool Evaluate()
        {
            var isConditionValid = false;
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.GetComp<Buffs>(out var buffComponent))
            {
                var exists = buffComponent.StatusEffects.TryGetValue(this.buffId, out var buff);
                isConditionValid = this.@operator switch
                {
                    OperatorType.BIGGER_THAN => (exists ? this.GetValue(buff) : 0f) > this.threshold,
                    OperatorType.LESS_THAN => (exists ? this.GetValue(buff) : 0f) < this.threshold,
                    OperatorType.CONTAINS => exists,
                    OperatorType.NOT_CONTAINS => !exists,
                    _ => throw new Exception($"BuffCondition doesn't support {this.@operator}.")
                };
            }

            return this.component == null ? isConditionValid : this.component.execute(isConditionValid);
        }

        private float GetValue(StatusEffectStruct buffDetails)
        {
            return this.checkType switch
            {
                StatusEffectCheckType.CHARGES => buffDetails.Charges,
                StatusEffectCheckType.DURATION => buffDetails.TimeLeft,
                StatusEffectCheckType.DURATION_PERCENT => float.IsInfinity(buffDetails.TimeLeft) ? 100f :
                                              (buffDetails.TimeLeft / buffDetails.TotalTime) * 100,
                _ => throw new Exception($"Invalid check type {this.checkType}")
            };
        }

        private void ToImGui(bool expand = true)
        {
            if (!expand)
            {
                if (this.@operator != OperatorType.CONTAINS && this.@operator != OperatorType.NOT_CONTAINS)
                {
                    ImGui.Text("Player has");
                    ImGui.SameLine();
                    ImGui.TextColored(new System.Numerics.Vector4(255, 255, 0, 255), $"{this.buffId}");
                    ImGui.SameLine();
                    if (this.@operator == OperatorType.BIGGER_THAN)
                    {
                        ImGui.Text("(de)buff with more than");
                    }
                    else
                    {
                        ImGui.Text("(de)buff with less than");
                    }

                    ImGui.SameLine();
                    ImGui.TextColored(new System.Numerics.Vector4(255, 255, 0, 255), $"{this.threshold}");
                    ImGui.SameLine();
                    ImGui.Text($"{this.checkType}");
                }
                else
                {
                    ImGui.Text("Player");
                    ImGui.SameLine();
                    if (this.@operator == OperatorType.NOT_CONTAINS)
                    {
                        ImGui.Text("does not have");
                    }
                    else
                    {
                        ImGui.Text("has");
                    }

                    ImGui.SameLine();
                    ImGui.TextColored(new System.Numerics.Vector4(255, 255, 0, 255), $"{this.buffId}");
                    ImGui.SameLine();
                    ImGui.Text("(de)buff");
                }

                return;
            }

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
                ImGuiHelper.ToolTip($"What to compare. {StatusEffectCheckType.DURATION_PERCENT} ranges from " +
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
    }
}
