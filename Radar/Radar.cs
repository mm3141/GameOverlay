// <copyright file="Radar.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Radar
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using Coroutine;
    using GameHelper;
    using GameHelper.CoroutineEvents;
    using GameHelper.Plugin;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.RemoteObjects.States.InGameStateObjects;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;

    /// <summary>
    /// <see cref="Radar"/> plugin.
    /// </summary>
    public sealed class Radar : PCore<RadarSettings>
    {
        private ActiveCoroutine onMove;
        private ActiveCoroutine onForegroundChange;

        private Vector2 miniMapCenterWithDefaultShift = Vector2.Zero;
        private double miniMapDiagonalLength = 0x00;

        private double largeMapDiagonalLength = 0x00;

        private string SettingPathname
            => Path.Join(this.DllDirectory, "config", "settings.txt");

        /// <inheritdoc/>
        public override void DrawSettings()
        {
            ImGui.TextWrapped("Please provide the Large Map Scale multiplier " +
                "value. Also, before providing this value make sure the Core -> " +
                "window scale value is what your window setting is showing.");
            ImGui.DragFloat(
                "###LargeMapScaleMultiplier",
                ref this.Settings.LargeMapScaleMultiplier,
                0.001f,
                0.01f,
                0.2f);
            ImGui.TextWrapped("If, after changing the game/monitor resolution, your " +
                "mini/large map icon gets invisible. Open this setting window, " +
                "click anywhere on it and then close this setting window. " +
                "It will fix the issue.");
            ImGui.Separator();
            ImGui.Checkbox("Hide Entities without Life/Chest component", ref this.Settings.HideUseless);

            ImGui.Columns(2, "icons columns", false);
            foreach (var icon in this.Settings.Icons)
            {
                ImGui.Text(icon.Key);
                ImGui.NextColumn();
                icon.Value.ShowSettingWidget();
                ImGui.NextColumn();
            }

            ImGui.Columns(1);
        }

        /// <inheritdoc/>
        public override void DrawUI()
        {
            if (Core.States.GameCurrentState != GameStateTypes.InGameState)
            {
                return;
            }

            if (!Core.Process.Foreground)
            {
                return;
            }

            var largeMap = Core.States.InGameStateObject.GameUi.LargeMap;
            if (largeMap.IsVisible)
            {
                this.DrawOnMap(
                    ImGui.GetForegroundDrawList(),
                    largeMap.Center + largeMap.Shift + largeMap.DefaultShift,
                    this.largeMapDiagonalLength,
                    largeMap.Zoom * this.Settings.LargeMapScaleMultiplier,
                    false);
            }

            var miniMap = Core.States.InGameStateObject.GameUi.MiniMap;
            if (miniMap.IsVisible)
            {
                ImGui.SetNextWindowPos(miniMap.Postion);
                ImGui.SetNextWindowSize(miniMap.Size);
                ImGui.SetNextWindowBgAlpha(0f);
                ImGui.Begin("###minimapRadar", UiHelper.TransparentWindowFlags);
                this.DrawOnMap(
                    ImGui.GetWindowDrawList(),
                    this.miniMapCenterWithDefaultShift + miniMap.Shift,
                    this.miniMapDiagonalLength,
                    miniMap.Zoom,
                    true);
                ImGui.End();
            }
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            this.onMove?.Cancel();
            this.onForegroundChange?.Cancel();
            this.onMove = null;
            this.onForegroundChange = null;
        }

        /// <inheritdoc/>
        public override void OnEnable()
        {
            if (File.Exists(this.SettingPathname))
            {
                var content = File.ReadAllText(this.SettingPathname);
                this.Settings = JsonConvert.DeserializeObject<RadarSettings>(content);
            }
            else
            {
                var iconPathName = Path.Join(this.DllDirectory, "icons.png");
                this.Settings.Icons.Add("Chest Icon", new IconPicker(iconPathName, 14, 41));
                this.Settings.Icons.Add("Monster Icon", new IconPicker(iconPathName, 14, 41));
                this.Settings.Icons.Add("Friendly Monster Icon", new IconPicker(iconPathName, 14, 41));
            }

            this.onMove = CoroutineHandler.Start(this.OnMove());
            this.onForegroundChange = CoroutineHandler.Start(this.OnForegroundChange());
        }

        /// <inheritdoc/>
        public override void SaveSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.SettingPathname));
            var settingsData = JsonConvert.SerializeObject(this.Settings, Formatting.Indented);
            File.WriteAllText(this.SettingPathname, settingsData);
        }

        private void DrawOnMap(ImDrawListPtr fgDraw, Vector2 mapCenter, double diagonalLength, float scale, bool isMiniMap)
        {
            Helper.DiagonalLength = diagonalLength;
            Helper.Scale = scale;
            Core.States.InGameStateObject.CurrentAreaInstance.
                Player.TryGetComponent<Positioned>(out var playerPos);
            Core.States.InGameStateObject.CurrentAreaInstance.
                Player.TryGetComponent<Render>(out var playerRender);
            if (playerPos == null || playerRender == null)
            {
                return;
            }

            var pPos = new Vector2(playerPos.GridPosition.X, playerPos.GridPosition.Y);
            foreach (var entity in Core.States.InGameStateObject.CurrentAreaInstance.AwakeEntities)
            {
                var hasVital = entity.Value.TryGetComponent<Life>(out var lifeComp);
                var isChest = entity.Value.TryGetComponent<Chest>(out var chestComp);
                var hasOMP = entity.Value.TryGetComponent<ObjectMagicProperties>(out var omp);

                if (this.Settings.HideUseless && !(hasVital || isChest))
                {
                    continue;
                }

                if (this.Settings.HideUseless && isChest && chestComp.IsOpened)
                {
                    continue;
                }

                if (this.Settings.HideUseless && hasVital && lifeComp.Health.Current <= 0)
                {
                    continue;
                }

                if (this.Settings.HideUseless && hasVital && !hasOMP)
                {
                    continue;
                }

                if (!entity.Value.TryGetComponent<Positioned>(out var entityPos))
                {
                    continue;
                }

                if (!entity.Value.TryGetComponent<Render>(out var entityZ))
                {
                    continue;
                }

                var ePos = new Vector2(entityPos.GridPosition.X, entityPos.GridPosition.Y);
                var fpos = Helper.DeltaInWorldToMapDelta(
                    ePos - pPos, entityZ.TerrainHeight - playerRender.TerrainHeight);
                var finalSize = Vector2.One * scale * (isMiniMap ? 1f : 5f);
                if (isChest)
                {
                    // TODO: Name Filter IconInfo/Color/null LargeMapSize MinimapSize
                    // TODO: Strongbox Draw
                    // TODO: Delve Chests
                    // TODO: Heist Chest
                    // TODO: Legion Chest
                    // TODO: Breach big chests
                    var chestIcon = this.Settings.Icons["Chest Icon"];
                    finalSize *= chestIcon.IconScale;
                    fgDraw.AddImage(
                        chestIcon.TexturePtr,
                        mapCenter + fpos - finalSize,
                        mapCenter + fpos + finalSize,
                        chestIcon.UV0,
                        chestIcon.UV1);
                }
                else if (hasVital)
                {
                    // TODO: Delierium pauses show?
                    // TODO: Fix bug, legion monster showing as friendly.
                    // TODO: Monsters with 0 rarity.
                    // TODO: Normal, Magic, Rare, Unique Monster
                    // TODO: Invisible/Hidden/Non-Targetable/Frozen/Exploding/in-the-cloud/not-giving-exp things
                    if (entityPos.IsFriendly)
                    {
                        var monsterIcon = this.Settings.Icons["Friendly Monster Icon"];
                        finalSize *= monsterIcon.IconScale;
                        fgDraw.AddImage(
                            monsterIcon.TexturePtr,
                            mapCenter + fpos - finalSize,
                            mapCenter + fpos + finalSize,
                            monsterIcon.UV0,
                            monsterIcon.UV1);
                    }
                    else
                    {
                        var monsterIcon = this.Settings.Icons["Monster Icon"];
                        finalSize *= monsterIcon.IconScale;
                        fgDraw.AddImage(
                            monsterIcon.TexturePtr,
                            mapCenter + fpos - finalSize,
                            mapCenter + fpos + finalSize,
                            monsterIcon.UV0,
                            monsterIcon.UV1);
                    }
                }
                else
                {
                    fgDraw.AddCircleFilled(mapCenter + fpos, 5f, UiHelper.Color(255, 0, 255, 255));
                }
            }
        }

        private IEnumerator<Wait> OnMove()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnMoved);
                this.UpdateMiniMapDetails();
                this.UpdateLargeMapDetails();
            }
        }

        private IEnumerator<Wait> OnForegroundChange()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnForegroundChanged);
                this.UpdateMiniMapDetails();
                this.UpdateLargeMapDetails();
            }
        }

        private void UpdateMiniMapDetails()
        {
            var map = Core.States.InGameStateObject.GameUi.MiniMap;
            this.miniMapCenterWithDefaultShift = map.Postion + (map.Size / 2) + map.DefaultShift;

            var widthSq = map.Size.X * map.Size.X;
            var heightSq = map.Size.Y * map.Size.Y;
            this.miniMapDiagonalLength = Math.Sqrt(widthSq + heightSq);
        }

        private void UpdateLargeMapDetails()
        {
            var map = Core.States.InGameStateObject.GameUi.LargeMap;
            var widthSq = map.Size.X * map.Size.X;
            var heightSq = map.Size.Y * map.Size.Y;
            this.largeMapDiagonalLength = Math.Sqrt(widthSq + heightSq);
        }
    }
}
