// <copyright file="CoreSettings.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper
{
    using System.IO;
    using Newtonsoft.Json;

#pragma warning disable SA1401 // Fields should be private
    /// <summary>
    /// Game Helper Core Settings.
    /// </summary>
    public class CoreSettings
    {
        /// <summary>
        /// Core Setting File Information.
        /// </summary>
        [JsonIgnore]
        public static FileInfo CoreSettingFile = new FileInfo("configs/core_settings.json");

        /// <summary>
        /// Gets or sets hotKey to show/hide the main menu.
        /// </summary>
        public int MainMenuHotKey = 0x7B;

        /// <summary>
        /// Gets or sets a value indicating whether to hides the terminal.
        /// </summary>
        public bool HideTerminal = true;

        /// <summary>
        /// Gets or sets a value indicating whether to close the Game Helper on game exit.
        /// </summary>
        public bool CloseOnGameExit = false;

        /// <summary>
        /// Creates or load settings from the file.
        /// </summary>
        /// <returns>CoreSettings instance.</returns>
        public static CoreSettings CreateOrLoadSettings()
        {
            CoreSettingFile.Directory.Create();
            if (CoreSettingFile.Exists)
            {
                var content = File.ReadAllText(CoreSettingFile.FullName);
                return JsonConvert.DeserializeObject<CoreSettings>(content);
            }
            else
            {
                var coreSettings = new CoreSettings();
                coreSettings.SafeToFile();
                return coreSettings;
            }
        }

        /// <summary>
        /// Save the current settings into the file.
        /// </summary>
        public void SafeToFile()
        {
            var content = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(CoreSettingFile.FullName, content);
        }
    }

#pragma warning restore SA1401 // Fields should be private
}
