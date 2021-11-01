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

    /// <summary>
    /// For triggering a flask on player vitals changes.
    /// </summary>
    public class VitalsCondition
        : BaseCondition<int>
    {
        private static OperatorEnum operatorStatic = OperatorEnum.BIGGER_THAN;
        private static VitalsEnum vitalTypeStatic = VitalsEnum.MANA;
        private static int thresholdStatic = 0;

        [JsonProperty]
        private VitalsEnum vitalType;

        /// <summary>
        /// Initializes a new instance of the <see cref="VitalsCondition"/> class.
        /// </summary>
        /// <param name="operator_"><see cref="OperatorEnum"/> to use in this condition.</param>
        /// <param name="vital">Player vital type to use in this condition.</param>
        /// <param name="threshold">Vital threshold to use in this condition.</param>
        public VitalsCondition(OperatorEnum operator_, VitalsEnum vital, int threshold)
            : base(operator_, threshold)
        {
            this.vitalType = vital;
        }

        /// <summary>
        /// Different type of player Vitals.
        /// </summary>
        public enum VitalsEnum
        {
            /// <summary>
            /// Condition based on player Mana.
            /// </summary>
            MANA,

            /// <summary>
            /// Condition based on player Mana.
            /// </summary>
            MANA_PERCENT,

            /// <summary>
            /// Condition based on player Life.
            /// </summary>
            LIFE,

            /// <summary>
            /// Condition based on player Life.
            /// </summary>
            LIFE_PERCENT,

            /// <summary>
            /// Condition based on player Energy Shield.
            /// </summary>
            ENERGYSHIELD,

            /// <summary>
            /// Condition based on player Energy Shield.
            /// </summary>
            ENERGYSHIELD_PERCENT,
        }

        /// <summary>
        /// Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        /// <see cref="ICondition"/> if user wants to add the condition, otherwise null.
        /// </returns>
        public static new VitalsCondition Add()
        {
            ToImGui(ref operatorStatic, ref vitalTypeStatic, ref thresholdStatic);
            ImGui.SameLine();
            if (ImGui.Button($"Add##Vitals") &&
                (operatorStatic == OperatorEnum.BIGGER_THAN ||
                operatorStatic == OperatorEnum.LESS_THAN))
            {
                return new VitalsCondition(operatorStatic, vitalTypeStatic, thresholdStatic);
            }

            return null;
        }

        /// <inheritdoc/>
        public override void Display(int index = 0)
        {
            ToImGui(ref this.conditionOperator, ref this.vitalType, ref this.rightHandOperand);
            base.Display(index);
        }

        /// <inheritdoc/>
        public override bool Evaluate()
        {
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Life>(out var lifeComponent))
            {
                return this.conditionOperator switch
                {
                    OperatorEnum.BIGGER_THAN => this.GetVitalValue(lifeComponent) > this.rightHandOperand,
                    OperatorEnum.LESS_THAN => this.GetVitalValue(lifeComponent) < this.rightHandOperand,
                    _ => throw new Exception($"VitalCondition doesn't support {this.conditionOperator}."),
                }

                && this.EvaluateNext();
            }

            return false;
        }

        private static void ToImGui(ref OperatorEnum operator_, ref VitalsEnum vital, ref int threshold)
        {
            ImGui.Text($"Only {OperatorEnum.BIGGER_THAN} & {OperatorEnum.LESS_THAN} supported.");
            ImGui.Text($"Player");
            ImGui.SameLine();
            UiHelper.EnumComboBox("is##VitalSelector", ref vital);
            ImGui.SameLine();
            UiHelper.EnumComboBox("##VitalOperator", ref operator_);
            ImGui.SameLine();
            ImGui.InputInt("##VitalThreshold", ref threshold);
        }

        private int GetVitalValue(Life component)
        {
            return this.vitalType switch
            {
                VitalsEnum.MANA => component.Mana.Current,
                VitalsEnum.MANA_PERCENT => component.Mana.CurrentInPercent(),
                VitalsEnum.LIFE => component.Health.Current,
                VitalsEnum.LIFE_PERCENT => component.Health.CurrentInPercent(),
                VitalsEnum.ENERGYSHIELD => component.EnergyShield.Current,
                VitalsEnum.ENERGYSHIELD_PERCENT => component.EnergyShield.CurrentInPercent(),
                _ => throw new Exception($"Invalid Vital Type {this.vitalType}"),
            };
        }
    }
}
