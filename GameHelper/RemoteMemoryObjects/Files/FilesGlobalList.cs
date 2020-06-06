// <copyright file="FilesGlobalList.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteMemoryObjects.Files
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameOffsets.Native;
    using GameOffsets.RemoteMemoryObjects.Files;

    /// <summary>
    /// Points and read the global list Of files loaded in the memory of the game.
    /// </summary>
    public sealed class FilesGlobalList : RemoteMemoryObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilesGlobalList"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        public FilesGlobalList(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.GatherData());
            CoroutineHandler.Start(this.ClearLoadedFiles());
        }

        /// <summary>
        /// Gets the list of (Metadata only) files loaded for the current Area.
        /// </summary>
        public List<string> CurrentAreaFiles { get; private set; } = new List<string>(2000);

        /// <inheritdoc/>
        protected override IEnumerator<IWait> GatherData()
        {
            while (true)
            {
                yield return new WaitSeconds(10);
                if (this.Address != IntPtr.Zero)
                {
                    var reader = Core.Process.Handle;
                    var filelistNode = reader.ReadMemory<StdListNode<FileInfoPtr>>(this.Address);
                    var lastFileNode = reader.ReadMemory<StdListNode<FileInfoPtr>>(filelistNode.Previous);
                    if (lastFileNode.Data.NamePtr == IntPtr.Zero)
                    {
                        throw new Exception("Could not read LastFile Node." +
                            $"Address: {Core.Files.Address.ToInt64():X}");
                    }

                    int currentFileNumber = 0;
                    FileInfo information;
                    string name;
                    while (filelistNode.Data.NamePtr != lastFileNode.Data.NamePtr)
                    {
                        filelistNode = reader.ReadMemory<StdListNode<FileInfoPtr>>(filelistNode.Next);
                        information = reader.ReadMemory<FileInfo>(filelistNode.Data.Information);
                        if (information.AreaChangeCount > 2 && information.AreaChangeCount == Core.AreaChangeCounter.Value)
                        {
                            name = reader.ReadStdWString(information.Name);
                            if (name.StartsWith("Metadata") && !this.CurrentAreaFiles.Contains(name))
                            {
                                this.CurrentAreaFiles.Add(name);
                            }
                        }

                        currentFileNumber++;
                        if (currentFileNumber % 1000 == 0)
                        {
                            yield return new WaitSeconds(0);
                        }

                        if (currentFileNumber > 80000)
                        {
                            throw new Exception("Number of files loaded in memory is larger than" +
                                " 80,000. The FilesGlobalList remote memory object must be stuck" +
                                " in an infinite loop. Files Controller " +
                                $"Address: {Core.Files.Address.ToInt64():X}");
                        }
                    }
                }
            }
        }

        private IEnumerator<IWait> ClearLoadedFiles()
        {
            while (true)
            {
                yield return new WaitEvent(Core.States.AreaLoading.AreaChanged);
                this.CurrentAreaFiles.Clear();
            }
        }
    }
}
