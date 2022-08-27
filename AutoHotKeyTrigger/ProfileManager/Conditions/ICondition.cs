// <copyright file="ICondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>


namespace AutoHotKeyTrigger.ProfileManager.Conditions
{
    using AutoHotKeyTrigger.ProfileManager.Component;

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
        /// <param name="expand">whether to expand the condition or not</param>
        public void Display(bool expand);

        /// <summary>
        ///     Adds the component to the condition.
        /// </summary>
        /// <param name="component">component object to add.</param>
        void Add(IComponent component);
    }
}