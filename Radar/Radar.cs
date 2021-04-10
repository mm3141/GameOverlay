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
        private Dictionary<ushort, byte> frozenInTimeEntities = new Dictionary<ushort, byte>();

        private string heistUsefullChestContains = "HeistChestSecondary";
        private string heistAllChestStarting = "Metadata/Chests/LeagueHeist";
        private Dictionary<ushort, string> heistChestCache = new Dictionary<ushort, string>();

        // Delirium Hidden Monster cache.
        private Dictionary<ushort, string> deliriumHiddenMonster = new Dictionary<ushort, string>();
        private string deliriumHiddenMonsterStarting = "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemon";

        private string delveChestStarting = "Metadata/Chests/DelveChests/";
        private bool isAzuriteMine = false;
        private Dictionary<ushort, string> delveChestCache = new Dictionary<ushort, string>();

        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");

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
            ImGui.Checkbox("Modify Large Map Culling Window", ref this.Settings.ModifyCullWindow);
            ImGui.Checkbox("Hide Entities without Life/Chest component", ref this.Settings.HideUseless);

            this.Settings.DrawIconsSettingToImGui(
                "BaseGame Icons",
                this.Settings.BaseIcons,
                string.Empty);

            this.Settings.DrawIconsSettingToImGui(
                "Legion Icons",
                this.Settings.LegionIcons,
                "Legion bosses are same as Base Game unique monsters.");

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
                "Delve wall is in the Base Game Icons.");
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
                var isChest = entity.Value.TryGetComponent<Chest>(out var chestComp);
                var hasOMP = entity.Value.TryGetComponent<ObjectMagicProperties>(out var omp);
                var isShrine = entity.Value.TryGetComponent<Shrine>(out var shrineComp);
                var isBlockage = entity.Value.TryGetComponent<TriggerableBlockage>(out var blockageComp);

                if (this.Settings.HideUseless && !(hasVital || isChest))
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

                    if (!hasOMP && !isBlockage)
                    {
                        continue;
                    }

                    if (isBlockage && !blockageComp.IsBlocked)
                    {
                        continue;
                    }
                }

                if (this.Settings.HideUseless && isBlockage && !blockageComp.IsBlocked)
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
                if (isBlockage)
                {
                    var blockageIcon = this.Settings.BaseIcons["Blockage OR DelveWall"];
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
                    if (lifeComp.StatusEffects.ContainsKey("frozen_in_time"))
                    {
                        this.frozenInTimeEntities.TryAdd(entity.Key.id, 1);
                        if (lifeComp.StatusEffects.ContainsKey("legion_reward_display") ||
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
                    else if (lifeComp.StatusEffects.ContainsKey("hidden_monster"))
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

        private IconPicker RarityToIconMapping(Rarity rarity)
        {
            return this.Settings.BaseIcons[$"{rarity} Monster"];
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
                return "Chest";
            }

            return "Delve Ignore";
        }
    }
}
