// <copyright file="State.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Settings
{
    using System.IO;
    using Newtonsoft.Json;

#pragma warning disable SA1401 // Fields should be private, not possible because of Newtonsoft/ImGui.

    /// <summary>
    /// Game Helper Core Settings.
    /// </summary>
    internal class State
    {
        /// <summary>
        /// Core Setting File Information.
        /// </summary>
        [JsonIgnore]
        public static readonly FileInfo CoreSettingFile = new FileInfo("configs/core_settings.json");

        /// <summary>
        /// Plugins metadata File information.
        /// </summary>
        [JsonIgnore]
        public static readonly FileInfo PluginsMetadataFile = new FileInfo("configs/plugins.json");

        /// <summary>
        /// Folder containing all the plugins.
        /// </summary>
        [JsonIgnore]
        public static readonly DirectoryInfo PluginsDirectory = new DirectoryInfo("Plugins");

        /// <summary>
        /// Gets or sets a value indicating whether the overlay is running or not.
        /// </summary>
        [JsonIgnore]
        public bool IsOverlayRunning = true;

        /// <summary>
        /// Gets or sets hotKey to show/hide the main menu.
        /// </summary>
        public int MainMenuHotKey = 0x7B;

        /// <summary>
        /// Gets or sets a value indicating whether to show
        /// the performance stats or not.
        /// </summary>
        public bool ShowPerfStats = false;

        /// <summary>
        /// Gets or sets a value indicating whether to hide
        /// the performance stats window when game is in background.
        /// </summary>
        public bool HidePerfStatsWhenBg = false;

        /// <summary>
        /// Gets or sets a value indicating whether
        /// to show DataVisualization window or not.
        /// </summary>
        public bool ShowDataVisualization = false;

        /// <summary>
        /// Gets or sets a value indicating whether
        /// to show Game Ui Explorer or not.
        /// </summary>
        public bool ShowGameUiExplorer = false;

        /// <summary>
        /// Gets the Window scale value.
        /// </summary>
        public float WindowScale = 1f;

        /// <summary>
        /// Gets a value indicating how much time to wait between key presses.
        /// </summary>
        public int KeyPressTimeout = 50;
    }

#pragma warning restore SA1401 // Fields should be private
}