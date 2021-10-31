// <copyright file="SimpleFlaskManagerSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager {
    using System.Collections.Generic;
    using GameHelper.Plugin;
    using ProfileManager;
    using ProfileManager.Conditions;

    /// <summary>
    ///     <see cref="SimpleFlaskManager" /> plugin settings class.
    /// </summary>
    public sealed class SimpleFlaskManagerSettings : IPSettings {
        /// <summary>
        ///     Condition on which user want to auto-quit.
        /// </summary>
        public readonly VitalsCondition AutoQuitCondition =
            new(OperatorEnum.LESS_THAN, VitalsCondition.VitalsEnum.LIFE, -1);

        /// <summary>
        ///     Gets the currently selected profile.
        /// </summary>
        public string CurrentProfile = string.Empty;

        /// <summary>
        ///     Gets a value indicating weather flask manager debug mode is enabled or not.
        /// </summary>
        public bool DebugMode = false;

        /// <summary>
        ///     Gets all the profiles containing rules on when to drink the flasks.
        /// </summary>
        public readonly Dictionary<string, Profile> Profiles = new();

        /// <summary>
        ///     Gets a value indicating weather flask manager should work in hideout or not.
        /// </summary>
        public bool ShouldRunInHideout = true;
    }
}