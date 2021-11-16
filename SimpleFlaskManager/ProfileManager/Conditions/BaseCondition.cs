// <copyright file="BaseCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using Newtonsoft.Json;
    using SimpleFlaskManager.ProfileManager.Enums;

    /// <summary>
    ///     Abstract class for creating conditions on which flasks can trigger.
    /// </summary>
    /// <typeparam name="T">
    ///     The condition right hand side operand type.
    /// </typeparam>
    public abstract class BaseCondition<T>
        : ICondition
    {
        /// <summary>
        ///     The operator to use for the condition.
        /// </summary>
        [JsonProperty]
        protected OperatorType conditionOperator;

        /// <summary>
        ///     Right hand side operand of the condition.
        /// </summary>
        [JsonProperty]
        protected T rightHandOperand;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseCondition{T}" /> class.
        /// </summary>
        /// <param name="operator_">
        ///     <see cref="OperatorType" /> to use in this condition.
        /// </param>
        /// <param name="rightHandSide">
        ///     Right hand side operand of the Condition.
        /// </param>
        public BaseCondition(OperatorType operator_, T rightHandSide)
        {
            this.conditionOperator = operator_;
            this.rightHandOperand = rightHandSide;
        }

        /// <inheritdoc />
        public abstract bool Evaluate();

        /// <inheritdoc />
        public virtual void Display(int index = 0)
        {
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public static ICondition Add()
        {
            throw new NotImplementedException(
                $"{typeof(BaseCondition<T>).Name} class doesn't have ImGui widget for creating conditions.");
        }
    }
}