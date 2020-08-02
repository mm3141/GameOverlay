// <copyright file="IPlugin.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Plugin
{
    /// <summary>
    /// Interface for creating plugins.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Gets or sets the plugin root directory folder.
        /// </summary>
        public string PluginRootDirectory { get; set; }

        /// <summary>
        /// Called when the plugin is initialized/loaded into the framework.
        /// </summary>
        public void OnLoad();

        /// <summary>
        /// Called when the plugin is Enabled.
        /// </summary>
        public void OnEnable();

        /// <summary>
        /// Called when the plugin is disabled.
        /// </summary>
        public void OnDisable();
    }
}