// <copyright file="DelayTimerCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System.Diagnostics;
    using ImGuiNET;

    /// <summary>
    ///     For triggering a flask on number of flask charges the flask got.
    /// </summary>
    public class DelayTimerCondition
        : BaseCondition<float>
    {
        private static float delayTimerStatic;

        private readonly Stopwatch delayTimer = Stopwatch.StartNew();

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelayTimerCondition" /> class.
        /// </summary>
        /// <param name="delayTimer">Delay time in seconds.</param>
        public DelayTimerCondition(float delayTimer)
            : base(OperatorEnum.BIGGER_THAN, delayTimer) // OperatorEnum is ignored in this condition.
        {
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public new static DelayTimerCondition Add()
        {
            ToImGui(ref delayTimerStatic);
            ImGui.SameLine();
            if (ImGui.Button("Add##FlaskCharges") && delayTimerStatic >= 0.0f)
            {
                return new DelayTimerCondition(delayTimerStatic);
            }

            return null;
        }

        /// <inheritdoc />
        public override void Display(int index = 0)
        {
            ToImGui(ref this.rightHandOperand);
            base.Display(index);
        }

        /// <inheritdoc />
        public override bool Evaluate()
        {
            var elapsedSeconds = this.delayTimer.ElapsedMilliseconds / 1000f;
            if (elapsedSeconds > this.rightHandOperand)
            {
                if (this.Next() == null || this.EvaluateNext())
                {
                    this.delayTimer.Restart();
                    return true;
                }
            }

            return false;
        }

        private static void ToImGui(ref float delayTimer)
        {
            ImGui.Text("Wait for ");
            ImGui.SameLine();
            ImGui.DragFloat("seconds##DelayTimerConditionDelay", ref delayTimer, 0.1f, 0.0f, 30.0f);
        }
    }
}