// <copyright file="IPlugin.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Plugin
{
    /// <summary>
    /// Interface for creating plugins.
    /// </summary>
    internal interface IPlugin
    {
        /// <summary>
        /// Called when the plugin is loaded in the memory.
        /// </summary>
        public abstract void OnLoad();

        /// <summary>
        /// Called when the plugin is Enabled.
        /// </summary>
        public abstract void OnEnable();

        /// <summary>
        /// Called when the plugin is disabled.
        /// </summary>
        public abstract void OnDisable();

        /// <summary>
        /// Draws the settings on the ImGui Settings window.
        /// </summary>
        public abstract void DrawSettings();

        /// <summary>
        /// Draws the plugin UI.
        /// </summary>
        public abstract void DrawUI();
    }
}