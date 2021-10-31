// <copyright file="StatusEffectCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions {
    using System;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;

    /// <summary>
    ///     For triggering a flask on player Status Effect changes.
    /// </summary>
    public class StatusEffectCondition
        : BaseCondition<string> {
        private static OperatorEnum operatorStatic = OperatorEnum.CONTAINS;
        private static string buffIdStatic = string.Empty;
        private static int buffChargesStatic;

        [JsonProperty] private int charges;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StatusEffectCondition" /> class.
        /// </summary>
        /// <param name="operator_"><see cref="OperatorEnum" /> to use in this condition.</param>
        /// <param name="buffId">Player buff/debuff to use in this condition.</param>
        /// <param name="buffCharges">Number of charges the buff/debuff has.</param>
        public StatusEffectCondition(OperatorEnum operator_, string buffId, int buffCharges)
            : base(operator_, buffId) {
            charges = buffCharges;
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public new static StatusEffectCondition Add() {
            ToImGui(ref operatorStatic, ref buffIdStatic, ref buffChargesStatic);
            ImGui.SameLine();
            if (ImGui.Button("Add##StatusEffect") &&
                (operatorStatic == OperatorEnum.CONTAINS ||
                 operatorStatic == OperatorEnum.NOT_CONTAINS ||
                 operatorStatic == OperatorEnum.BIGGER_THAN ||
                 operatorStatic == OperatorEnum.LESS_THAN)) {
                return new StatusEffectCondition(operatorStatic, buffIdStatic, buffChargesStatic);
            }

            return null;
        }

        /// <inheritdoc />
        public override void Display(int index = 0) {
            ToImGui(ref conditionOperator, ref rightHandOperand, ref charges);
            base.Display(index);
        }

        /// <inheritdoc />
        public override bool Evaluate() {
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Buffs>(out var buffComponent)) {
                return conditionOperator switch {
                           OperatorEnum.CONTAINS => buffComponent.StatusEffects.ContainsKey(rightHandOperand),
                           OperatorEnum.NOT_CONTAINS => !buffComponent.StatusEffects.ContainsKey(rightHandOperand),
                           OperatorEnum.BIGGER_THAN => buffComponent.StatusEffects.ContainsKey(rightHandOperand) &&
                                                       buffComponent.StatusEffects[rightHandOperand].Charges > charges,
                           OperatorEnum.LESS_THAN => buffComponent.StatusEffects.ContainsKey(rightHandOperand) &&
                                                     buffComponent.StatusEffects[rightHandOperand].Charges < charges,
                           _ => throw new Exception($"BuffCondition doesn't support {conditionOperator}.")
                       }
                       && EvaluateNext();
            }

            return false;
        }

        private static void ToImGui(ref OperatorEnum operator_, ref string buffId, ref int charges) {
            ImGui.TextWrapped($"{OperatorEnum.CONTAINS}, {OperatorEnum.NOT_CONTAINS}, " +
                              $"{OperatorEnum.BIGGER_THAN} and {OperatorEnum.LESS_THAN} are supported." +
                              $"{OperatorEnum.CONTAINS} and {OperatorEnum.NOT_CONTAINS} just checks if " +
                              $"buff/debuff is there or not. {OperatorEnum.BIGGER_THAN} and " +
                              $"{OperatorEnum.LESS_THAN} checks if buff/debuff is there and charges " +
                              "are valid as well.");
            ImGui.Text("Player");
            ImGui.SameLine();
            UiHelper.EnumComboBox("##StatusEffectOperator", ref operator_);
            ImGui.SameLine();
            ImGui.InputText("(de)buff with##StatusEffect", ref buffId, 200);
            ImGui.SameLine();
            ImGui.InputInt("charges##StatusEffect", ref charges);
        }
    }
}