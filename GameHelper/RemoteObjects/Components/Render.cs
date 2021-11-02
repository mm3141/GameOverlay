// <copyright file="Render.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;
    using GameOffsets.Natives;
    using GameOffsets.Objects.Components;
    using GameOffsets.Objects.States.InGameState;
    using ImGuiNET;

    /// <summary>
    ///     The <see cref="Render" /> component in the entity.
    /// </summary>
    public class Render : RemoteObjectBase
    {
        private static readonly float WorldToGridRatio =
            TileStructure.TileToWorldConversion / TileStructure.TileToGridConversion;

        private StdTuple3D<float> gridPos2D;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Render" /> class.
        /// </summary>
        /// <param name="address">address of the <see cref="Render" /> component.</param>
        public Render(IntPtr address)
            : base(address, true) { }

        /// <summary>
        ///     Gets the position where entity is located on the grid (map).
        /// </summary>
        public StdTuple3D<float> GridPosition
        {
            get => this.gridPos2D;
            private set => this.gridPos2D = value;
        }

        /// <summary>
        ///     Gets the position where entity is located on the grid (map).
        /// </summary>
        public StdTuple3D<float> ModelBounds { get; private set; }

        /// <summary>
        ///     Gets the postion where entity is rendered in the game world.
        ///     NOTE: Z-Axis is pointing to the (visible/invisible) healthbar.
        /// </summary>
        public StdTuple3D<float> WorldPosition { get; private set; }

        /// <summary>
        ///     Gets the terrain height on which the Entity is standing.
        /// </summary>
        public float TerrainHeight { get; private set; }

        /// <summary>
        ///     Converts the <see cref="Render" /> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Grid Position: {this.gridPos2D}");
            ImGui.Text($"World Position: {this.WorldPosition}");
            ImGui.Text($"Terrain Height (Z-Axis): {this.TerrainHeight}");
            ImGui.Text($"Model Bounds: {this.ModelBounds}");
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
            var data = reader.ReadMemory<RenderOffsets>(this.Address);
            this.WorldPosition = data.CurrentWorldPosition;
            this.ModelBounds = data.CharactorModelBounds;
            this.TerrainHeight = (float)Math.Round(data.TerrainHeight, 4);
            this.gridPos2D.X = data.CurrentWorldPosition.X / WorldToGridRatio;
            this.gridPos2D.Y = data.CurrentWorldPosition.Y / WorldToGridRatio;
        }
    }
}