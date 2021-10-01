// <copyright file="ICondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    /// <summary>
    /// Interface for the conditions on which flask can trigger.
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// Evaluates the condition.
        /// </summary>
        /// <returns>true if the condition is successful, otherwise false.</returns>
        public bool Evaluate();

        /// <summary>
        /// Displays the condition to the user via ImGui.
        /// </summary>
        public void Display();

        /// <summary>
        /// Appends another condition to this condition.
        /// </summary>
        /// <param name="condition">
        /// condition to append.
        /// </param>
        public void Append(ICondition condition);

        /// <summary>
        /// Gets the next in line condition.
        /// </summary>
        /// <returns><see cref="ICondition"/> if it exists, otherwise null.</returns>
        public ICondition Next();

        /// <summary>
        /// Deletes all the conditions in the rule.
        /// </summary>
        public void Delete();
    }
}
