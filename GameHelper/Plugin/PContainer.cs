// <copyright file="PContainer.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Plugin
{
    using Newtonsoft.Json;

    /// <summary>
    ///     Container for storing plugin and its metadata.
    /// </summary>
    internal struct PContainer
    {
        /// <summary>
        ///     Gets or sets a value indicating whether the plugin is enabled or not.
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        ///     Gets or sets the plugin.
        /// </summary>
        [JsonIgnore]
        public IPCore Plugin { get; set; }
    }
}