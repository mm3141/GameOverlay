// <copyright file="JsonDataHelper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager
{
    using System.Collections.Generic;

    /// <summary>
    /// A helper class that reads JSON data and provides useful functions.
    /// </summary>
    public static class JsonDataHelper
    {
        /// <summary>
        /// Gets or sets the flask buff name by looking at flask base name.
        /// </summary>
        public static Dictionary<string, string> FlaskNameToBuff { get; set; } = null;
    }
}
