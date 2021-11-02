// <copyright file="CooldownCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System.Diagnostics;
    using ImGuiNET;

    /// <summary>
    ///     For triggering a flask on number of flask charges the flask got.
    /// </summary>
    public class CooldownCondition
        : BaseCondition<float>
    {
        private static float cooldownStatic;

        private readonly Stopwatch cooldownTimer = Stopwatch.StartNew();

        /// <summary>
        ///     Initializes a new instance of the <see cref="CooldownCondition" /> class.
        /// </summary>
        /// <param name="cooldown">Cooldown time in seconds.</param>
        public CooldownCondition(float cooldown)
            : base(OperatorEnum.BIGGER_THAN, cooldown) // OperatorEnum is ignored in this condition.
        {
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public new static CooldownCondition Add()
        {
            ToImGui(ref cooldownStatic);
            ImGui.SameLine();
            if (ImGui.Button("Add##FlaskCharges") && cooldownStatic >= 0.0f)
            {
                return new CooldownCondition(cooldownStatic);
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
            var elapsedSeconds = this.cooldownTimer.ElapsedMilliseconds / 1000f;
            if (elapsedSeconds > this.rightHandOperand)
            {
                if (this.Next() == null || this.EvaluateNext())
                {
                    this.cooldownTimer.Restart();
                    return true;
                }
            }

            return false;
        }

        private static void ToImGui(ref float cooldown)
        {
            ImGui.Text("Wait for ");
            ImGui.SameLine();
            ImGui.DragFloat("seconds##CooldownConditionCooldown", ref cooldown, 0.1f, 0.0f, 30.0f);
        }
    }
}