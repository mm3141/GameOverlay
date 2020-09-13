// <copyright file="Settings.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper
{
    using System.IO;
    using Newtonsoft.Json;

#pragma warning disable SA1401 // Fields should be private, not possible because of Newtonsoft/ImGui.

    /// <summary>
    /// Game Helper Core Settings.
    /// </summary>
    internal class Settings
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
        /// Gets or sets hotKey to show/hide the main menu.
        /// </summary>
        public int MainMenuHotKey = 0x7B;

        /// <summary>
        /// Gets or sets a value indicating whether to show the terminal.
        /// </summary>
        public bool ShowTerminal = true;

        /// <summary>
        /// Gets or sets a value indicating whether to close the Game Helper on game exit.
        /// </summary>
        public bool CloseOnGameExit = false;
    }

#pragma warning restore SA1401 // Fields should be private
}