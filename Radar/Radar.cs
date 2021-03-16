// <copyright file="Radar.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Radar
{
    using System;
    using System.Numerics;
    using GameHelper;
    using GameHelper.Plugin;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// <see cref="Radar"/> plugin.
    /// </summary>
    public sealed class Radar : PCore<RadarSettings>
    {
        /// <inheritdoc/>
        public override void DrawSettings()
        {
        }

        /// <inheritdoc/>
        public override void DrawUI()
        {
            if (Core.States.GameCurrentState != GameStateTypes.InGameState)
            {
                return;
            }

            if (Core.States.InGameStateObject.GameUi.LargeMap.IsVisible)
            {
                this.DrawOnLargeMap();
            }

            if (Core.States.InGameStateObject.GameUi.MiniMap.IsVisible)
            {
                this.DrawOnMiniMap();
            }
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
        }

        /// <inheritdoc/>
        public override void OnEnable()
        {
        }

        /// <inheritdoc/>
        public override void SaveSettings()
        {
        }

        private void DrawOnMiniMap()
        {
            var miniMapUiElement = Core.States.InGameStateObject.GameUi.MiniMap;
            var miniMapCenter = miniMapUiElement.Postion + (miniMapUiElement.Size / 2);
            var diag = Math.Sqrt((miniMapUiElement.Size.X * miniMapUiElement.Size.X) +
                (miniMapUiElement.Size.Y * miniMapUiElement.Size.Y)) / 2f;
            Core.States.InGameStateObject.CurrentAreaInstance.Player.TryGetComponent<Positioned>(out var playerPos);
            Core.States.InGameStateObject.CurrentAreaInstance.Player.TryGetComponent<Render>(out var playerZ);
            var pZ = playerZ.TerrainHeight;
            var pPos = playerPos.GridPosition;
            var pPosV = new Vector2(pPos.X, pPos.Y);
            foreach (var entity in Core.States.InGameStateObject.CurrentAreaInstance.AwakeEntities)
            {
                if (!entity.Value.TryGetComponent<Positioned>(out var entityPos))
                {
                    continue;
                }

                if (!entity.Value.TryGetComponent<Render>(out var entityZ))
                {
                    continue;
                }

                var ePos = entityPos.GridPosition;
                float eZ = entityZ.TerrainHeight;
                var ePosV = new Vector2(ePos.X, ePos.Y);
                Helper.DiagonalLength = diag;
                Helper.Scale = miniMapUiElement.Zoom;
                var fpos = Helper.DeltaInWorldToMinimapDelta(ePosV - pPosV, eZ - pZ);
                var fgDraw = ImGui.GetForegroundDrawList();
                fgDraw.AddCircleFilled(miniMapCenter + fpos, 3f, UiHelper.Color(255, 0, 0, 255));
            }
        }

        private void DrawOnLargeMap()
        {
            var diag = 1468.605f;
            var center = Core.States.InGameStateObject.GameUi.LargeMap.Postion +
                Core.States.InGameStateObject.GameUi.LargeMap.DefaultShift +
                Core.States.InGameStateObject.GameUi.LargeMap.Shift;
            Helper.DiagonalLength = diag;
            Helper.Scale = Core.States.InGameStateObject.GameUi.LargeMap.Zoom * 0.174f;
            Core.States.InGameStateObject.CurrentAreaInstance.Player.TryGetComponent<Positioned>(out var playerPos);
            Core.States.InGameStateObject.CurrentAreaInstance.Player.TryGetComponent<Render>(out var playerZ);
            var pZ = playerZ.TerrainHeight;
            var pPos = playerPos.GridPosition;
            var pPosV = new Vector2(pPos.X, pPos.Y);

            foreach (var entity in Core.States.InGameStateObject.CurrentAreaInstance.AwakeEntities)
            {
                if (!entity.Value.TryGetComponent<Positioned>(out var entityPos))
                {
                    continue;
                }

                if (!entity.Value.TryGetComponent<Render>(out var entityZ))
                {
                    continue;
                }

                var ePos = entityPos.GridPosition;
                float eZ = entityZ.TerrainHeight;
                var ePosV = new Vector2(ePos.X, ePos.Y);
                var fpos = Helper.DeltaInWorldToMinimapDelta(ePosV - pPosV, eZ - pZ);
                var fgDraw = ImGui.GetForegroundDrawList();
                fgDraw.AddCircleFilled(center + fpos, 2f, UiHelper.Color(255, 0, 255, 255));
            }
        }
    }
}
