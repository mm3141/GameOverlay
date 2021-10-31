// <copyright file="AnimationCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions {
    using System;
    using System.Diagnostics;
    using GameHelper;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;

    /// <summary>
    ///     For triggering a flask on player animation changes.
    /// </summary>
    public class AnimationCondition
        : BaseCondition<Animation> {
        private static OperatorEnum operatorStatic = OperatorEnum.EQUAL_TO;
        private static Animation selectedStatic = 0;
        private static int durationMsStatic;
        private readonly Stopwatch sw;

        [JsonProperty] private int durationMs;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AnimationCondition" /> class.
        /// </summary>
        /// <param name="operator_"><see cref="OperatorEnum" /> to use in this condition.</param>
        /// <param name="animation">player animation to use for this condition.</param>
        /// <param name="duration">duration (ms) for which the animation is active.</param>
        public AnimationCondition(OperatorEnum operator_, Animation animation, int duration)
            : base(operator_, animation) {
            durationMs = duration;
            sw = Stopwatch.StartNew();
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public new static AnimationCondition Add() {
            ToImGui(ref operatorStatic, ref selectedStatic, ref durationMsStatic);
            ImGui.SameLine();
            if (ImGui.Button("Add##Animation") &&
                (operatorStatic == OperatorEnum.EQUAL_TO ||
                 operatorStatic == OperatorEnum.NOT_EQUAL_TO)) {
                return new AnimationCondition(operatorStatic, selectedStatic, durationMsStatic);
            }

            return null;
        }

        /// <inheritdoc />
        public override void Display(int index = 0) {
            ToImGui(ref conditionOperator, ref rightHandOperand, ref durationMs);
            base.Display(index);
        }

        /// <inheritdoc />
        public override bool Evaluate() {
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Actor>(out var actorComponent)) {
                var isConditionValid = conditionOperator switch {
                    OperatorEnum.EQUAL_TO => actorComponent.Animation == rightHandOperand,
                    OperatorEnum.NOT_EQUAL_TO => actorComponent.Animation != rightHandOperand,
                    _ => throw new Exception($"AnimationCondition doesn't support {conditionOperator}.")
                };

                if (isConditionValid) {
                    if (sw.ElapsedMilliseconds >= durationMs) {
                        return true && EvaluateNext();
                    }
                }
                else {
                    sw.Restart();
                }
            }

            return false;
        }

        private static void ToImGui(ref OperatorEnum operator_, ref Animation animation, ref int duration) {
            ImGui.Text($"Only {OperatorEnum.EQUAL_TO} & {OperatorEnum.NOT_EQUAL_TO} supported.");
            ImGui.Text("Player is");
            ImGui.SameLine();
            UiHelper.EnumComboBox("##AnimationOperator", ref operator_);
            ImGui.SameLine();
            UiHelper.EnumComboBox("for ##AnimationRHS", ref animation);
            ImGui.SameLine();
            ImGui.InputInt("ms##AnimationDuration", ref duration);
        }
    }
}