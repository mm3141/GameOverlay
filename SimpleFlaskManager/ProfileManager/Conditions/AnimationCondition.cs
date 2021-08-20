// <copyright file="AnimationCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using System.Diagnostics;
    using GameHelper;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.Components;
    using ImGuiNET;

    /// <summary>
    /// FlaskManager condition to trigger flask on player Animation changes.
    /// </summary>
    public class AnimationCondition
        : BaseCondition
    {
        private static OperatorEnum op = OperatorEnum.EQUAL_TO;
        private static Animation selected = 0;
        private static int animationTimeMs = 0;
        private Animation value;
        private int timeMs = 0;
        private Stopwatch sw;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationCondition"/> class.
        /// </summary>
        /// <param name="op">Operation to use in this condition.</param>
        /// <param name="animation">Animation to compare the data with.</param>
        /// <param name="animationTime">
        /// For how long (ms) the condition is met before drinking the flask.
        /// </param>
        public AnimationCondition(OperatorEnum op, Animation animation, int animationTime)
        : base()
        {
            this.Name = typeof(Animation).Name;
            this.Operator = op;
            this.value = animation;
            this.timeMs = animationTime;
            this.sw = Stopwatch.StartNew();
        }

        /// <summary>
        /// Draws the ImGui Widget for creating  <see cref="AnimationCondition"/>.
        /// </summary>
        /// <returns>
        /// <see cref="AnimationCondition"/> if user allows it to be created otherwise null.
        /// </returns>
        public static AnimationCondition AddConditionImGuiWidget()
        {
            ImGui.Text($"Player Animation is");
            ImGui.SameLine();
            if (ImGui.BeginCombo($"##OperationAnimation", $"{op}"))
            {
                if (ImGui.Selectable($"{OperatorEnum.EQUAL_TO}"))
                {
                    op = OperatorEnum.EQUAL_TO;
                }

                if (ImGui.Selectable($"{OperatorEnum.NOT_EQUAL_TO}"))
                {
                    op = OperatorEnum.NOT_EQUAL_TO;
                }

                ImGui.EndCombo();
            }

            var enumNames = Enum.GetNames(typeof(Animation));
            int sel = (int)selected;
            if (ImGui.Combo("for ##AnimationCondition", ref sel, enumNames, enumNames.Length))
            {
                selected = (Animation)Enum.Parse(typeof(Animation), enumNames[sel]);
            }

            ImGui.SameLine();
            ImGui.InputInt("(ms)##animationForCondition", ref animationTimeMs);
            ImGui.SameLine();
            if (ImGui.Button($"Add##Animation"))
            {
                return new AnimationCondition(op, selected, animationTimeMs);
            }

            return null;
        }

        /// <inheritdoc/>
        public override void DisplayConditionImGuiWidget()
        {
            ImGui.Text($"Player {this.Name} is {this.Operator} ");
            ImGui.SameLine();
            var enumNames = Enum.GetNames(typeof(Animation));
            int sel = (int)this.value;
            ImGui.Combo("##AnimationCondition", ref sel, enumNames, enumNames.Length);
            this.value = (Animation)Enum.Parse(typeof(Animation), enumNames[sel]);
            if (this.Next != null)
            {
                ImGui.TreePush();
                this.Next.DisplayConditionImGuiWidget();
                ImGui.TreePop();
            }
        }

        /// <inheritdoc/>
        public override bool Evaluate()
        {
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Actor>(out var actorComponent))
            {
                var isConditionValid = this.Operator switch
                {
                    OperatorEnum.EQUAL_TO => actorComponent.Animation == this.value,
                    OperatorEnum.NOT_EQUAL_TO => actorComponent.Animation != this.value,
                    _ => throw new Exception($"AnimationCondition doesn't support {this.Operator}."),
                };

                if (isConditionValid)
                {
                    if (this.sw.ElapsedMilliseconds >= this.timeMs)
                    {
                        return true && this.EvaluateNext();
                    }
                }
                else
                {
                    this.sw.Restart();
                }
            }

            return false;
        }
    }
}
