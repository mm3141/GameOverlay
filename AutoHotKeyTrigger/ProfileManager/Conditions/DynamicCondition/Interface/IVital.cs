// <copyright file="IVital.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Conditions.DynamicCondition.Interface
{
    /// <summary>
    ///     Information about a vital
    /// </summary>
    public interface IVital
    {
        /// <summary>
        ///     Current value
        /// </summary>
        double Current { get; }

        /// <summary>
        ///     Maximum value
        /// </summary>
        double Max { get; }

        /// <summary>
        ///     Value in percent from the max
        /// </summary>
        double Percent { get; }
    }
}
