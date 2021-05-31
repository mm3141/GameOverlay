// <copyright file="LoadedFiles.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.Utils;
    using GameOffsets.Objects;
    using ImGuiNET;

    /// <summary>
    /// Gathers the files loaded in the game for the current area.
    /// </summary>
    public class LoadedFiles : RemoteObjectBase
    {
        private string areaHashCache = string.Empty;
        private bool areaAlreadyDone = false;
        private string filename = string.Empty;
        private string searchText = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedFiles"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal LoadedFiles(IntPtr address)
            : base(address)
        {
            Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(
                this.OnAreaChange(), "[LoadedFiles] Gather Preload Data", int.MaxValue - 1));
        }

        /// <summary>
        /// Gets the pathname of the files.
        /// </summary>
        public ConcurrentDictionary<string, int> PathNames
        {
            get;
            private set;
        }

        = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// Converts the <see cref="LoadedFiles"/> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Total Loaded Files in current area: {this.PathNames.Count}");
            ImGui.Text("File Name: ");
            ImGui.SameLine();
            ImGui.InputText("##filename", ref this.filename, 100);
            ImGui.SameLine();
            if (!this.areaAlreadyDone)
            {
                if (ImGui.Button("Save"))
                {
                    string dir_name = "preload_dumps";
                    Directory.CreateDirectory(dir_name);
                    var dataToWrite = this.PathNames.Keys.ToList();
                    dataToWrite.Sort();
                    File.WriteAllText(
                        Path.Join(dir_name, this.filename),
                        string.Join("\n", dataToWrite));
                    this.areaAlreadyDone = true;
                }
            }
            else
            {
                UiHelper.DrawDisabledButton("Save");
            }

            ImGui.Text("Search:    ");
            ImGui.SameLine();
            if (ImGui.InputText("##LoadedFiles", ref this.searchText, 50))
            {
                this.searchText = this.searchText.ToLower();
            }

            if (!string.IsNullOrEmpty(this.searchText))
            {
                ImGui.BeginChild("Result##loadedfiles", Vector2.Zero, true);
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
                foreach (var pathname in this.PathNames.Keys)
                {
                    if (pathname.ToLower().Contains(this.searchText))
                    {
                        if (ImGui.SmallButton(pathname))
                        {
                            ImGui.SetClipboardText(pathname);
                        }
                    }
                }

                ImGui.PopStyleColor();
                ImGui.EndChild();
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.PathNames.Clear();
            this.areaHashCache = string.Empty;
            this.areaAlreadyDone = false;
            this.filename = string.Empty;
            this.searchText = string.Empty;
        }

        /// <summary>
        /// this function is overrided to do nothing because it's done @ AreaChange event.
        /// </summary>
        /// <param name="hasAddressChanged">ignore me.</param>
        protected override void UpdateData(bool hasAddressChanged)
        {
        }

        private LoadedFilesRootObject[] GetAllPointers()
        {
            var totalFiles = LoadedFilesRootObject.TotalCount;
            var reader = Core.Process.Handle;
            return reader.ReadMemoryArray<LoadedFilesRootObject>(this.Address, totalFiles);
        }

        private void ScanAllBucketsParallel(SafeMemoryHandle reader, LoadedFilesRootObject filesRootObj)
        {
            if (filesRootObj.FilesArray == IntPtr.Zero)
            {
                throw new Exception("Couldn't read LoadedFilesRootObject array " +
                    $"from FileRoot address: {this.Address.ToInt64():X}");
            }

            if (filesRootObj.FilesPointerStructureCapacity != LoadedFilesRootObject.Capacity)
            {
                throw new Exception($"Looks like Capacity changed to" +
                    $"{filesRootObj.FilesPointerStructureCapacity} " +
                    $"from {LoadedFilesRootObject.Capacity}");
            }

            // Max buckets can be calculated from FilesPointerStructureCapacity
            // but I don't expect it to ever change unless GGG change their
            // FileStoring algorithm. With that being said, I prefer known constants
            // rather than calculations. How to calculate this constant is given in
            // FilesArrayStructure.MaximumBuckets description.
            var filesPtr = reader.ReadMemoryArray<FilesArrayStructure>(
                filesRootObj.FilesArray,
                FilesArrayStructure.MaximumBuckets);
            Parallel.For(0, filesPtr.Length, (i) =>
            {
                var fileNode = filesPtr[i];
                if (fileNode.Flag0 != FilesArrayStructure.InValidPointerFlagValue)
                {
                    this.AddFileIfLoadedInCurrentArea(reader, fileNode.Pointer0.FilesPointer);
                }

                if (fileNode.Flag1 != FilesArrayStructure.InValidPointerFlagValue)
                {
                    this.AddFileIfLoadedInCurrentArea(reader, fileNode.Pointer1.FilesPointer);
                }

                if (fileNode.Flag2 != FilesArrayStructure.InValidPointerFlagValue)
                {
                    this.AddFileIfLoadedInCurrentArea(reader, fileNode.Pointer2.FilesPointer);
                }

                if (fileNode.Flag3 != FilesArrayStructure.InValidPointerFlagValue)
                {
                    this.AddFileIfLoadedInCurrentArea(reader, fileNode.Pointer3.FilesPointer);
                }

                if (fileNode.Flag4 != FilesArrayStructure.InValidPointerFlagValue)
                {
                    this.AddFileIfLoadedInCurrentArea(reader, fileNode.Pointer4.FilesPointer);
                }

                if (fileNode.Flag5 != FilesArrayStructure.InValidPointerFlagValue)
                {
                    this.AddFileIfLoadedInCurrentArea(reader, fileNode.Pointer5.FilesPointer);
                }

                if (fileNode.Flag6 != FilesArrayStructure.InValidPointerFlagValue)
                {
                    this.AddFileIfLoadedInCurrentArea(reader, fileNode.Pointer6.FilesPointer);
                }

                if (fileNode.Flag7 != FilesArrayStructure.InValidPointerFlagValue)
                {
                    this.AddFileIfLoadedInCurrentArea(reader, fileNode.Pointer7.FilesPointer);
                }
            });
        }

        private void AddFileIfLoadedInCurrentArea(SafeMemoryHandle reader, IntPtr address)
        {
            var information = reader.ReadMemory<FileInfoValueStruct>(address);
            if (information.AreaChangeCount > FileInfoValueStruct.IGNORE_FIRST_X_AREAS &&
            information.AreaChangeCount == Core.AreaChangeCounter.Value)
            {
                var name = reader.ReadStdWString(information.Name);
                this.PathNames[name] = information.AreaChangeCount;
            }
        }

        private IEnumerator<Wait> OnAreaChange()
        {
            while (true)
            {
                yield return new Wait(RemoteEvents.AreaChanged);
                if (this.Address != IntPtr.Zero)
                {
                    var areaHash = Core.States.InGameStateObject.CurrentAreaInstance.AreaHash;
                    var iH = Core.States.InGameStateObject.CurrentAreaInstance.AreaDetails.IsHideout;
                    var iT = Core.States.InGameStateObject.CurrentAreaInstance.AreaDetails.IsTown;
                    var name = Core.States.AreaLoading.CurrentAreaName;
                    if (iH || iT || areaHash == this.areaHashCache)
                    {
                        continue;
                    }

                    this.CleanUpData();
                    this.filename = $"{name}_{areaHash}.txt";
                    this.areaAlreadyDone = false;
                    this.areaHashCache = areaHash;

                    var filesRootObjs = this.GetAllPointers();
                    var reader = Core.Process.Handle;
                    for (int i = 0; i < filesRootObjs.Length; i++)
                    {
                        this.ScanAllBucketsParallel(reader, filesRootObjs[i]);
                        yield return new Wait(0d);
                    }

                    CoroutineHandler.RaiseEvent(HybridEvents.PreloadsUpdated);
                }
            }
        }
    }
}
