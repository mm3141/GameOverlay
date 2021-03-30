// <copyright file="Chest.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;
    using GameOffsets.Objects.Components;
    using ImGuiNET;

    /// <summary>
    /// The <see cref="Chest"/> component in the entity.
    /// </summary>
    public class Chest : RemoteObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Chest"/> class.
        /// </summary>
        /// <param name="address">address of the <see cref="Chest"/> component.</param>
        public Chest(IntPtr address)
            : base(address, true)
        {
        }

        /// <summary>
        /// Gets a value indicating whether chest is opened or not.
        /// </summary>
        public bool IsOpened { get; private set; } = false;

        /// <summary>
        /// Converts the <see cref="Chest"/> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"IsOpened: {this.IsOpened}");
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            throw new Exception("Component Address should never be Zero.");
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<ChestOffsets>(this.Address);
            this.IsOpened = data.IsOpened;
        }
    }
}
