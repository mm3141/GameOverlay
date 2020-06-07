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
        }

        /// <summary>
        /// Gets the list of (Metadata only) files loaded for the current Area.
        /// </summary>
        public HashSet<string> CurrentAreaFiles { get; private set; } = new HashSet<string>(2000);

        /// <inheritdoc/>
        protected override IEnumerator<IWait> GatherData()
        {
            while (true)
            {
                yield return new WaitEvent(Core.States.AreaLoading.AreaChanged);
                this.CurrentAreaFiles.Clear();
                for (int i = 0; i < 5; i++)
                {
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
                        byte firstChar;
                        while (filelistNode.Data.NamePtr != lastFileNode.Data.NamePtr)
                        {
                            filelistNode = reader.ReadMemory<StdListNode<FileInfoPtr>>(filelistNode.Next);
                            firstChar = reader.ReadMemory<byte>(filelistNode.Data.NamePtr);
                            switch (firstChar)
                            {
                                case 0x53: // ShaderProgram
                                case 0x54: // TextureResource
                                case 0x46: // FONTFontin
                                case 0x44: // Data
                                    break;
                                default:
                                    information = reader.ReadMemory<FileInfo>(filelistNode.Data.Information);
                                    if (information.AreaChangeCount > 2 &&
                                        information.AreaChangeCount == Core.AreaChangeCounter.Value &&
                                        information.FileType == 0x01)
                                    {
                                        name = reader.ReadStdWString(information.Name);
                                        this.CurrentAreaFiles.Add(name);
                                    }

                                    break;
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

                    yield return new WaitSeconds(5);
                }
            }
        }
    }
}
