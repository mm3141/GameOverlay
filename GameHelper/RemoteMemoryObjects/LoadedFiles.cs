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
    using GameHelper.RemoteEnums;
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
        internal LoadedFiles(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnAreaChange());
            CoroutineHandler.Start(this.OnGameStateChange());
        }

        /// <summary>
        /// Gets the wait (in seconds) between multiple preload memory scans.
        /// As preloads are scanned multiple times (i.e. <see cref="MaximumPreloadScans"/>).
        /// </summary>
        public static int WaitBetweenScans { get; } = 3;

        /// <summary>
        /// Gets the maxiumum number of preload scans that can happen in a given area.
        /// </summary>
        public static int MaximumPreloadScans { get; } = 2;

        /// <summary>
        /// Gets the current iteration of preload scan. Minimum value can be 0, maxiumum value
        /// can be <see cref="MaximumPreloadScans"/>.
        /// </summary>
        public int CurrentPreloadScan { get; private set; } = 0;

        /// <summary>
        /// Gets the files.
        /// </summary>
        public ConcurrentBag<string> Data { get; private set; } = new ConcurrentBag<string>();

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            if (this.Data.Count > 0)
            {
                this.Data.Clear();
            }

            this.CurrentPreloadScan = 0;
        }

        /// <inheritdoc/>
        protected override void GatherData()
        {
            var totalFiles = LoadedFilesRootObject.TotalCount;
            var reader = Core.Process.Handle;
            var filesRootObjs = reader.ReadMemoryArray<LoadedFilesRootObject>(this.Address, totalFiles);
            if (filesRootObjs.Length > 0)
            {
                if (filesRootObjs[0].FilesList.Head == IntPtr.Zero ||
                    filesRootObjs[0].FilesVectorUseless.First == IntPtr.Zero ||
                    filesRootObjs[0].FilesVectorUseless.Last == IntPtr.Zero ||
                    filesRootObjs[0].FilesVectorUseless.End == IntPtr.Zero ||
                    filesRootObjs[0].IsValid != 1f)
                {
                    throw new Exception("Couldn't read LoadedFilesRootObject array " +
                                        $"from FileRoot address: {this.Address.ToInt64():X}");
                }

                for (int k = 0; k < totalFiles; k++)
                {
                    switch (filesRootObjs[k].TemplateId2)
                    {
                        case 64:
                        case 512:
                        case 1024:
                            break;
                        default:
                            throw new Exception($"New template found (in index {k}) " +
                                $"(templateId {filesRootObjs[k].TemplateId1}," +
                                $"{filesRootObjs[k].TemplateId2}) in " +
                                $"LoadedFilesRootObject object at " +
                                $"address: {this.Address.ToInt64():X}.");
                    }
                }

                Parallel.For(0, totalFiles, (i) =>
                {
                    var filesListPtrAddress = filesRootObjs[i].FilesList.Head;

                    // Currently working, otherwise 0 - 127 1 type (Name)
                    // and 128 - 255 other type (Name2)
                    var templateId2 = filesRootObjs[i].TemplateId2;
                    var currNodeAddress = reader.ReadMemory<StdListNode>(filesListPtrAddress).Next;
                    StdListNode<StdKeyValuePair> currNode;
                    FileInfoValueStruct information;
                    while (currNodeAddress != filesListPtrAddress)
                    {
                        currNode = reader.ReadMemory<StdListNode<StdKeyValuePair>>(currNodeAddress);
                        if (currNodeAddress == IntPtr.Zero)
                        {
                            Console.WriteLine("Terminating Preloads finding because of" +
                                "unexpected 0x00 found. This is normal if it happens " +
                                "after closing the game, otherwise report it.");
                            break;
                        }

                        information = reader.ReadMemory<FileInfoValueStruct>(currNode.Data.ValuePtr);
                        if (information.AreaChangeCount >= 2 &&
                            information.AreaChangeCount == Core.AreaChangeCounter.Value)
                        {
                            this.Data.Add(reader.ReadStdWString(
                                templateId2 == 64 ? information.Name2 : information.Name));
                        }

                        currNodeAddress = currNode.Next;
                    }
                });

                this.CurrentPreloadScan++;
            }
        }

        private IEnumerator<Wait> OnAreaChange()
        {
            yield return new Wait(0);
            while (true)
            {
                yield return new Wait(Core.States.AreaLoading.AreaChanged);
                if (this.Address != IntPtr.Zero)
                {
                    this.CleanUpData();
                    for (int i = 0; i < MaximumPreloadScans; i++)
                    {
                        this.Data.Clear();
                        this.GatherData();
                        if (i < MaximumPreloadScans - 1)
                        {
                            yield return new Wait(WaitBetweenScans);
                        }
                    }
                }
            }
        }

        private IEnumerator<Wait> OnGameStateChange()
        {
            yield return new Wait(0);
            while (true)
            {
                yield return new Wait(Core.States.CurrentStateInGame.StateChanged);
                if (Core.States.CurrentStateInGame.Name != GameStateTypes.InGameState
                    && Core.States.CurrentStateInGame.Name != GameStateTypes.EscapeState
                    && Core.States.CurrentStateInGame.Name != GameStateTypes.AreaLoadingState)
                {
                    this.CleanUpData();
                }
            }
        }
    }
}
