// <copyright file="LifeCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using ImGuiNET;

    /// <summary>
    /// FlaskManager condition to trigger flask on Life changes.
    /// </summary>
    public class LifeCondition
        : DecimalCondition
    {
        private static string name = "Life";
        private static OperatorEnum op = OperatorEnum.BIGGER_THAN;
        private static int threshold = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="LifeCondition"/> class.
        /// </summary>
        /// <param name="op">Operator to perform on the <see cref="LifeCondition"/>.</param>
        /// <param name="threshold">threshold of <see cref="LifeCondition"/>.</param>
        public LifeCondition(OperatorEnum op, int threshold)
        : base(name, op, threshold)
        {
        }

        /// <summary>
        /// Draws the ImGui Widget for creating  <see cref="LifeCondition"/> class.
        /// </summary>
        /// <returns>
        /// <see cref="LifeCondition"/> if user allows it to be created otherwise null.
        /// </returns>
        public static LifeCondition AddConditionImGuiWidget()
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
                return new LifeCondition(op, threshold);
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
                    OperatorEnum.BIGGER_THAN => lifeComponent.Health.Current > this.value && this.EvaluateNext(),
                    OperatorEnum.LESS_THAN => lifeComponent.Health.Current < this.value && this.EvaluateNext(),
                    _ => throw new Exception($"{name}Condition doesn't support {this.Operator}."),
                };
            }

            return false;
        }
    }
}
