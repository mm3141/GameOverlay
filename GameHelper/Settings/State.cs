// <copyright file="State.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Settings
{
    using System;
    using System.IO;
    using ClickableTransparentOverlay;
    using Newtonsoft.Json;

    /// <summary>
    ///     Game Helper Core Settings.
    /// </summary>
    internal class State
    {
        /// <summary>
        ///     Core Setting File Information.
        /// </summary>
        [JsonIgnore]
        public static readonly FileInfo CoreSettingFile = new("configs/core_settings.json");

        /// <summary>
        ///     Plugins metadata File information.
        /// </summary>
        [JsonIgnore]
        public static readonly FileInfo PluginsMetadataFile = new("configs/plugins.json");

        /// <summary>
        ///     Folder containing all the plugins.
        /// </summary>
        [JsonIgnore]
        public static readonly DirectoryInfo PluginsDirectory = new("Plugins");

        /// <summary>
        ///     Gets a value indicating whether user wants to disable entity processing
        ///     when in town/hideout.
        /// </summary>
        public bool DisableEntityProcessingInTownOrHideout = false;

        /// <summary>
        ///     Gets or sets a value indicating whether to hide
        ///     the performance stats window when game is in background.
        /// </summary>
        public bool HidePerfStatsWhenBg = false;

        /// <summary>
        ///     Gets a value indicating whether user wants to hide the overlay on start or not.
        /// </summary>
        public bool HideSettingWindowOnStart = false;

        /// <summary>
        ///     Gets or sets a value indicating whether the overlay is running or not.
        /// </summary>
        [JsonIgnore]
        public bool IsOverlayRunning = true;

        /// <summary>
        ///     Gets a value indicating how much time to wait between key presses.
        /// </summary>
        public int KeyPressTimeout = 80;

        /// <summary>
        ///     Gets the font pathname to load in ImGui.
        /// </summary>
        public string FontPathName = @"C:\Windows\Fonts\msyh.ttc";

        /// <summary>
        ///     Gets the font size to load in ImGui.
        /// </summary>
        public int FontSize = 18;

        /// <summary>
        ///     Gets the language that the font supports.
        /// </summary>
        public FontGlyphRangeType FontLanguage = FontGlyphRangeType.ChineseSimplifiedCommon;


        /// <summary>
        ///     Gets the custom glyph range to load from the font texture. This is useful in case
        ///     <see cref="FontGlyphRangeType"/> isn't enough. Set it to empty string to disable this
        ///     feature.
        /// </summary>
        public string FontCustomGlyphRange = string.Empty;

        /// <summary>
        ///     Gets or sets hotKey to show/hide the main menu.
        /// </summary>
        public ConsoleKey MainMenuHotKey = ConsoleKey.F12;

        /// <summary>
        ///     Gets or sets a value indicating whether
        ///     to show DataVisualization window or not.
        /// </summary>
        public bool ShowDataVisualization = false;

        /// <summary>
        ///     Gets or sets a value indicating whether
        ///     to show Game Ui Explorer or not.
        /// </summary>
        public bool ShowGameUiExplorer = false;

        /// <summary>
        ///     Gets or sets a value indicating whether to show
        ///     the performance stats or not.
        /// </summary>
        public bool ShowPerfStats = false;

        /// <summary>
        ///     Gets or sets a value indicating what nearby means to the user.
        /// </summary>
        public int NearbyMeaning = 70;

        /// <summary>
        ///     Gets a value indicating whether user wants to load the
        ///     preload-loaded-files in hideout or not.
        /// </summary>
        public bool SkipPreloadedFilesInHideout = true;

        /// <summary>
        ///     Gets a value indicating whether user wants to close the Game Helper when
        ///     the game exit or not.
        /// </summary>
        public bool CloseWhenGameExit = false;
    }
}
