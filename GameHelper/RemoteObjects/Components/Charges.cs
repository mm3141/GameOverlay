// <copyright file="Charges.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;
    using GameOffsets.Objects.Components;
    using ImGuiNET;

    /// <summary>
    ///     The <see cref="Charges" /> component in the entity.
    /// </summary>
    public class Charges : RemoteObjectBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Charges" /> class.
        /// </summary>
        /// <param name="address">address of the <see cref="Charges" /> component.</param>
        public Charges(IntPtr address)
            : base(address, true) { }

        /// <summary>
        ///     Gets a value indicating number of charges the flask has.
        /// </summary>
        public int Current { get; private set; }

        /// <summary>
        ///     Converts the <see cref="Charges" /> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Current Charges: {this.Current}");
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
            var data = reader.ReadMemory<ChargesOffsets>(this.Address);
            this.Current = data.current;
        }
    }
}