// <copyright file="ConditionType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Enums
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    ///     Conditions supported by this plugin.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConditionType
    {
        /// <summary>
        ///     Condition based on player Vitals.
        /// </summary>
        VITALS,

        /// <summary>
        ///     Condition based on what player is doing.
        /// </summary>
        ANIMATION,

        /// <summary>
        ///     Condition based on player Buffs/Debuffs.
        /// </summary>
        STATUS_EFFECT,

        /// <summary>
        ///     Condition based on flask mod active on player or not.
        /// </summary>
        FLASK_EFFECT,

        /// <summary>
        ///     Condition based on number of charges flask has.
        /// </summary>
        FLASK_CHARGES,

        /// <summary>
        ///     Condition based on Ailment on the player.
        /// </summary>
        AILMENT,

        /// <summary>
        ///     Condition base on user code
        /// </summary>
        DYNAMIC
    }
}
