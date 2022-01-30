// <copyright file="ICondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Conditions
{
    /// <summary>
    ///     Interface for the conditions on which actions can trigger.
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        ///     Evaluates the condition.
        /// </summary>
        /// <returns>true if the condition is successful, otherwise false.</returns>
        public bool Evaluate();

        /// <summary>
        ///     Displays the condition to the user via ImGui.
        /// </summary>
        public void Display();
    }
}