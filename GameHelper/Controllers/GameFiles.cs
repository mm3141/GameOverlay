// <copyright file="GameFiles.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Controllers
{
    using System;

    /// <summary>
    /// Reads and store the files loaded in the game memory.
    /// </summary>
    public class GameFiles : ControllerBase
    {
        /// <inheritdoc/>
        protected override void OnAddressUpdated(IntPtr newAddress)
        {
        }
    }
}
