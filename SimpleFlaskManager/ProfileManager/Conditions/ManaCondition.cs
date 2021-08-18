// <copyright file="ManaCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using ImGuiNET;

    /// <summary>
    /// FlaskManager condition to trigger flask on Mana changes.
    /// </summary>
    public class ManaCondition
        : DecimalCondition
    {
        private static string name = "Mana";
        private static OperatorEnum op = OperatorEnum.BIGGER_THAN;
        private static int threshold = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManaCondition"/> class.
        /// </summary>
        /// <param name="op">Operator to perform on the <see cref="ManaCondition"/>.</param>
        /// <param name="threshold">threshold of <see cref="ManaCondition"/>.</param>
        public ManaCondition(OperatorEnum op, int threshold)
        : base(name, op, threshold)
        {
        }

        /// <summary>
        /// Draws the ImGui Widget for creating  <see cref="ManaCondition"/> class.
        /// </summary>
        /// <returns>
        /// <see cref="ManaCondition"/> if user allows it to be created otherwise null.
        /// </returns>
        public static ManaCondition AddConditionImGuiWidget()
        {
            ImGui.Text(name);
            ImGui.SameLine();
            if (ImGui.BeginCombo("##OperationMana", $"{op}"))
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
            ImGui.InputInt(" threshold##Mana", ref threshold);
            ImGui.SameLine();
            if (ImGui.Button("Add##Mana"))
            {
                return new ManaCondition(op, threshold);
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
                    OperatorEnum.BIGGER_THAN => lifeComponent.Mana.Current > this.value && this.EvaluateNext(),
                    OperatorEnum.LESS_THAN => lifeComponent.Mana.Current < this.value && this.EvaluateNext(),
                    _ => throw new Exception($"ManaCondition doesn't support {this.Operator}."),
                };
            }

            return false;
        }
    }
}
