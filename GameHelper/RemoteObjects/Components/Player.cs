// <copyright file="Player.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;
    using GameOffsets.Objects.Components;
    using ImGuiNET;

    /// <summary>
    ///     The <see cref="Chest" /> component in the entity.
    /// </summary>
    public class Player : RemoteObjectBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Player" /> class.
        /// </summary>
        /// <param name="address">address of the <see cref="Chest" /> component.</param>
        public Player(IntPtr address)
            : base(address, true) { }

        /// <summary>
        ///     Gets the name of the player.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Converts the <see cref="Player" /> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Player Name: {this.Name}");
        }

        /// <inheritdoc />
        protected override void CleanUpData()
        {
            throw new Exception("Component Address should never be Zero.");
        }

        /// <inheritdoc />
        protected override void UpdateData(bool hasAddressChanged)
        {
            if (!hasAddressChanged)
            {
                return;
            }

            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<PlayerOffsets>(this.Address);
            this.Name = reader.ReadStdWString(data.Name);
        }
    }
}