// <copyright file="LoadedFiles.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Coroutine;
    using GameHelper.RemoteEnums;
    using GameOffsets.Objects;

    /// <summary>
    /// Gathers the files loaded in the game for the current area.
    /// </summary>
    public class LoadedFiles : RemoteObjectBase
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
        /// Gets the pathname of the files.
        /// </summary>
        public ConcurrentDictionary<string, byte> PathNames
        {
            get;
            private set;
        }

        = new ConcurrentDictionary<string, byte>();

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.PathNames.Clear();
        }

        /// <inheritdoc/>
        protected override void UpdateData()
        {
            var totalFiles = LoadedFilesRootObject.TotalCount;
            var reader = Core.Process.Handle;
            var filesRootObjs = reader.ReadMemoryArray<LoadedFilesRootObject>(this.Address, totalFiles);
            Parallel.For(0, filesRootObjs.Length, (i) =>
            {
                var filesRootObj = filesRootObjs[i];
                if (filesRootObj.FilesList.Head == IntPtr.Zero || filesRootObj.IsValid != 1f)
                {
                    throw new Exception("Couldn't read LoadedFilesRootObject array " +
                        $"from FileRoot address: {this.Address.ToInt64():X}");
                }

                switch (filesRootObj.TemplateId2)
                {
                    case 512:
                    case 1024:
                        break;
                    default:
                        throw new Exception($"New template found (in index {i}) " +
                            $"(templateId {filesRootObj.TemplateId1}," +
                            $"{filesRootObj.TemplateId2}) in " +
                            $"LoadedFilesRootObject object at " +
                            $"address: {this.Address.ToInt64():X}.");
                }

                var filesPtr = reader.ReadStdList<FilesKeyValueStruct>(filesRootObj.FilesList);
                for (int j = 0; j < filesPtr.Count; j++)
                {
                    var fileNode = filesPtr[j];
                    var information = reader.ReadMemory<FileInfoValueStruct>(fileNode.ValuePtr);
                    if (information.AreaChangeCount >= 2 &&
                    information.AreaChangeCount == Core.AreaChangeCounter.Value)
                    {
                        var name = reader.ReadStdWString(information.Name);
                        this.PathNames[name] = 0x00;
                    }
                }
            });
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
                    this.UpdateData();
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
