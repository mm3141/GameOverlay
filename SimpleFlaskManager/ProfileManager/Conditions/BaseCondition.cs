// <copyright file="BaseCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using ImGuiNET;
    using Newtonsoft.Json;

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
        protected OperatorEnum conditionOperator;

        [JsonProperty] private ICondition next;

        /// <summary>
        ///     Right hand side operand of the condition.
        /// </summary>
        [JsonProperty]
        protected T rightHandOperand;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseCondition{T}" /> class.
        /// </summary>
        /// <param name="operator_">
        ///     <see cref="OperatorEnum" /> to use in this condition.
        /// </param>
        /// <param name="rightHandSide">
        ///     Right hand side operand of the Condition.
        /// </param>
        public BaseCondition(OperatorEnum operator_, T rightHandSide)
        {
            this.next = null;
            this.conditionOperator = operator_;
            this.rightHandOperand = rightHandSide;
        }

        /// <inheritdoc />
        public abstract bool Evaluate();

        /// <inheritdoc />
        public virtual void Display(int index = 0)
        {
            if (this.next != null)
            {
                ImGui.Separator();
                ImGui.PushID(++index);
                this.next.Display(index);
                ImGui.PopID();
            }
        }

        /// <inheritdoc />
        public ICondition Next()
        {
            return this.next;
        }

        /// <inheritdoc />
        public void Append(ICondition condition)
        {
            if (this.next == null)
            {
                this.next = condition;
            }
            else
            {
                this.next.Append(condition);
            }
        }

        /// <inheritdoc />
        public void Delete()
        {
            if (this.next != null)
            {
                this.next.Delete();
            }

            this.next = null;
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

        /// <summary>
        ///     Evaluates the next in line condition.
        /// </summary>
        /// <returns>
        ///     True if the next condition doesn't exists or is successful otherwise false.
        /// </returns>
        protected bool EvaluateNext()
        {
            return this.next == null || this.next.Evaluate();
        }
    }
}