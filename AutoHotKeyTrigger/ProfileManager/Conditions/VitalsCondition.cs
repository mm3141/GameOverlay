// <copyright file="VitalsCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Conditions
{
    using System;
    using System.Linq;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using Enums;
    using AutoHotKeyTrigger.ProfileManager.Component;

    /// <summary>
    ///     For triggering a flask on player vitals changes.
    /// </summary>
    public class VitalsCondition : ICondition
    {
        private static readonly OperatorType[] SupportedOperatorTypes = { OperatorType.BIGGER_THAN, OperatorType.LESS_THAN };
        private static readonly VitalsCondition ConfigurationInstance = new(OperatorType.BIGGER_THAN, VitalType.MANA, 0);

        [JsonProperty] private OperatorType @operator;
        [JsonProperty] private VitalType vitalType;
        [JsonProperty] private int threshold;
        [JsonProperty] private IComponent component;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VitalsCondition" /> class.
        /// </summary>
        /// <param name="operator"><see cref="OperatorType" /> to use in this condition.</param>
        /// <param name="vital">Player vital type to use in this condition.</param>
        /// <param name="threshold">Vital threshold to use in this condition.</param>
        public VitalsCondition(OperatorType @operator, VitalType vital, int threshold)
        {
            this.@operator = @operator;
            this.vitalType = vital;
            this.threshold = threshold;
            this.component = null;
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public static VitalsCondition Add()
        {
            ConfigurationInstance.ToImGui();
            ImGui.SameLine();
            if (ImGui.Button("Add##Vitals"))
            {
                return new VitalsCondition(
                    ConfigurationInstance.@operator,
                    ConfigurationInstance.vitalType,
                    ConfigurationInstance.threshold);
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
            if (player.GetComp<Life>(out var lifeComponent))
            {
                isConditionValid = this.@operator switch
                {
                    OperatorType.BIGGER_THAN => this.GetVitalValue(lifeComponent) > this.threshold,
                    OperatorType.LESS_THAN => this.GetVitalValue(lifeComponent) < this.threshold,
                    _ => throw new Exception($"VitalCondition doesn't support {this.@operator}.")
                };
            }

            return this.component == null ? isConditionValid : this.component.execute(isConditionValid);
        }

        private void ToImGui(bool expand = true)
        {
            ImGui.Text("Player");
            ImGui.SameLine();
            if (expand)
            {
                ImGuiHelper.EnumComboBox("is##VitalSelector", ref this.vitalType);
                ImGui.SameLine();
                ImGuiHelper.EnumComboBox("##VitalOperator", ref this.@operator, SupportedOperatorTypes);
                ImGui.SameLine();
                ImGui.InputInt("##VitalThreshold", ref this.threshold);
            }
            else
            {
                ImGui.TextColored(new System.Numerics.Vector4(255, 255, 0, 255),$"{this.vitalType}");
                ImGui.SameLine();
                if (this.@operator == OperatorType.BIGGER_THAN)
                {
                    ImGui.Text("is more than");
                }
                else
                {
                    ImGui.Text("is less than");
                }

                ImGui.SameLine();
                ImGui.TextColored(new System.Numerics.Vector4(255, 255, 0, 255), $"{this.threshold}");
            }
        }

        private int GetVitalValue(Life component)
        {
            return this.vitalType switch
            {
                VitalType.MANA => component.Mana.Current,
                VitalType.MANA_PERCENT => component.Mana.CurrentInPercent(),
                VitalType.LIFE => component.Health.Current,
                VitalType.LIFE_PERCENT => component.Health.CurrentInPercent(),
                VitalType.ENERGYSHIELD => component.EnergyShield.Current,
                VitalType.ENERGYSHIELD_PERCENT => component.EnergyShield.CurrentInPercent(),
                _ => throw new Exception($"Invalid Vital Type {this.vitalType}")
            };
        }
    }
}