// <copyright file="BaseCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using ImGuiNET;

    /// <summary>
    /// Abstract class for creating conditions on which flasks can trigger.
    /// </summary>
    /// <typeparam name="T">
    /// The condition right hand side operand type.
    /// </typeparam>
    public abstract class BaseCondition<T>
        : ICondition
    {
#pragma warning disable SA1401 // Fields should be private
        /// <summary>
        /// Right hand side operand of the condition.
        /// </summary>
        protected T rightHandOperand;

        /// <summary>
        /// The operator to use for the condition.
        /// </summary>
        protected OperatorEnum conditionOperator;
#pragma warning restore SA1401 // Fields should be private

        private ICondition next;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCondition{T}"/> class.
        /// </summary>
        /// <param name="operator_">
        /// <see cref="OperatorEnum"/> to use in this condition.
        /// </param>
        /// <param name="rightHandSide">
        /// Right hand side operand of the Condition.
        /// </param>
        public BaseCondition(OperatorEnum operator_, T rightHandSide)
        {
            this.next = null;
            this.conditionOperator = operator_;
            this.rightHandOperand = rightHandSide;
        }

        /// <summary>
        /// Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        /// <see cref="ICondition"/> if user wants to add the condition, otherwise null.
        /// </returns>
        public static ICondition Add() =>
            throw new NotImplementedException($"{typeof(BaseCondition<T>).Name} " +
                $"class doesn't have ImGui widget for creating conditions.");

        /// <inheritdoc/>
        public abstract bool Evaluate();

        /// <inheritdoc/>
        public virtual void Display()
        {
            if (this.next != null)
            {
                ImGui.TreePush();
                this.next.Display();
                ImGui.TreePop();
            }
        }

        /// <inheritdoc/>
        public ICondition Next() => this.next;

        /// <inheritdoc/>
        public void Append(ICondition condition)
        {
            var nxt = this.next;
            while (nxt != null)
            {
                nxt = nxt.Next();
            }

            nxt = condition;
        }

        /// <summary>
        /// Evaluates the next in line condition.
        /// </summary>
        /// <returns>
        /// True if the next condition doesn't exists or is successful otherwise false.
        /// </returns>
        protected bool EvaluateNext()
        {
            return this.next == null || this.next.Evaluate();
        }
    }
}
