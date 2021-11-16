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
    using Newtonsoft.Json;
    using SimpleFlaskManager.ProfileManager.Enums;

    /// <summary>
    ///     For triggering a flask on player Status Effect changes.
    /// </summary>
    public class StatusEffectCondition
        : BaseCondition<string>
    {
        private static OperatorType operatorStatic = OperatorType.CONTAINS;
        private static string buffIdStatic = string.Empty;
        private static int buffChargesStatic;

        [JsonProperty] private int charges;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StatusEffectCondition" /> class.
        /// </summary>
        /// <param name="operator_"><see cref="OperatorType" /> to use in this condition.</param>
        /// <param name="buffId">Player buff/debuff to use in this condition.</param>
        /// <param name="buffCharges">Number of charges the buff/debuff has.</param>
        public StatusEffectCondition(OperatorType operator_, string buffId, int buffCharges)
            : base(operator_, buffId)
        {
            this.charges = buffCharges;
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public new static StatusEffectCondition Add()
        {
            ToImGui(ref operatorStatic, ref buffIdStatic, ref buffChargesStatic);
            ImGui.SameLine();
            if (ImGui.Button("Add##StatusEffect") &&
                (operatorStatic == OperatorType.CONTAINS ||
                 operatorStatic == OperatorType.NOT_CONTAINS ||
                 operatorStatic == OperatorType.BIGGER_THAN ||
                 operatorStatic == OperatorType.LESS_THAN))
            {
                return new StatusEffectCondition(operatorStatic, buffIdStatic, buffChargesStatic);
            }

            return null;
        }

        /// <inheritdoc />
        public override void Display(int index = 0)
        {
            ToImGui(ref this.conditionOperator, ref this.rightHandOperand, ref this.charges);
            base.Display(index);
        }

        /// <inheritdoc />
        public override bool Evaluate()
        {
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Buffs>(out var buffComponent))
            {
                return this.conditionOperator switch
                       {
                           OperatorType.CONTAINS => buffComponent.StatusEffects.ContainsKey(this.rightHandOperand),
                           OperatorType.NOT_CONTAINS => !buffComponent.StatusEffects.ContainsKey(this.rightHandOperand),
                           OperatorType.BIGGER_THAN => buffComponent.StatusEffects.ContainsKey(this.rightHandOperand) &&
                                                       buffComponent.StatusEffects[this.rightHandOperand].Charges >
                                                       this.charges,
                           OperatorType.LESS_THAN => buffComponent.StatusEffects.ContainsKey(this.rightHandOperand) &&
                                                     buffComponent.StatusEffects[this.rightHandOperand].Charges <
                                                     this.charges,
                           _ => throw new Exception($"BuffCondition doesn't support {this.conditionOperator}.")
                       };
            }

            return false;
        }

        private static void ToImGui(ref OperatorType operator_, ref string buffId, ref int charges)
        {
            ImGui.TextWrapped($"{OperatorType.CONTAINS}, {OperatorType.NOT_CONTAINS}, " +
                              $"{OperatorType.BIGGER_THAN} and {OperatorType.LESS_THAN} are supported." +
                              $"{OperatorType.CONTAINS} and {OperatorType.NOT_CONTAINS} just checks if " +
                              $"buff/debuff is there or not. {OperatorType.BIGGER_THAN} and " +
                              $"{OperatorType.LESS_THAN} checks if buff/debuff is there and charges " +
                              "are valid as well.");
            ImGui.Text("Player");
            ImGui.SameLine();
            ImGuiHelper.EnumComboBox("##StatusEffectOperator", ref operator_);
            ImGui.SameLine();
            ImGui.InputText("(de)buff with##StatusEffect", ref buffId, 200);
            ImGui.SameLine();
            ImGui.InputInt("charges##StatusEffect", ref charges);
        }
    }
}