// <copyright file="WorldData.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.RemoteObjects.FilesStructures;
    using GameOffsets.Natives;
    using GameOffsets.Objects.States.InGameState;
    using ImGuiNET;

    /// <summary>
    ///     [3].
    /// </summary>
    public class WorldData : RemoteObjectBase
    {
        private IntPtr areaDetailsPtrCache = IntPtr.Zero;
        /// <summary>
        ///     Gets the World to Screen Matrix.
        /// </summary>
        private Matrix4x4 worldToScreenMatrix = Matrix4x4.Identity;

        /// <summary>
        ///     Gets the Area Details.
        /// </summary>
        public WorldAreaDat AreaDetails { get; } = new(IntPtr.Zero);

        /// <summary>
        ///     Converts the World position to Screen location.
        /// </summary>
        /// <param name="worldPosition">3D world position of the entity.</param>
        /// <returns>screen location of the entity.</returns>
        public Vector2 WorldToScreen(StdTuple3D<float> worldPosition)
        {
            var result = Vector2.Zero;
            if (this.Address == IntPtr.Zero)
            {
                return result;
            }

            Vector4 temp0 = new(worldPosition.X, worldPosition.Y, worldPosition.Z, 1.0f);
            temp0 = Vector4.Transform(temp0, this.worldToScreenMatrix);
            temp0 /= temp0.W;
            result.X = (temp0.X + 1.0f) * (Core.Process.WindowArea.Width / 2.0f);
            result.Y = (1.0f - temp0.Y) * (Core.Process.WindowArea.Height / 2.0f);
            return result;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WorldData" /> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal WorldData(IntPtr address)
            : base(address)
        {
            Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(
                this.OnPerFrame(), "[AreaInstance] Update World Data", int.MaxValue - 3));
        }

        /// <summary>
        ///     Converts the <see cref="WorldData" /> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            if (ImGui.TreeNode("WindowToScreenMatrix"))
            {
                var d = this.worldToScreenMatrix;
                ImGui.Text($"{d.M11:0.00}\t{d.M12:0.00}\t{d.M13:0.00}\t{d.M14:0.00}");
                ImGui.Text($"{d.M21:0.00}\t{d.M22:0.00}\t{d.M23:0.00}\t{d.M24:0.00}");
                ImGui.Text($"{d.M31:0.00}\t{d.M32:0.00}\t{d.M33:0.00}\t{d.M34:0.00}");
                ImGui.Text($"{d.M41:0.00}\t{d.M42:0.00}\t{d.M43:0.00}\t{d.M44:0.00}");
                ImGui.TreePop();
            }
        }

        /// <inheritdoc />
        protected override void CleanUpData()
        {
            this.areaDetailsPtrCache = IntPtr.Zero;
            this.AreaDetails.Address = IntPtr.Zero;
            this.worldToScreenMatrix = Matrix4x4.Identity;
        }

        /// <inheritdoc />
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<WorldDataOffset>(this.Address);
            if (this.areaDetailsPtrCache != data.WorldAreaDetailsPtr)
            {
                var areaInfo = reader.ReadMemory<WorldAreaDetailsStruct>(data.WorldAreaDetailsPtr);
                this.AreaDetails.Address = areaInfo.WorldAreaDetailsRowPtr;
                this.areaDetailsPtrCache = data.WorldAreaDetailsPtr;
            }

            this.worldToScreenMatrix = data.CameraStructurePtr.WorldToScreenMatrix;
        }

        private IEnumerator<Wait> OnPerFrame()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.PerFrameDataUpdate);
                if (this.Address != IntPtr.Zero)
                {
                    this.UpdateData(false);
                }
            }
        }
    }
}
