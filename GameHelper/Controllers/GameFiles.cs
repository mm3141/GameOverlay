// <copyright file="GameFiles.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Controllers
{
    using System;
    using GameHelper.RemoteMemoryObjects.Files;
    using GameOffsets.Controllers;

    /// <summary>
    /// Reads and store the static file object located in the game memory.
    /// </summary>
    public class GameFiles : ControllerBase
    {
        /// <summary>
        /// Gets the global list of files loaded in the memory.
        /// </summary>
        public FilesGlobalList Files
        {
            get;
            private set;
        }

        = new FilesGlobalList(IntPtr.Zero);

        /// <inheritdoc/>
        protected override void OnAddressUpdated(IntPtr newAddress)
        {
            if (newAddress == IntPtr.Zero)
            {
                this.Files.Address = IntPtr.Zero;
            }
            else
            {
                var data = Core.Process.Handle.ReadMemory<GameFilesStaticObject>(newAddress);
                this.Files.Address = data.FilesList.Head;
            }
        }
    }
}
