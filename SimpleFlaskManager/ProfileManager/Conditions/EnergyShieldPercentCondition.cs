// <copyright file="EnergyShieldPercentCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using ImGuiNET;

    /// <summary>
    /// FlaskManager condition to trigger flask on EnergyShield changes.
    /// </summary>
    public class EnergyShieldPercentCondition
        : DecimalCondition
    {
        private static string name = "EnergyShield Percent";
        private static OperatorEnum op = OperatorEnum.BIGGER_THAN;
        private static int threshold = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnergyShieldPercentCondition"/> class.
        /// </summary>
        /// <param name="op">Operator to perform on the <see cref="EnergyShieldPercentCondition"/>.</param>
        /// <param name="threshold">threshold of <see cref="EnergyShieldPercentCondition"/>.</param>
        public EnergyShieldPercentCondition(OperatorEnum op, int threshold)
        : base(name, op, threshold)
        {
        }

        /// <summary>
        /// Draws the ImGui Widget for creating  <see cref="EnergyShieldPercentCondition"/> class.
        /// </summary>
        /// <returns>
        /// <see cref="EnergyShieldPercentCondition"/> if user allows it to be created otherwise null.
        /// </returns>
        public static EnergyShieldPercentCondition AddConditionImGuiWidget()
        {
            ImGui.Text(name);
            ImGui.SameLine();
            if (ImGui.BeginCombo($"##Operation{name}", $"{op}"))
            {
                if (ImGui.Selectable($"{OperatorEnum.BIGGER_THAN}"))
                {
                    op = OperatorEnum.BIGGER_THAN;
                }

                if (ImGui.Selectable($"{OperatorEnum.LESS_THAN}"))
                {
                    op = OperatorEnum.LESS_THAN;
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();
            ImGui.InputInt($"threshold  ##{name}", ref threshold);
            ImGui.SameLine();
            if (ImGui.Button($"Add##{name}"))
            {
                return new EnergyShieldPercentCondition(op, threshold);
            }

            return null;
        }

        /// <inheritdoc/>
        public override bool Evaluate()
        {
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Life>(out var lifeComponent))
            {
                return this.Operator switch
                {
                    OperatorEnum.BIGGER_THAN => lifeComponent.EnergyShield.CurrentInPercent() > this.value,
                    OperatorEnum.LESS_THAN => lifeComponent.EnergyShield.CurrentInPercent() < this.value,
                    _ => throw new Exception($"{name}Condition doesn't support {this.Operator}."),
                }

                && this.EvaluateNext();
            }

            return false;
        }
    }
}
