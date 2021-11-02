// <copyright file="ObjectMagicProperties.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;
    using GameOffsets.Objects.Components;
    using ImGuiNET;
    using RemoteEnums;

    /// <summary>
    ///     ObjectMagicProperties component of the entity.
    /// </summary>
    public class ObjectMagicProperties : RemoteObjectBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ObjectMagicProperties" /> class.
        /// </summary>
        /// <param name="address">address of the <see cref="ObjectMagicProperties" /> component.</param>
        public ObjectMagicProperties(IntPtr address)
            : base(address, true) { }

        /// <summary>
        ///     Gets a value indicating entity rarity information.
        /// </summary>
        public Rarity Rarity { get; private set; } = Rarity.Normal;

        /// <summary>
        ///     Converts the <see cref="ObjectMagicProperties" /> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Rarity: {this.Rarity}");
        }

        /// <inheritdoc />
        protected override void CleanUpData()
        {
            throw new Exception("Component Address should never be Zero.");
        }

        /// <inheritdoc />
        protected override void UpdateData(bool hasAddressChanged)
        {
            if (hasAddressChanged)
            {
                var reader = Core.Process.Handle;
                var data = reader.ReadMemory<ObjectMagicPropertiesOffsets>(this.Address);
                this.Rarity = (Rarity)data.Rarity;
            }
        }
    }
}