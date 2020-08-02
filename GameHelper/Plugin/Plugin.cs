// <copyright file="Plugin.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Plugin
{
    /// <summary>
    /// Plugin loaded into the memory.
    /// </summary>
    public class Plugin
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="core">plugin class of type IPlugin.</param>
        /// <param name="settings">plugin class of type ISettings.</param>
        /// <param name="pluginRootDirectory">Plugin directory location.</param>
        public Plugin(IPlugin core, ISettings settings, string pluginRootDirectory)
        {
            this.Settings = settings;
            this.Core = core;
            this.Core.PluginRootDirectory = pluginRootDirectory;
            this.Core.OnLoad();
        }

        /// <summary>
        /// Gets the plugin core class.
        /// </summary>
        public IPlugin Core { get; }

        /// <summary>
        /// Gets the plugin settings class.
        /// </summary>
        public ISettings Settings { get; }
    }
}
