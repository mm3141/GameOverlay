// <copyright file="Render.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;
    using GameOffsets.Natives;
    using GameOffsets.Objects.Components;
    using ImGuiNET;

    /// <summary>
    /// The <see cref="Render"/> component in the entity.
    /// </summary>
    public class Render : RemoteObjectBase
    {
        private float terrainHeight = 0x00;
        private StdTuple3D<float> modelBounds = default;

        /// <summary>
        /// Initializes a new instance of the <see cref="Render"/> class.
        /// </summary>
        /// <param name="address">address of the <see cref="Render"/> component.</param>
        public Render(IntPtr address)
            : base(address, true)
        {
        }

        /// <summary>
        /// Gets the postion where entity is rendered in the game world.
        /// </summary>
        public StdTuple3D<float> WorldPosition3D { get; private set; } = default;

        /// <summary>
        /// Converts the <see cref="Render"/> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"World Position: {this.WorldPosition3D}");
            ImGui.Text($"Terrain Height (Z-Axis): {this.terrainHeight}");
            ImGui.Text($"Model Bonds: {this.modelBounds}");
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
            var data = reader.ReadMemory<RenderOffsets>(this.Address);
            this.WorldPosition3D = data.CurrentWorldPosition;
            this.modelBounds = data.CharactorModelBounds;
            this.terrainHeight = data.TerrainHeight;
        }
    }
}
