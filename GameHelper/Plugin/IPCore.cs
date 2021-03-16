// <copyright file="IPCore.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Plugin
{
    /// <summary>
    /// Interface for creating plugins.
    /// </summary>
    internal interface IPCore
    {
        /// <summary>
        /// Called at the init of the plugin to set
        /// it's directory information.
        /// </summary>
        /// <param name="dllLocation">plugin dll directory.</param>
        public abstract void SetPluginDllLocation(string dllLocation);

        /// <summary>
        /// Called when the plugin needs to be enabled
        /// (e.g. game opened or user wants the plugin).
        /// </summary>
        public abstract void OnEnable();

        /// <summary>
        /// Called when the plugin needs to be disabled
        /// (e.g. game closed or user don't want the plugin).
        /// </summary>
        public abstract void OnDisable();

        /// <summary>
        /// Called to draw the plugin settings on the GameHelper Settings window.
        /// Should use ImGui objects to draw the settings.
        /// </summary>
        public abstract void DrawSettings();

        /// <summary>
        /// Draws the plugin UI. This function isn't called when the plugin is disabled.
        /// </summary>
        public abstract void DrawUI();

        /// <summary>
        /// Called when it's time to save all the settings
        /// related to the plugin on the file.
        ///
        /// NOTE: Load settings function isn't provided as
        /// it's expected the plugin will load the settings
        /// in OnEnable function.
        /// </summary>
        public abstract void SaveSettings();
    }
}