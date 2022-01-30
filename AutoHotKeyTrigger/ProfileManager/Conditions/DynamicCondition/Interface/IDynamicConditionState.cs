// <copyright file="IDynamicConditionState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Conditions.DynamicCondition.Interface
{
    using System.Collections.Generic;
    using GameHelper.RemoteEnums;

    /// <summary>
    ///     The structure that can be queried using DynamicCondition
    /// </summary>
    public interface IDynamicConditionState
    {
        /// <summary>
        ///     The ailment list
        /// </summary>
        IReadOnlyCollection<string> Ailments { get; }

        /// <summary>
        ///     The current animation
        /// </summary>
        Animation Animation { get; }

        /// <summary>
        ///     The buff list
        /// </summary>
        IBuffDictionary Buffs { get; }

        /// <summary>
        ///     The flask information
        /// </summary>
        IFlasksInfo Flasks { get; }

        /// <summary>
        ///     The vitals information
        /// </summary>
        IVitalsInfo Vitals { get; }
    }
}
