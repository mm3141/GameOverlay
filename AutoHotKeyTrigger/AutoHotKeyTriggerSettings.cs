// <copyright file="AutoHotKeyTriggerSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger
{
    using System;
    using System.Collections.Generic;
    using GameHelper.Plugin;
    using ProfileManager;
    using ProfileManager.Conditions;

    /// <summary>
    ///     <see cref="AutoHotKeyTrigger" /> plugin settings class.
    /// </summary>
    public sealed class AutoHotKeyTriggerSettings : IPSettings
    {
        /// <summary>
        ///    Gets a value indicating whether to enable or disable the auto-quit feature.
        /// </summary>
        public bool EnableAutoQuit = false;

        /// <summary>
        ///     Condition on which user want to auto-quit.
        /// </summary>
        [Obsolete("Any rule can be an auto-quit trigger now")]
        public VitalsCondition AutoQuitCondition { internal get; set; } = null;

        /// <summary>
        ///     Gets a key which allows the user to manually quit the game Connection.
        /// </summary>
        public ConsoleKey AutoQuitKey = ConsoleKey.F11;

        /// <summary>
        ///     Gets all the profiles containing rules on when to perform the action.
        /// </summary>
        public readonly Dictionary<string, Profile> Profiles = new();

        /// <summary>
        ///     Gets the currently selected profile.
        /// </summary>
        public string CurrentProfile = string.Empty;

        /// <summary>
        ///     Gets a value indicating weather the debug mode is enabled or not.
        /// </summary>
        public bool DebugMode = false;

        /// <summary>
        ///     Gets a value indicating weather this plugin should work in hideout or not.
        /// </summary>
        public bool ShouldRunInHideout = true;

        /// <summary>
        ///     Gets a value indicating weather user wants to dump the player
        ///     status effect or not.
        /// </summary>
        public ConsoleKey DumpStatusEffectOnMe = ConsoleKey.F10;
    }
}