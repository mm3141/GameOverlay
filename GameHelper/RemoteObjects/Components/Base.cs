// <copyright file="Base.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Coroutine;
    using GameOffsets.Objects.Components;
    using GameOffsets.Objects.FilesStructures;
    using ImGuiNET;

    /// <summary>
    ///     The <see cref="Base" /> component in the entity.
    /// </summary>
    public class Base : RemoteObjectBase
    {
        /// <summary>
        ///     Cache the BaseItemType.Dat data to save few reads per frame.
        /// </summary>
        private static readonly ConcurrentDictionary<IntPtr, string> BaseItemTypeDatCache = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Base" /> class.
        /// </summary>
        /// <param name="address">address of the <see cref="Base" /> component.</param>
        public Base(IntPtr address)
            : base(address, true) { }

        static Base()
        {
            CoroutineHandler.Start(OnGameClose());
        }

        /// <summary>
        ///     Gets the items base name.
        /// </summary>
        public string ItemBaseName { get; private set; } = string.Empty;

        /// <inheritdoc />
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Base Name: {this.ItemBaseName}");
        }

        /// <inheritdoc />
        protected override void CleanUpData()
        {
            throw new Exception("Component Address should never be Zero.");
        }

        /// <inheritdoc />
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<BaseOffsets>(this.Address);
            if (BaseItemTypeDatCache.TryGetValue(data.BaseInternalPtr, out var itemName))
            {
                this.ItemBaseName = itemName;
            }
            else
            {
                var baseItemTypeDatRow = reader.ReadMemory<BaseItemTypesDatOffsets>(data.BaseInternalPtr);
                var name = reader.ReadStdWString(baseItemTypeDatRow.BaseNamePtr);
                if (!string.IsNullOrEmpty(name))
                {
                    BaseItemTypeDatCache[data.BaseInternalPtr] = name;
                    this.ItemBaseName = name;
                }
            }
        }

        private static IEnumerable<Wait> OnGameClose()
        {
            while (true)
            {
                yield return new(CoroutineEvents.GameHelperEvents.OnClose);
                BaseItemTypeDatCache.Clear();
            }
        }
    }
}