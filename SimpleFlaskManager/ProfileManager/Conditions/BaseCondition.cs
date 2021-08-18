// <copyright file="BaseCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    /// <summary>
    /// Abstract class to store FlaskManager trigger conditions.
    /// </summary>
    public abstract class BaseCondition
    {
        /// <summary>
        /// Gets or sets the user friendly name of the condition.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating the kind of comparison to perform on the data.
        /// </summary>
        public OperatorEnum Operator { get; set; } = OperatorEnum.EQUAL_TO;

        /// <summary>
        /// Gets or sets the next condition to evaluate in case this condition is true.
        /// </summary>
        public BaseCondition Next { get; set; } = null;

        /// <summary>
        /// Evaluates the condition.
        /// </summary>
        /// <returns>True in case the condition is successful otherwise fase.</returns>
        public abstract bool Evaluate();

        /// <summary>
        /// Displays the Condition on ImGui window.
        /// </summary>
        public abstract void DisplayConditionImGuiWidget();

        /// <summary>
        /// A helper function to evaluates the next condition.
        /// </summary>
        /// <returns>
        /// True if next condition is successfull
        /// or next condition is null otherwise false.
        /// </returns>
        protected bool EvaluateNext()
        {
            return this.Next == null || this.Next.Evaluate();
        }
    }
}
