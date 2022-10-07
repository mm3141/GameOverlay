// <copyright file="Targetable.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;
    using GameOffsets.Objects.Components;
    using ImGuiNET;

    /// <summary>
    ///     The <see cref="Targetable" /> component in the entity.
    /// </summary>
    public class Targetable : RemoteObjectBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Targetable" /> class.
        /// </summary>
        /// <param name="address">address of the <see cref="Targetable" /> component.</param>
        public Targetable(IntPtr address)
            : base(address, false) { }

        /// <summary>
        ///     Gets a value indicating whether the entity is targetable or not.
        /// </summary>
        public bool IsTargetable { get; private set; }

        /// <summary>
        ///     Converts the <see cref="Targetable" /> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Is Targetable: {this.IsTargetable}");
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
            var data = reader.ReadMemory<TargetableOffsets>(this.Address);
            this.IsTargetable = data.isTargetable;
        }
    }
}