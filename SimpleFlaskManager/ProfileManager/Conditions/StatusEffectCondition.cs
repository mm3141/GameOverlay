// <copyright file="StatusEffectCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// For triggering a flask on player Status Effect changes.
    /// </summary>
    public class StatusEffectCondition
        : BaseCondition<string>
    {
        private static OperatorEnum operatorStatic = OperatorEnum.CONTAINS;
        private static string buffIdStatic = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusEffectCondition"/> class.
        /// </summary>
        /// <param name="operator_"><see cref="OperatorEnum"/> to use in this condition.</param>
        /// <param name="buffId">Player buff/debuff to use in this condition.</param>
        public StatusEffectCondition(OperatorEnum operator_, string buffId)
        : base(operator_, buffId)
        {
        }

        /// <summary>
        /// Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        /// <see cref="ICondition"/> if user wants to add the condition, otherwise null.
        /// </returns>
        public static new StatusEffectCondition Add()
        {
            ToImGui(ref operatorStatic, ref buffIdStatic);
            ImGui.SameLine();
            if (ImGui.Button($"Add##StatusEffect") &&
                (operatorStatic == OperatorEnum.CONTAINS ||
                operatorStatic == OperatorEnum.NOT_CONTAINS))
            {
                return new StatusEffectCondition(operatorStatic, buffIdStatic);
            }

            return null;
        }

        /// <inheritdoc/>
        public override void Display()
        {
            ToImGui(ref this.conditionOperator, ref this.rightHandOperand);
            base.Display();
        }

        /// <inheritdoc/>
        public override bool Evaluate()
        {
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Buffs>(out var buffComponent))
            {
                return this.conditionOperator switch
                {
                    OperatorEnum.CONTAINS => buffComponent.StatusEffects.ContainsKey(this.rightHandOperand),
                    OperatorEnum.NOT_CONTAINS => !buffComponent.StatusEffects.ContainsKey(this.rightHandOperand),
                    _ => throw new Exception($"BuffCondition doesn't support {this.conditionOperator}."),
                }

                && this.EvaluateNext();
            }

            return false;
        }

        private static void ToImGui(ref OperatorEnum operator_, ref string buffId)
        {
            ImGui.Text($"Only {OperatorEnum.CONTAINS} & {OperatorEnum.NOT_CONTAINS} supported.");
            ImGui.Text($"Player");
            ImGui.SameLine();
            UiHelper.EnumComboBox("##StatusEffectOperator", ref operator_);
            ImGui.SameLine();
            ImGui.InputText("buff-id/debuff-id##StatusEffect", ref buffId, 200);
        }
    }
}
