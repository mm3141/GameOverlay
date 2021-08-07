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
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    /// <summary>
    /// <see cref="Radar"/> plugin.
    /// </summary>
    public sealed class Radar : PCore<RadarSettings>
    {
        /// <summary>
        /// If we don't do this, user will be asked to
        /// setup the culling window everytime they open the game.
        /// </summary>
        private bool skipOneSettingChange = false;

        private ActiveCoroutine onMove;
        private ActiveCoroutine onForegroundChange;
        private ActiveCoroutine onGameClose;
        private ActiveCoroutine onAreaChange;

        private Vector2 miniMapCenterWithDefaultShift = Vector2.Zero;
        private double miniMapDiagonalLength = 0x00;

        private double largeMapDiagonalLength = 0x00;

        // Legion Cache.
        private Dictionary<uint, byte> frozenInTimeEntities = new Dictionary<uint, byte>();

        private string heistUsefullChestContains = "HeistChestSecondary";
        private string heistAllChestStarting = "Metadata/Chests/LeagueHeist";
        private Dictionary<uint, string> heistChestCache = new Dictionary<uint, string>();

        // Delirium Hidden Monster cache.
        private Dictionary<uint, string> deliriumHiddenMonster = new Dictionary<uint, string>();
        private string deliriumHiddenMonsterStarting = "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemon";

        private string delveChestStarting = "Metadata/Chests/DelveChests/";
        private bool isAzuriteMine = false;
        private Dictionary<uint, string> delveChestCache = new Dictionary<uint, string>();

        private IntPtr walkableMapTexture = IntPtr.Zero;
        private Vector2 walkableMapDimension = Vector2.Zero;

        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");

        /// <inheritdoc/>
        public override void DrawSettings()
        {
            ImGui.TextWrapped("Following slider is known as large-map icons multiplier." +
                " When setting the icons sizes only make sure that they look nice on the mini-map." +
                " After that use the slider below to adjust large map icon" +
                " size w.r.t the mini-map-icon size.");
            ImGui.DragFloat(
                "###LargeMapScaleMultiplier",
                ref this.Settings.LargeMapScaleMultiplier,
                0.001f,
                0.01f,
                0.2f);
            ImGui.TextWrapped("If, after changing the game/monitor resolution, your " +
                "mini/large map icon gets invisible. Open this Overlay setting window, " +
                "click anywhere on it and then hide this Overlay setting window. " +
                "It will fix the issue.");
            ImGui.Separator();
            ImGui.Checkbox("(Incomplete Feature) Draw Area/Zone Map", ref this.Settings.DrawWalkableMap);
            if (this.Settings.DrawWalkableMap)
            {
                ImGui.Text("Map Position On the Screen");
                ImGui.DragFloat2("##MapPositionOnScreen", ref this.Settings.WalkableMapPosition);
            }

            ImGui.Checkbox("Modify Large Map Culling Window", ref this.Settings.ModifyCullWindow);
            ImGui.Checkbox("Hide Entities without Life/Chest component", ref this.Settings.HideUseless);
            ImGui.Checkbox("Show Player Names", ref this.Settings.ShowPlayersNames);

            this.Settings.DrawIconsSettingToImGui(
                "BaseGame Icons",
                this.Settings.BaseIcons,
                "Blockages icon can be set from Delve Icons category i.e. 'Blockage OR DelveWall'");

            this.Settings.DrawIconsSettingToImGui(
                "Legion Icons",
                this.Settings.LegionIcons,
                "Legion bosses are same as BaseGame Icons -> Unique Monsters.");

            this.Settings.DrawIconsSettingToImGui(
                "Delirium Icons",
                this.Settings.DeliriumIcons,
                string.Empty);

            this.Settings.DrawIconsSettingToImGui(
                "Heist Icons",
                this.Settings.HeistIcons,
                string.Empty);

            this.Settings.DrawIconsSettingToImGui(
                "Delve Icons",
                this.Settings.DelveIcons,
                string.Empty);
        }

        /// <inheritdoc/>
        public override void DrawUI()
        {
            if (this.Settings.ModifyCullWindow)
            {
                var lMap = Core.States.InGameStateObject.GameUi.LargeMap;
                ImGui.SetNextWindowPos(lMap.Center, ImGuiCond.Appearing);
                ImGui.SetNextWindowSize(new Vector2(400f), ImGuiCond.Appearing);
                ImGui.Begin("Large Map Culling Window");
                ImGui.TextWrapped("This is a culling window for the large map icons. " +
                    "Any large map icons outside of this window will be hidden automatically. " +
                    "Feel free to change the position/size of this window. " +
                    "Once you are happy with the dimensions, double click this window. " +
                    "You can bring this window back from the settings menu.");
                this.Settings.CullWindowPos = ImGui.GetWindowPos();
                this.Settings.CullWindowSize = ImGui.GetWindowSize();
                if (ImGui.IsWindowHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    this.Settings.ModifyCullWindow = false;
                }

                ImGui.End();
            }

            if (this.Settings.DrawWalkableMap && this.walkableMapTexture != IntPtr.Zero)
            {
                var rectf = new RectangleF(
                    this.Settings.WalkableMapPosition.X,
                    this.Settings.WalkableMapPosition.Y,
                    this.walkableMapDimension.X,
                    this.walkableMapDimension.Y);

                var p1 = Helper.DeltaInWorldToMapDelta(new Vector2(rectf.Left, rectf.Top), 0);
                var p2 = Helper.DeltaInWorldToMapDelta(new Vector2(rectf.Right, rectf.Top), 0);
                var p3 = Helper.DeltaInWorldToMapDelta(new Vector2(rectf.Right, rectf.Bottom), 0);
                var p4 = Helper.DeltaInWorldToMapDelta(new Vector2(rectf.Left, rectf.Bottom), 0);
                ImGui.GetBackgroundDrawList().AddImageQuad(
                    this.walkableMapTexture,
                    p1,
                    p2,
                    p3,
                    p4);
            }

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
                ImGui.SetNextWindowPos(this.Settings.CullWindowPos);
                ImGui.SetNextWindowSize(this.Settings.CullWindowSize);
                ImGui.SetNextWindowBgAlpha(0f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
                ImGui.Begin("Large Map Culling Window", UiHelper.TransparentWindowFlags);
                ImGui.PopStyleVar();
                this.DrawOnMap(
                    ImGui.GetWindowDrawList(),
                    largeMap.Center + largeMap.Shift + largeMap.DefaultShift,
                    this.largeMapDiagonalLength,
                    largeMap.Zoom * this.Settings.LargeMapScaleMultiplier,
                    false);
                ImGui.End();
            }

            var miniMap = Core.States.InGameStateObject.GameUi.MiniMap;
            if (miniMap.IsVisible)
            {
                ImGui.SetNextWindowPos(miniMap.Postion);
                ImGui.SetNextWindowSize(miniMap.Size);
                ImGui.SetNextWindowBgAlpha(0f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
                ImGui.Begin("###minimapRadar", UiHelper.TransparentWindowFlags);
                ImGui.PopStyleVar();
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
            this.onGameClose?.Cancel();
            this.onAreaChange?.Cancel();
            this.onMove = null;
            this.onForegroundChange = null;
            this.onGameClose = null;
            this.onAreaChange = null;
        }

        /// <inheritdoc/>
        public override void OnEnable(bool isGameOpened)
        {
            if (!isGameOpened)
            {
                this.skipOneSettingChange = true;
            }

            if (File.Exists(this.SettingPathname))
            {
                var content = File.ReadAllText(this.SettingPathname);
                this.Settings = JsonConvert.DeserializeObject<RadarSettings>(content);
            }

            this.Settings.AddDefaultIcons(this.DllDirectory);

            this.onMove = CoroutineHandler.Start(this.OnMove());
            this.onForegroundChange = CoroutineHandler.Start(this.OnForegroundChange());
            this.onGameClose = CoroutineHandler.Start(this.OnClose());
            this.onAreaChange = CoroutineHandler.Start(this.ClearCachesAndUpdateAreaInfo());
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
                var hasBuffs = entity.Value.TryGetComponent<Buffs>(out var buffsComp);
                var isChest = entity.Value.TryGetComponent<Chest>(out var chestComp);
                var hasOMP = entity.Value.TryGetComponent<ObjectMagicProperties>(out var omp);
                var isShrine = entity.Value.TryGetComponent<Shrine>(out var shrineComp);
                var isBlockage = entity.Value.TryGetComponent<TriggerableBlockage>(out var blockageComp);
                var isPlayer = entity.Value.TryGetComponent<Player>(out var playerComp);

                if (this.Settings.HideUseless && !(hasVital || isChest || isPlayer))
                {
                    continue;
                }

                if (this.Settings.HideUseless && isChest && chestComp.IsOpened)
                {
                    continue;
                }

                if (this.Settings.HideUseless && hasVital)
                {
                    if (!lifeComp.IsAlive)
                    {
                        continue;
                    }

                    if (!hasOMP && !isBlockage && !isPlayer)
                    {
                        continue;
                    }
                }

                if (this.Settings.HideUseless && isBlockage && !blockageComp.IsBlocked)
                {
                    continue;
                }

                if (this.Settings.HideUseless && isPlayer && entity.Value.Address == Core.States.InGameStateObject.CurrentAreaInstance.Player.Address)
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
                if (isPlayer)
                {
                    if (this.Settings.ShowPlayersNames)
                    {
                        var pNameSizeH = ImGui.CalcTextSize(playerComp.Name) / 2;
                        fgDraw.AddRectFilled(mapCenter + fpos - pNameSizeH, mapCenter + fpos + pNameSizeH, UiHelper.Color(0, 0, 0, 200));
                        fgDraw.AddText(ImGui.GetFont(), ImGui.GetFontSize(), mapCenter + fpos - pNameSizeH, UiHelper.Color(255, 128, 128, 255), playerComp.Name);
                    }
                    else
                    {
                        var playerIcon = this.Settings.BaseIcons["Player"];
                        finalSize *= playerIcon.IconScale;
                        fgDraw.AddImage(
                            playerIcon.TexturePtr,
                            mapCenter + fpos - finalSize,
                            mapCenter + fpos + finalSize,
                            playerIcon.UV0,
                            playerIcon.UV1);
                    }
                }
                else if (isBlockage)
                {
                    var blockageIcon = this.Settings.DelveIcons["Blockage OR DelveWall"];
                    finalSize *= blockageIcon.IconScale;
                    fgDraw.AddImage(
                        blockageIcon.TexturePtr,
                        mapCenter + fpos - finalSize,
                        mapCenter + fpos + finalSize,
                        blockageIcon.UV0,
                        blockageIcon.UV1);
                }
                else if (isChest)
                {
                    // TODO: Name Filter IconInfo/Color/null LargeMapSize MinimapSize
                    // TODO: Strongbox Draw
                    // TODO: Breach big chests
                    if (this.isAzuriteMine)
                    {
                        if (this.delveChestCache.TryGetValue(entity.Key.id, out var iconFinder))
                        {
                            if (this.Settings.DelveIcons.TryGetValue(iconFinder, out var delveChestIcon))
                            {
                                // Have to force keep the Delve Chest since GGG changed
                                // network bubble radius for them.
                                entity.Value.ForceKeepEntity();
                                finalSize *= delveChestIcon.IconScale;
                                fgDraw.AddImage(
                                    delveChestIcon.TexturePtr,
                                    mapCenter + fpos - finalSize,
                                    mapCenter + fpos + finalSize,
                                    delveChestIcon.UV0,
                                    delveChestIcon.UV1);
                            }
                        }
                        else
                        {
                            this.delveChestCache[entity.Key.id] =
                                this.DelveChestPathToIcon(entity.Value.Path);
                        }

                        continue;
                    }

                    if (entity.Value.TryGetComponent<MinimapIcon>(out var _))
                    {
                        if (this.heistChestCache.TryGetValue(entity.Key.id, out var iconFinder))
                        {
                            if (this.Settings.HeistIcons.TryGetValue(iconFinder, out var heistChestIcon))
                            {
                                finalSize *= heistChestIcon.IconScale;
                                fgDraw.AddImage(
                                    heistChestIcon.TexturePtr,
                                    mapCenter + fpos - finalSize,
                                    mapCenter + fpos + finalSize,
                                    heistChestIcon.UV0,
                                    heistChestIcon.UV1);
                            }

                            continue;
                        }
                        else if (entity.Value.Path.StartsWith(
                            this.heistAllChestStarting,
                            StringComparison.Ordinal))
                        {
                            this.heistChestCache[entity.Key.id] =
                                this.HeistChestPathToIcon(entity.Value.Path);
                            continue;
                        }
                    }

                    var chestIcon = this.Settings.BaseIcons["Chest"];
                    finalSize *= chestIcon.IconScale;
                    fgDraw.AddImage(
                        chestIcon.TexturePtr,
                        mapCenter + fpos - finalSize,
                        mapCenter + fpos + finalSize,
                        chestIcon.UV0,
                        chestIcon.UV1);
                }
                else if (isShrine)
                {
                    if (!shrineComp.IsUsed)
                    {
                        var shrineIcon = this.Settings.BaseIcons["Shrine"];
                        finalSize *= shrineIcon.IconScale;
                        fgDraw.AddImage(
                            shrineIcon.TexturePtr,
                            mapCenter + fpos - finalSize,
                            mapCenter + fpos + finalSize,
                            shrineIcon.UV0,
                            shrineIcon.UV1);
                    }

                    continue;
                }
                else if (hasVital)
                {
                    // TODO: Invisible/Hidden/Non-Targetable/Frozen/Exploding/in-the-cloud/not-giving-exp things
                    if (hasBuffs && buffsComp.StatusEffects.ContainsKey("frozen_in_time"))
                    {
                        this.frozenInTimeEntities.TryAdd(entity.Key.id, 1);
                        if (buffsComp.StatusEffects.ContainsKey("legion_reward_display") ||
                            entity.Value.Path.Contains("Chest"))
                        {
                            var monsterChestIcon = this.Settings.LegionIcons["Legion Monster Chest"];
                            finalSize *= monsterChestIcon.IconScale;
                            fgDraw.AddImage(
                                monsterChestIcon.TexturePtr,
                                mapCenter + fpos - finalSize,
                                mapCenter + fpos + finalSize,
                                monsterChestIcon.UV0,
                                monsterChestIcon.UV1);
                            continue;
                        }
                    }
                    else if (hasBuffs && buffsComp.StatusEffects.ContainsKey("hidden_monster"))
                    {
                        if (this.frozenInTimeEntities.ContainsKey(entity.Key.id))
                        {
                            continue;
                        }

                        if (this.deliriumHiddenMonster.TryGetValue(entity.Key.id, out var iconFinder))
                        {
                            if (this.Settings.DeliriumIcons.TryGetValue(iconFinder, out var dHiddenMIcon))
                            {
                                finalSize *= dHiddenMIcon.IconScale;
                                fgDraw.AddImage(
                                    dHiddenMIcon.TexturePtr,
                                    mapCenter + fpos - finalSize,
                                    mapCenter + fpos + finalSize,
                                    dHiddenMIcon.UV0,
                                    dHiddenMIcon.UV1);
                            }

                            continue;
                        }

                        if (entity.Value.Path.StartsWith(
                            this.deliriumHiddenMonsterStarting,
                            StringComparison.Ordinal))
                        {
                            this.deliriumHiddenMonster[entity.Key.id] =
                                this.DeliriumHiddenMonsterPathToIcon(entity.Value.Path);
                            continue;
                        }
                    }

                    var monsterIcon = entityPos.IsFriendly ?
                        this.Settings.BaseIcons["Friendly"] :
                        this.RarityToIconMapping(omp.Rarity);
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
                    fgDraw.AddCircleFilled(mapCenter + fpos, 5f, UiHelper.Color(255, 0, 255, 255));
                }
            }
        }

        private IEnumerator<Wait> ClearCachesAndUpdateAreaInfo()
        {
            while (true)
            {
                yield return new Wait(RemoteEvents.AreaChanged);
                this.frozenInTimeEntities.Clear();
                this.heistChestCache.Clear();
                this.deliriumHiddenMonster.Clear();
                this.delveChestCache.Clear();
                this.isAzuriteMine = Core.States.AreaLoading.CurrentAreaName == "Azurite Mine";
                if (this.Settings.DrawWalkableMap)
                {
                    this.GenerateMapTexture();
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
                if (this.skipOneSettingChange)
                {
                    this.skipOneSettingChange = false;
                }
                else
                {
                    this.Settings.ModifyCullWindow = true;
                }
            }
        }

        private IEnumerator<Wait> OnClose()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnClose);
                this.skipOneSettingChange = true;
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

        private void GenerateMapTexture()
        {
            Core.Overlay.RemoveImage("walkable_map");
            var instance = Core.States.InGameStateObject.CurrentAreaInstance;
            var mapTextureData = instance.WalkableData;
            var bytesPerRow = instance.TerrainMetadata.BytesPerRow;
            var totalRows = mapTextureData.Length / bytesPerRow;
            using Image<Rgba32> image = new Image<Rgba32>(bytesPerRow * 2, totalRows);
            image.Mutate(c => c.ProcessPixelRowsAsVector4((row, i) =>
            {
                for (int x = 0; x < row.Length - 1; x += 2)
                {
                    byte data = mapTextureData[(i.Y * bytesPerRow) + (x / 2)];

                    // each byte contains 2 data points of size 4 bit.
                    // I know this because if we don't do this, the whole map texture (final output)
                    // has to be multiplied (scaled) by 2 since (at that point) we are eating 1 data point.
                    // In the following loop, in iteration 0, we draw 1st data point and, in iteration 1,
                    // we draw the 2nd data point.
                    for (int k = 0; k < 2; k++)
                    {
                        switch ((data >> (0x04 * k)) & 0x0F)
                        {
                            case 1: // walkable
                                row[x + k] = Vector4.One;
                                break;
                            case 5: // walkable
                            case 4: // walkable
                            case 3: // walkable
                            case 2: // walkable
                            case 0: // non-walable
                                row[x + k] = Vector4.Zero;
                                break;
                            default:
                                throw new Exception($"New digit found {(data >> (0x04 * k)) & 0x0F}");
                        }
                    }
                }
            }));

            Core.Overlay.AddOrGetImagePointer("walkable_map", image, false, false, out var t, out var w, out var h);
            this.walkableMapTexture = t;
            this.walkableMapDimension = new Vector2(w, h);
        }

        private IconPicker RarityToIconMapping(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Normal:
                case Rarity.Magic:
                case Rarity.Rare:
                case Rarity.Unique:
                    return this.Settings.BaseIcons[$"{rarity} Monster"];
                default:
                    return this.Settings.BaseIcons[$"Normal Monster"];
            }
        }

        private string HeistChestPathToIcon(string path)
        {
            var strToReplace = string.Join('/', this.heistAllChestStarting, this.heistUsefullChestContains);
            var truncatedPath = path
                .Replace(strToReplace, null, StringComparison.Ordinal)
                .Replace("Military", null, StringComparison.Ordinal)
                .Replace("Thug", null, StringComparison.Ordinal)
                .Replace("Science", null, StringComparison.Ordinal)
                .Replace("Robot", null, StringComparison.Ordinal);
            return $"Heist {truncatedPath}";
        }

        private string DeliriumHiddenMonsterPathToIcon(string path)
        {
            if (path.Contains("BloodBag"))
            {
                return "Delirium Bomb";
            }
            else if (path.Contains("EggFodder"))
            {
                return "Delirium Spawner";
            }
            else if (path.Contains("GlobSpawn"))
            {
                return "Delirium Spawner";
            }
            else
            {
                return $"Delirium Ignore";
            }
        }

        private string DelveChestPathToIcon(string path)
        {
            string truncatedPath = path.Replace(
                this.delveChestStarting,
                null,
                StringComparison.Ordinal);

            if (truncatedPath.Length != path.Length)
            {
                return truncatedPath;
            }

            return "Delve Ignore";
        }
    }
}
