// <copyright file="SimpleFlaskManagerSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager
{
    using System.Collections.Generic;
    using GameHelper.Plugin;
    using SimpleFlaskManager.ProfileManager;

    /// <summary>
    /// <see cref="SimpleFlaskManager"/> plugin settings class.
    /// </summary>
    public sealed class SimpleFlaskManagerSettings : IPSettings
    {
#pragma warning disable SA1401
        /// <summary>
        /// Gets a value indicating weather flask manager debug mode is enabled or not.
        /// </summary>
        public bool DebugMode = false;

        /// <summary>
        /// Gets a value indicating weather flask manager should work in hideoutor not.
        /// </summary>
        public bool ShouldRunInHideout = true;

        /// <summary>
        /// Gets the currently selected profile.
        /// </summary>
        public string CurrentProfile = string.Empty;

        /// <summary>
        /// Gets all the profiles containing rules on when to drink the flasks.
        /// </summary>
        public Dictionary<string, Profile> Profiles = new Dictionary<string, Profile>();
#pragma warning restore SA1401
    }
}
