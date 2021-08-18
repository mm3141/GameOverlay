// <copyright file="DecimalCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using ImGuiNET;

    /// <summary>
    /// FlaskManager condition to trigger flask on some decimal changes.
    /// </summary>
    public abstract class DecimalCondition
        : BaseCondition
    {
        /// <summary>
        /// value to compare the data with.
        /// </summary>
#pragma warning disable SA1401
        protected int value = default;
#pragma warning restore SA1401

        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalCondition"/> class.
        /// </summary>
        /// <param name="name">Name of this <see cref="DecimalCondition"/>.</param>
        /// <param name="op">Operator to perform on the <see cref="DecimalCondition"/>.</param>
        /// <param name="threshold">threshold of <see cref="DecimalCondition"/>.</param>
        public DecimalCondition(string name, OperatorEnum op, int threshold)
        : base()
        {
            this.Name = name;
            this.Operator = op;
            this.value = threshold;
        }

        /// <inheritdoc/>
        public override void DisplayConditionImGuiWidget()
        {
            ImGui.Text($"{this.Name} {this.Operator} ");
            ImGui.SameLine();
            ImGui.InputInt($"##{this.Name}{this.GetHashCode()}", ref this.value);
            if (this.Next != null)
            {
                ImGui.TreePush();
                this.Next.DisplayConditionImGuiWidget();
                ImGui.TreePop();
            }
        }
    }
}
