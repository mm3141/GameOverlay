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
        /// Draws the plugin UI.
        /// </summary>
        public abstract void DrawUI();
    }
}