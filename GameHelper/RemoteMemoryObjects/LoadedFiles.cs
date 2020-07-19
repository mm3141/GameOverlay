// <copyright file="LoadedFiles.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteMemoryObjects
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Coroutine;
    using GameOffsets.Native;
    using GameOffsets.RemoteMemoryObjects;

    /// <summary>
    /// Gathers the files loaded in the game for the current area.
    /// </summary>
    public class LoadedFiles : RemoteMemoryObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedFiles"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        public LoadedFiles(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.GatherData());
        }

        /// <summary>
        /// Gets the files.
        /// </summary>
        public ConcurrentBag<string> Data { get; private set; } = new ConcurrentBag<string>();

        /// <inheritdoc/>
        protected override IEnumerator<Wait> GatherData()
        {
            while (true)
            {
                yield return new Wait(Core.States.AreaLoading.AreaChanged);
                if (this.Address == IntPtr.Zero)
                {
                    continue;
                }

                this.Data.Clear();
                for (int i = 0; i < 2; i++)
                {
                    var totalFiles = LoadedFilesRootObjectOffset.TotalCount;
                    var reader = Core.Process.Handle;
                    var filesRootObjs = reader.ReadMemoryArray<LoadedFilesRootObjectOffset>(this.Address, totalFiles);
                    if (filesRootObjs[0].FilesList.Head == IntPtr.Zero)
                    {
                        throw new Exception("Couldn't read LoadedFilesRootObjectOffset array " +
                                            $"from FileRoot address: {this.Address.ToInt64():X}");
                    }

                    Parallel.For(0, totalFiles, (i) =>
                    {
                        var files = filesRootObjs[i].FilesList;
                        var currNode = reader.ReadMemory<StdListNode<FileInfoPtr>>(files.Head);
                        var lastNode = reader.ReadMemory<StdListNode<FileInfoPtr>>(currNode.Previous);
                        if (lastNode.Data.NamePtr == IntPtr.Zero)
                        {
                            throw new Exception($"Couldn't get lastNode of index {i} from FileRoot " +
                                                $"address: {this.Address.ToInt64():X}");
                        }

                        byte firstChar = 0x00;
                        int fileNumber = 0x00;
                        FileInfo information;
                        string name;
                        while (currNode.Data.NamePtr != lastNode.Data.NamePtr)
                        {
                            currNode = reader.ReadMemory<StdListNode<FileInfoPtr>>(currNode.Next);
                            fileNumber++;
                            firstChar = reader.ReadMemory<byte>(currNode.Data.NamePtr);
                            if (firstChar == 0x00)
                            {
                                throw new Exception("Couldn't read firstChar of the file name @ " +
                                                    $"FileRootAddress: {this.Address.ToInt64():X} " +
                                                    $"Index: {i} " +
                                                    $"Depth: {fileNumber}");
                            }

                            switch (firstChar)
                            {
                                case (byte)'S': // ShaderProgram
                                case (byte)'T': // TextureResource
                                case (byte)'F': // FONTFontin
                                case (byte)'D': // Data
                                case (byte)'a': // audio
                                case (byte)'A': // Art
                                    break;
                                default:
                                    information = reader.ReadMemory<FileInfo>(currNode.Data.Information);
                                    if (information.AreaChangeCount > 2 &&
                                        information.AreaChangeCount == Core.AreaChangeCounter.Value &&
                                        information.FileType == 0x01)
                                    {
                                        name = reader.ReadStdWString(information.Name);
                                        this.Data.Add(name);
                                    }

                                    break;
                            }
                        }
                    });

                    yield return new Wait(5);
                }
            }
        }
    }
}
