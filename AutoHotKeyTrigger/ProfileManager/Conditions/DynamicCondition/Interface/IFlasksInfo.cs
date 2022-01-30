// <copyright file="IFlasksInfo.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Conditions.DynamicCondition.Interface
{
    /// <summary>
    ///     Information about a set of flasks
    /// </summary>
    public interface IFlasksInfo
    {
        /// <summary>
        ///     Provides access to the flask array
        /// </summary>
        /// <param name="i">The flask index (0-based)</param>
        IFlaskInfo this[int i] { get; }
    }
}
