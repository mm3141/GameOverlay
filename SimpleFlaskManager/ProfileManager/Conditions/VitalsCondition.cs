// <copyright file="VitalsCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using SimpleFlaskManager.ProfileManager.Enums;

    /// <summary>
    ///     For triggering a flask on player vitals changes.
    /// </summary>
    public class VitalsCondition
        : BaseCondition<int>
    {
        private static OperatorType operatorStatic = OperatorType.BIGGER_THAN;
        private static VitalType vitalTypeStatic = VitalType.MANA;
        private static int thresholdStatic;

        [JsonProperty] private VitalType vitalType;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VitalsCondition" /> class.
        /// </summary>
        /// <param name="operator_"><see cref="OperatorType" /> to use in this condition.</param>
        /// <param name="vital">Player vital type to use in this condition.</param>
        /// <param name="threshold">Vital threshold to use in this condition.</param>
        public VitalsCondition(OperatorType operator_, VitalType vital, int threshold)
            : base(operator_, threshold)
        {
            this.vitalType = vital;
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public new static VitalsCondition Add()
        {
            ToImGui(ref operatorStatic, ref vitalTypeStatic, ref thresholdStatic);
            ImGui.SameLine();
            if (ImGui.Button("Add##Vitals") &&
                (operatorStatic == OperatorType.BIGGER_THAN ||
                 operatorStatic == OperatorType.LESS_THAN))
            {
                return new VitalsCondition(operatorStatic, vitalTypeStatic, thresholdStatic);
            }

            return null;
        }

        /// <inheritdoc />
        public override void Display(int index = 0)
        {
            ToImGui(ref this.conditionOperator, ref this.vitalType, ref this.rightHandOperand);
            base.Display(index);
        }

        /// <inheritdoc />
        public override bool Evaluate()
        {
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Life>(out var lifeComponent))
            {
                return this.conditionOperator switch
                       {
                           OperatorType.BIGGER_THAN => this.GetVitalValue(lifeComponent) > this.rightHandOperand,
                           OperatorType.LESS_THAN => this.GetVitalValue(lifeComponent) < this.rightHandOperand,
                           _ => throw new Exception($"VitalCondition doesn't support {this.conditionOperator}.")
                       };
            }

            return false;
        }

        private static void ToImGui(ref OperatorType operator_, ref VitalType vital, ref int threshold)
        {
            ImGui.Text($"Only {OperatorType.BIGGER_THAN} & {OperatorType.LESS_THAN} supported.");
            ImGui.Text("Player");
            ImGui.SameLine();
            ImGuiHelper.EnumComboBox("is##VitalSelector", ref vital);
            ImGui.SameLine();
            ImGuiHelper.EnumComboBox("##VitalOperator", ref operator_);
            ImGui.SameLine();
            ImGui.InputInt("##VitalThreshold", ref threshold);
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