// <copyright file="VitalType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Enums
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    ///     Different type of player Vitals.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VitalType
    {
        /// <summary>
        ///     Condition based on player Mana.
        /// </summary>
        MANA,

        /// <summary>
        ///     Condition based on player Mana.
        /// </summary>
        MANA_PERCENT,

        /// <summary>
        ///     Condition based on player Life.
        /// </summary>
        LIFE,

        /// <summary>
        ///     Condition based on player Life.
        /// </summary>
        LIFE_PERCENT,

        /// <summary>
        ///     Condition based on player Energy Shield.
        /// </summary>
        ENERGYSHIELD,

        /// <summary>
        ///     Condition based on player Energy Shield.
        /// </summary>
        ENERGYSHIELD_PERCENT
    }
}
