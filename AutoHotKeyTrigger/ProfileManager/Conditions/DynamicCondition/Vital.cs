// <copyright file="Vital.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Conditions.DynamicCondition
{
    using GameOffsets.Objects.Components;
    using Interface;

    /// <summary>
    ///     Information about a vital
    /// </summary>
    /// <param name="Current">Current value</param>
    /// <param name="Max">Maximum value</param>
    public record Vital(double Current, double Max) : IVital
    {
        /// <summary>
        ///     Value in percent from the max
        /// </summary>
        public double Percent => this.Current / this.Max * 100;

        /// <summary>
        ///     Creates a new instance
        /// </summary>
        /// <param name="vital">Source data for the structure</param>
        /// <returns></returns>
        public static Vital From(VitalStruct vital)
        {
            return new Vital(vital.Current, vital.Unreserved);
        }
    }
}
