// <copyright file="Radar.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Radar
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;
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

    /// <summary>
    /// <see cref="Radar"/> plugin.
    /// </summary>
    public sealed class Radar : PCore<RadarSettings>
    {
        private readonly string heistUsefullChestContains = "HeistChestSecondary";
        private readonly string heistAllChestStarting = "Metadata/Chests/LeagueHeist";
        private readonly Dictionary<uint, string> heistChestCache = new();
        private readonly string delveChestStarting = "Metadata/Chests/DelveChests/";
        private readonly Dictionary<uint, string> delveChestCache = new();

        /// <summary>
        /// If we don't do this, user will be asked to
        /// setup the culling window everytime they open the game.
        /// </summary>
        private bool skipOneSettingChange = false;
        private bool isAddNewPOIHeaderOpened = false;
        private ActiveCoroutine onMove;
        private ActiveCoroutine onForegroundChange;
        private ActiveCoroutine onGameClose;
        private ActiveCoroutine onAreaChange;

        private string currentAreaName = string.Empty;
        private string tmpTileName = string.Empty;
        private string tmpDisplayName = string.Empty;
        private int tmpTgtSelectionCounter = 0;
        private string leaderName = string.Empty;

        private double miniMapDiagonalLength = 0x00;

        private double largeMapDiagonalLength = 0x00;

        private IntPtr walkableMapTexture = IntPtr.Zero;
        private Vector2 walkableMapDimension = Vector2.Zero;

        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");

        private string ImportantTgtPathName => Path.Join(this.DllDirectory, "important_tgt_files.txt");

        /// <inheritdoc/>
        public override void DrawSettings()
        {
            ImGui.TextWrapped("If your mini/large map icon are not working/visible. Open this " +
                "setting window, click anywhere on it and then hide this setting window. It will fix the issue.");
            ImGui.DragFloat("Large Map Fix", ref this.Settings.LargeMapScaleMultiplier, 0.001f, 0.01f, 0.3f);
            ImGuiHelper.ToolTip("This slider is for fixing large map (icons) offset. " +
                "You have to use it if you feel that LargeMap Icons " +
                "are moving while your player is moving. You only have " +
                "to find a value that works for you per game window resolution. " +
                "Basically, you don't have to change it unless you change your " +
                "game window resolution. Also, please contribute back, let me know " +
                "what resolution you use and what value works best for you. " +
                "This slider has no impact on mini-map icons. For windowed-full-screen " +
                "default value should be good enough.");
            ImGui.Checkbox("Hide Radar when in Hideout/Town", ref this.Settings.DrawWhenNotInHideoutOrTown);
            ImGui.Checkbox("Hide Radar when game is in the background", ref this.Settings.DrawWhenForeground);
            if (ImGui.Checkbox("Modify Large Map Culling Window", ref this.Settings.ModifyCullWindow))
            {
                if (this.Settings.ModifyCullWindow)
                {
                    this.Settings.MakeCullWindowFullScreen = false;
                }
            }

            ImGui.TreePush();
            if (ImGui.Checkbox("Make Culling Window Cover Whole Game", ref this.Settings.MakeCullWindowFullScreen))
            {
                this.Settings.ModifyCullWindow = !this.Settings.MakeCullWindowFullScreen;
                this.Settings.CullWindowPos = Vector2.Zero;
                this.Settings.CullWindowSize.X = Core.Process.WindowArea.Width;
                this.Settings.CullWindowSize.Y = Core.Process.WindowArea.Height;
            }

            if (ImGui.TreeNode("Culling window advance options"))
            {
                ImGui.Checkbox("Draw maphack in culling window", ref this.Settings.DrawMapInCull);
                ImGui.Checkbox("Draw POIs in culling window", ref this.Settings.DrawPOIInCull);
                ImGui.TreePop();
            }

            ImGui.TreePop();
            ImGui.Separator();
            ImGui.NewLine();
            if (ImGui.Checkbox("Draw Area/Zone Map (maphack)", ref this.Settings.DrawWalkableMap))
            {
                if (this.Settings.DrawWalkableMap)
                {
                    if (this.walkableMapTexture == IntPtr.Zero)
                    {
                        this.ReloadMapTexture();
                    }
                }
                else
                {
                    this.RemoveMapTexture();
                }
            }

            if (ImGui.ColorEdit4("Drawn Map Color", ref this.Settings.WalkableMapColor))
            {
                if (this.walkableMapTexture != IntPtr.Zero)
                {
                    this.ReloadMapTexture();
                }
            }

            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Checkbox("Show points of interest (POI)", ref this.Settings.ShowImportantPOI);
            ImGui.ColorEdit4("POI text color", ref this.Settings.POIColor);
            ImGui.Checkbox("Add black background to POI text", ref this.Settings.EnablePOIBackground);
            this.isAddNewPOIHeaderOpened = ImGui.CollapsingHeader("Add or Modify POI");
            if (this.isAddNewPOIHeaderOpened)
            {
                this.AddNewPOIWidget();
                this.ShowPOIWidget();
            }

            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Checkbox("Hide Entities outside the network bubble", ref this.Settings.HideOutsideNetworkBubble);
            ImGui.Checkbox("Show Player Names", ref this.Settings.ShowPlayersNames);
            ImGui.InputText("Party Leader Name", ref this.leaderName, 200);
            ImGuiHelper.ToolTip("This button will not work while Player is in the Scourge.");
            if (ImGui.CollapsingHeader("Icons Setting"))
            {
                this.Settings.DrawIconsSettingToImGui(
                    "BaseGame Icons",
                    this.Settings.BaseIcons,
                    "Blockages icon can be set from Delve Icons category i.e. 'Blockage OR DelveWall'");

                this.Settings.DrawIconsSettingToImGui(
                    "Breach Icons",
                    this.Settings.BreachIcons,
                    "Breach bosses are same as BaseGame Icons -> Unique Monsters.");

                this.Settings.DrawIconsSettingToImGui(
                    "Legion Icons",
                    this.Settings.LegionIcons,
                    "Selecting first icon from the icons image window will display important " +
                    "legion monster names rather than the icon.");

                this.Settings.DrawIconsSettingToImGui(
                    "Delirium Icons",
                    this.Settings.DeliriumIcons,
                    string.Empty);

                this.Settings.DrawIconsSettingToImGui(
                    "Heist Icons",
                    this.Settings.HeistIcons,
                    string.Empty);

                this.Settings.DrawIconsSettingToImGui(
                    "Expedition Icons",
                    this.Settings.ExpeditionIcons,
                    string.Empty);

                this.Settings.DrawIconsSettingToImGui(
                    "Delve Icons",
                    this.Settings.DelveIcons,
                    "Selecting first icon from the icons image window will display " +
                    "chest name rather than the icon.");
            }
        }

        /// <inheritdoc/>
        public override void DrawUI()
        {
            var largeMap = Core.States.InGameStateObject.GameUi.LargeMap;
            var miniMap = Core.States.InGameStateObject.GameUi.MiniMap;
            var areaDetails = Core.States.InGameStateObject.CurrentWorldInstance.AreaDetails;
            if (this.Settings.ModifyCullWindow)
            {
                ImGui.SetNextWindowPos(largeMap.Center, ImGuiCond.Appearing);
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

            if (Core.States.GameCurrentState != GameStateTypes.InGameState)
            {
                return;
            }

            if (this.Settings.DrawWhenForeground && !Core.Process.Foreground)
            {
                return;
            }

            if (this.Settings.DrawWhenNotInHideoutOrTown &&
                (areaDetails.IsHideout || areaDetails.IsTown))
            {
                return;
            }

            if (largeMap.IsVisible)
            {
                var largeMapRealCenter = largeMap.Center + largeMap.Shift + largeMap.DefaultShift;
                var largeMapModifiedZoom = this.Settings.LargeMapScaleMultiplier * largeMap.Zoom;
                Helper.DiagonalLength = this.largeMapDiagonalLength;
                Helper.Scale = largeMapModifiedZoom;
                ImGui.SetNextWindowPos(this.Settings.CullWindowPos);
                ImGui.SetNextWindowSize(this.Settings.CullWindowSize);
                ImGui.SetNextWindowBgAlpha(0f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
                ImGui.Begin("Large Map Culling Window", ImGuiHelper.TransparentWindowFlags);
                ImGui.PopStyleVar();
                this.DrawLargeMap(largeMapRealCenter);
                this.DrawTgtFiles(largeMapRealCenter);
                this.DrawMapIcons(largeMapRealCenter, largeMapModifiedZoom * 5f);
                ImGui.End();
            }

            if (miniMap.IsVisible)
            {
                Helper.DiagonalLength = this.miniMapDiagonalLength;
                Helper.Scale = miniMap.Zoom;
                var miniMapCenter = miniMap.Postion +
                    (miniMap.Size / 2) +
                    miniMap.DefaultShift +
                    miniMap.Shift;
                ImGui.SetNextWindowPos(miniMap.Postion);
                ImGui.SetNextWindowSize(miniMap.Size);
                ImGui.SetNextWindowBgAlpha(0f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
                ImGui.Begin("###minimapRadar", ImGuiHelper.TransparentWindowFlags);
                ImGui.PopStyleVar();
                this.DrawMapIcons(miniMapCenter, miniMap.Zoom);
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
            this.CleanUpRadarPluginCaches();
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

            if (File.Exists(this.ImportantTgtPathName))
            {
                var tgtfiles = File.ReadAllText(this.ImportantTgtPathName);
                this.Settings.ImportantTgts = JsonConvert.DeserializeObject
                    <Dictionary<string, Dictionary<string, string>>>(tgtfiles);
            }

            this.Settings.AddDefaultIcons(this.DllDirectory);

            this.onMove = CoroutineHandler.Start(this.OnMove());
            this.onForegroundChange = CoroutineHandler.Start(this.OnForegroundChange());
            this.onGameClose = CoroutineHandler.Start(this.OnClose());
            this.onAreaChange = CoroutineHandler.Start(this.ClearCachesAndUpdateAreaInfo());
            this.GenerateMapTexture();
        }

        /// <inheritdoc/>
        public override void SaveSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.SettingPathname));
            var settingsData = JsonConvert.SerializeObject(this.Settings, Formatting.Indented);
            File.WriteAllText(this.SettingPathname, settingsData);

            if (this.Settings.ImportantTgts.Count > 0)
            {
                var tgtfiles = JsonConvert.SerializeObject(
                    this.Settings.ImportantTgts, Formatting.Indented);
                File.WriteAllText(this.ImportantTgtPathName, tgtfiles);
            }
        }

        private void DrawLargeMap(Vector2 mapCenter)
        {
            if (!this.Settings.DrawWalkableMap)
            {
                return;
            }

            if (this.walkableMapTexture == IntPtr.Zero)
            {
                return;
            }

            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (!player.TryGetComponent<Render>(out var pRender))
            {
                return;
            }

            var rectf = new RectangleF(
                -pRender.GridPosition.X,
                -pRender.GridPosition.Y,
                this.walkableMapDimension.X,
                this.walkableMapDimension.Y);

            var p1 = Helper.DeltaInWorldToMapDelta(
                new Vector2(rectf.Left, rectf.Top), -pRender.TerrainHeight);
            var p2 = Helper.DeltaInWorldToMapDelta(
                new Vector2(rectf.Right, rectf.Top), -pRender.TerrainHeight);
            var p3 = Helper.DeltaInWorldToMapDelta(
                new Vector2(rectf.Right, rectf.Bottom), -pRender.TerrainHeight);
            var p4 = Helper.DeltaInWorldToMapDelta(
                new Vector2(rectf.Left, rectf.Bottom), -pRender.TerrainHeight);
            p1 += mapCenter;
            p2 += mapCenter;
            p3 += mapCenter;
            p4 += mapCenter;

            if (this.Settings.DrawMapInCull)
            {
                ImGui.GetWindowDrawList().AddImageQuad(this.walkableMapTexture, p1, p2, p3, p4);
            }
            else
            {
                ImGui.GetBackgroundDrawList().AddImageQuad(this.walkableMapTexture, p1, p2, p3, p4);
            }
        }

        private void DrawTgtFiles(Vector2 mapCenter)
        {
            var col = ImGuiHelper.Color(
                (uint)(this.Settings.POIColor.X * 255),
                (uint)(this.Settings.POIColor.Y * 255),
                (uint)(this.Settings.POIColor.Z * 255),
                (uint)(this.Settings.POIColor.W * 255));

            ImDrawListPtr fgDraw;
            if (this.Settings.DrawPOIInCull)
            {
                fgDraw = ImGui.GetWindowDrawList();
            }
            else
            {
                fgDraw = ImGui.GetBackgroundDrawList();
            }

            var currentAreaInstance = Core.States.InGameStateObject.CurrentAreaInstance;
            if (!currentAreaInstance.Player.TryGetComponent<Render>(out var playerRender))
            {
                return;
            }

            var pPos = new Vector2(playerRender.GridPosition.X, playerRender.GridPosition.Y);

            void drawString(string text, Vector2 location, Vector2 stringImGuiSize, bool drawBackground)
            {
                float height = 0;
                if (location.X < currentAreaInstance.GridHeightData[0].Length &&
                    location.Y < currentAreaInstance.GridHeightData.Length)
                {
                    height = currentAreaInstance.GridHeightData[(int)location.Y][(int)location.X];
                }

                var fpos = Helper.DeltaInWorldToMapDelta(
                    location - pPos, -playerRender.TerrainHeight + height);
                if (drawBackground)
                {
                    fgDraw.AddRectFilled(
                        mapCenter + fpos - stringImGuiSize,
                        mapCenter + fpos + stringImGuiSize,
                        ImGuiHelper.Color(0, 0, 0, 200));
                }

                fgDraw.AddText(
                    ImGui.GetFont(),
                    ImGui.GetFontSize(),
                    mapCenter + fpos - stringImGuiSize,
                    col,
                    text);
            }

            if (this.isAddNewPOIHeaderOpened)
            {
                var counter = 0;
                foreach (var tgtKV in currentAreaInstance.TgtTilesLocations)
                {
                    if (!(this.Settings.POIFrequencyFilter > 0 &&
                        tgtKV.Value.Count > this.Settings.POIFrequencyFilter))
                    {
                        var tgtKImGuiSize = ImGui.CalcTextSize(counter.ToString()) / 2;
                        for (var i = 0; i < tgtKV.Value.Count; i++)
                        {
                            drawString(counter.ToString(), tgtKV.Value[i], tgtKImGuiSize, false);
                        }
                    }

                    counter++;
                }
            }
            else if (this.Settings.ShowImportantPOI &&
                this.Settings.ImportantTgts.ContainsKey(this.currentAreaName))
            {
                foreach (var tile in this.Settings.ImportantTgts[this.currentAreaName])
                {
                    if (currentAreaInstance.TgtTilesLocations.ContainsKey(tile.Key))
                    {
                        var locations = currentAreaInstance.TgtTilesLocations[tile.Key];
                        var strSize = ImGui.CalcTextSize(tile.Value) / 2;
                        for (var i = 0; i < locations.Count; i++)
                        {
                            drawString(tile.Value, locations[i], strSize, this.Settings.EnablePOIBackground);
                        }
                    }
                }
            }
        }

        private void DrawMapIcons(Vector2 mapCenter, float iconSizeMultiplier)
        {
            var fgDraw = ImGui.GetWindowDrawList();
            var currentAreaInstance = Core.States.InGameStateObject.CurrentAreaInstance;
            if (!currentAreaInstance.Player.TryGetComponent<Render>(out var playerRender))
            {
                return;
            }

            var pPos = new Vector2(playerRender.GridPosition.X, playerRender.GridPosition.Y);
            foreach (var entity in currentAreaInstance.AwakeEntities)
            {
                if (this.Settings.HideOutsideNetworkBubble && !entity.Value.IsValid)
                {
                    continue;
                }

                if (entity.Value.EntityType == EntityTypes.Useless)
                {
                    continue;
                }

                entity.Value.TryGetComponent<Render>(out var entityRender);
                var ePos = new Vector2(entityRender.GridPosition.X, entityRender.GridPosition.Y);
                var fpos = Helper.DeltaInWorldToMapDelta(ePos - pPos, entityRender.TerrainHeight - playerRender.TerrainHeight);
                var iconSizeMultiplierVector = Vector2.One * iconSizeMultiplier;
                switch (entity.Value.EntityType)
                {
                    case EntityTypes.OtherPlayer:
                        entity.Value.TryGetComponent<Player>(out var playerComp);
                        if (this.Settings.ShowPlayersNames)
                        {
                            var pNameSizeH = ImGui.CalcTextSize(playerComp.Name) / 2;
                            fgDraw.AddRectFilled(mapCenter + fpos - pNameSizeH, mapCenter + fpos + pNameSizeH,
                                ImGuiHelper.Color(0, 0, 0, 200));
                            fgDraw.AddText(ImGui.GetFont(), ImGui.GetFontSize(), mapCenter + fpos - pNameSizeH,
                                ImGuiHelper.Color(255, 128, 128, 255), playerComp.Name);
                        }
                        else
                        {
                            var playerIcon = playerComp.Name == this.leaderName
                                ? this.Settings.BaseIcons["Leader"]
                                : this.Settings.BaseIcons["Player"];
                            iconSizeMultiplierVector *= playerIcon.IconScale;
                            fgDraw.AddImage(
                                playerIcon.TexturePtr,
                                mapCenter + fpos - iconSizeMultiplierVector,
                                mapCenter + fpos + iconSizeMultiplierVector,
                                playerIcon.UV0,
                                playerIcon.UV1);
                        }

                        break;
                    case EntityTypes.Blockage:
                        entity.Value.TryGetComponent<TriggerableBlockage>(out var blockComp);
                        if (blockComp.IsBlocked)
                        {
                            var blockageIcon = this.Settings.DelveIcons["Blockage OR DelveWall"];
                            iconSizeMultiplierVector *= blockageIcon.IconScale;
                            fgDraw.AddImage(
                                blockageIcon.TexturePtr,
                                mapCenter + fpos - iconSizeMultiplierVector,
                                mapCenter + fpos + iconSizeMultiplierVector,
                                blockageIcon.UV0,
                                blockageIcon.UV1);
                        }

                        break;
                    case EntityTypes.Chest:
                        var chestIcon = this.Settings.BaseIcons["Chests Without Label"];
                        iconSizeMultiplierVector *= chestIcon.IconScale;
                        fgDraw.AddImage(
                            chestIcon.TexturePtr,
                            mapCenter + fpos - iconSizeMultiplierVector,
                            mapCenter + fpos + iconSizeMultiplierVector,
                            chestIcon.UV0,
                            chestIcon.UV1);
                        break;
                    case EntityTypes.ChestWithLabels:
                        chestIcon = this.Settings.BaseIcons["Chests With Label"];
                        iconSizeMultiplierVector *= chestIcon.IconScale;
                        fgDraw.AddImage(
                            chestIcon.TexturePtr,
                            mapCenter + fpos - iconSizeMultiplierVector,
                            mapCenter + fpos + iconSizeMultiplierVector,
                            chestIcon.UV0,
                            chestIcon.UV1);
                        break;
                    case EntityTypes.ExpeditionChest:
                        chestIcon = this.Settings.ExpeditionIcons["Generic Expedition Chests"];
                        iconSizeMultiplierVector *= chestIcon.IconScale;
                        fgDraw.AddImage(
                            chestIcon.TexturePtr,
                            mapCenter + fpos - iconSizeMultiplierVector,
                            mapCenter + fpos + iconSizeMultiplierVector,
                            chestIcon.UV0,
                            chestIcon.UV1);
                        break;
                    case EntityTypes.DelveChest:
                        if (this.delveChestCache.TryGetValue(entity.Key.id, out var iconFinder))
                        {
                            if (this.Settings.DelveIcons.TryGetValue(iconFinder, out var delveChestIcon))
                            {
                                if (delveChestIcon.UV0 == Vector2.Zero)
                                {
                                    var s = ImGui.CalcTextSize(iconFinder) / 2;
                                    fgDraw.AddRectFilled(mapCenter + fpos - s, mapCenter + fpos + s,
                                        ImGuiHelper.Color(0, 0, 0, 255));
                                    fgDraw.AddText(mapCenter + fpos - s, ImGuiHelper.Color(255, 128, 128, 255),
                                        iconFinder);
                                }
                                else
                                {
                                    iconSizeMultiplierVector *= delveChestIcon.IconScale;
                                    fgDraw.AddImage(
                                        delveChestIcon.TexturePtr,
                                        mapCenter + fpos - iconSizeMultiplierVector,
                                        mapCenter + fpos + iconSizeMultiplierVector,
                                        delveChestIcon.UV0,
                                        delveChestIcon.UV1);
                                }
                            }
                        }
                        else
                        {
                            this.delveChestCache[entity.Key.id] =
                                this.DelveChestPathToIcon(entity.Value.Path);
                        }

                        break;
                    case EntityTypes.HeistChest:
                        if (this.heistChestCache.TryGetValue(entity.Key.id, out iconFinder))
                        {
                            if (this.Settings.HeistIcons.TryGetValue(iconFinder, out var heistChestIcon))
                            {
                                iconSizeMultiplierVector *= heistChestIcon.IconScale;
                                fgDraw.AddImage(
                                    heistChestIcon.TexturePtr,
                                    mapCenter + fpos - iconSizeMultiplierVector,
                                    mapCenter + fpos + iconSizeMultiplierVector,
                                    heistChestIcon.UV0,
                                    heistChestIcon.UV1);
                            }
                        }
                        else if (entity.Value.Path.StartsWith(
                            this.heistAllChestStarting, StringComparison.Ordinal))
                        {
                            this.heistChestCache[entity.Key.id] =
                                this.HeistChestPathToIcon(entity.Value.Path);
                        }

                        break;
                    case EntityTypes.ImportantStrongboxChest:
                        chestIcon = this.Settings.BaseIcons["Important Strongboxes"];
                        iconSizeMultiplierVector *= chestIcon.IconScale;
                        fgDraw.AddImage(
                            chestIcon.TexturePtr,
                            mapCenter + fpos - iconSizeMultiplierVector,
                            mapCenter + fpos + iconSizeMultiplierVector,
                            chestIcon.UV0,
                            chestIcon.UV1);
                        break;
                    case EntityTypes.StrongboxChest:
                        chestIcon = this.Settings.BaseIcons["Strongbox"];
                        iconSizeMultiplierVector *= chestIcon.IconScale;
                        fgDraw.AddImage(
                            chestIcon.TexturePtr,
                            mapCenter + fpos - iconSizeMultiplierVector,
                            mapCenter + fpos + iconSizeMultiplierVector,
                            chestIcon.UV0,
                            chestIcon.UV1);
                        break;
                    case EntityTypes.BreachChest:
                        chestIcon = this.Settings.BreachIcons["Breach Chest"];
                        iconSizeMultiplierVector *= chestIcon.IconScale;
                        fgDraw.AddImage(
                            chestIcon.TexturePtr,
                            mapCenter + fpos - iconSizeMultiplierVector,
                            mapCenter + fpos + iconSizeMultiplierVector,
                            chestIcon.UV0,
                            chestIcon.UV1);
                        break;
                    case EntityTypes.Shrine:
                        entity.Value.TryGetComponent<Shrine>(out var shrineComp);
                        if (!shrineComp.IsUsed)
                        {
                            var shrineIcon = this.Settings.BaseIcons["Shrine"];
                            iconSizeMultiplierVector *= shrineIcon.IconScale;
                            fgDraw.AddImage(
                                shrineIcon.TexturePtr,
                                mapCenter + fpos - iconSizeMultiplierVector,
                                mapCenter + fpos + iconSizeMultiplierVector,
                                shrineIcon.UV0,
                                shrineIcon.UV1);
                        }
                        break;
                    case EntityTypes.FriendlyMonster:
                        var friendlyIcon = this.Settings.BaseIcons["Friendly"];
                        iconSizeMultiplierVector *= friendlyIcon.IconScale;
                        fgDraw.AddImage(
                            friendlyIcon.TexturePtr,
                            mapCenter + fpos - iconSizeMultiplierVector,
                            mapCenter + fpos + iconSizeMultiplierVector,
                            friendlyIcon.UV0,
                            friendlyIcon.UV1);
                        break;
                    case EntityTypes.Stage0FIT:
                    case EntityTypes.Stage1FIT:
                    case EntityTypes.Monster:
                        entity.Value.TryGetComponent<ObjectMagicProperties>(out var omp);
                        if (entity.Value.EntityType == EntityTypes.Stage0FIT &&
                            omp.Rarity != Rarity.Unique)
                        {
                            break;
                        }

                        var monsterIcon = this.RarityToIconMapping(omp.Rarity);
                        iconSizeMultiplierVector *= monsterIcon.IconScale;
                        fgDraw.AddImage(
                            monsterIcon.TexturePtr,
                            mapCenter + fpos - iconSizeMultiplierVector,
                            mapCenter + fpos + iconSizeMultiplierVector,
                            monsterIcon.UV0,
                            monsterIcon.UV1);
                        break;
                    case EntityTypes.Stage0RewardFIT:
                    case EntityTypes.Stage1RewardFIT:
                    case EntityTypes.Stage0EChestFIT:
                    case EntityTypes.Stage1EChestFIT:
                        var monsterChestIcon = this.Settings.LegionIcons["Legion Reward Monster/Chest"];
                        if (entity.Value.EntityType == EntityTypes.Stage0EChestFIT ||
                            entity.Value.EntityType == EntityTypes.Stage1EChestFIT)
                        {
                            monsterChestIcon = this.Settings.LegionIcons["Legion Epic Chest"];
                        }

                        if (monsterChestIcon.UV0 == Vector2.Zero)
                        {
                            var fitName = entity.Value.Path.Split('/').LastOrDefault();
                            var s = ImGui.CalcTextSize(fitName) / 2;
                            fgDraw.AddRectFilled(mapCenter + fpos - s, mapCenter + fpos + s,
                                ImGuiHelper.Color(0, 0, 0, 255));
                            fgDraw.AddText(mapCenter + fpos - s, ImGuiHelper.Color(255, 128, 128, 255),
                                fitName);
                        }
                        else
                        {
                            iconSizeMultiplierVector *= monsterChestIcon.IconScale;
                            fgDraw.AddImage(
                                monsterChestIcon.TexturePtr,
                                mapCenter + fpos - iconSizeMultiplierVector,
                                mapCenter + fpos + iconSizeMultiplierVector,
                                monsterChestIcon.UV0,
                                monsterChestIcon.UV1);
                        }

                        break;
                    case EntityTypes.DeliriumBomb:
                        var dHiddenMIcon = this.Settings.DeliriumIcons["Delirium Bomb"];
                        iconSizeMultiplierVector *= dHiddenMIcon.IconScale;
                        fgDraw.AddImage(
                            dHiddenMIcon.TexturePtr,
                            mapCenter + fpos - iconSizeMultiplierVector,
                            mapCenter + fpos + iconSizeMultiplierVector,
                            dHiddenMIcon.UV0,
                            dHiddenMIcon.UV1);
                        break;
                    case EntityTypes.DeliriumSpawner:
                        dHiddenMIcon = this.Settings.DeliriumIcons["Delirium Spawner"];
                        iconSizeMultiplierVector *= dHiddenMIcon.IconScale;
                        fgDraw.AddImage(
                            dHiddenMIcon.TexturePtr,
                            mapCenter + fpos - iconSizeMultiplierVector,
                            mapCenter + fpos + iconSizeMultiplierVector,
                            dHiddenMIcon.UV0,
                            dHiddenMIcon.UV1);
                        break;
                }
            }
        }

        private IEnumerator<Wait> ClearCachesAndUpdateAreaInfo()
        {
            while (true)
            {
                yield return new Wait(RemoteEvents.AreaChanged);
                this.CleanUpRadarPluginCaches();
                this.currentAreaName = Core.States.InGameStateObject.CurrentWorldInstance.AreaDetails.Id;
                this.GenerateMapTexture();
            }
        }

        private IEnumerator<Wait> OnMove()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnMoved);
                this.UpdateMiniMapDetails();
                this.UpdateLargeMapDetails();
                if (this.Settings.MakeCullWindowFullScreen)
                {
                    this.Settings.CullWindowPos = Vector2.Zero;

                    this.Settings.CullWindowSize.X = Core.Process.WindowArea.Size.Width;
                    this.Settings.CullWindowSize.Y = Core.Process.WindowArea.Size.Height;
                    this.skipOneSettingChange = false;
                }
                else if (this.skipOneSettingChange)
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
                this.CleanUpRadarPluginCaches();
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

        private void ReloadMapTexture()
        {
            this.RemoveMapTexture();
            this.GenerateMapTexture();
        }

        private void RemoveMapTexture()
        {
            this.walkableMapTexture = IntPtr.Zero;
            this.walkableMapDimension = Vector2.Zero;
            Core.Overlay.RemoveImage("walkable_map");
        }

        private void GenerateMapTexture()
        {
            if (Core.States.GameCurrentState != GameStateTypes.InGameState &&
                Core.States.GameCurrentState != GameStateTypes.EscapeState)
            {
                return;
            }

            var instance = Core.States.InGameStateObject.CurrentAreaInstance;
            var gridHeightData = instance.GridHeightData;
            var mapWalkableData = instance.GridWalkableData;
            var bytesPerRow = instance.TerrainMetadata.BytesPerRow;
            if (bytesPerRow <= 0)
            {
                return;
            }

            var mapEdgeDetector = new MapEdgeDetector(mapWalkableData, bytesPerRow);
            using Image<Rgba32> image = new(bytesPerRow * 2, mapEdgeDetector.TotalRows);
            Parallel.For(0, gridHeightData.Length, y =>
            {
                for (var x = 1; x < gridHeightData[y].Length - 1; x++)
                {
                    if (!mapEdgeDetector.IsBorder(x, y))
                    {
                        continue;
                    }

                    var height = (int)(gridHeightData[y][x] / 21.91f);
                    var imageX = x - height;
                    var imageY = y - height;

                    if (mapEdgeDetector.IsInsideMapBoundary(imageX, imageY))
                    {
                        image[imageX, imageY] = new Rgba32(this.Settings.WalkableMapColor);
                    }
                }
            });
#if DEBUG
            image.Save(this.DllDirectory +
                       @$"/current_map_{Core.States.InGameStateObject.CurrentAreaInstance.AreaHash}.jpeg");
#endif
            Core.Overlay.AddOrGetImagePointer("walkable_map", image, false, false, out var t, out var w, out var h);
            this.walkableMapTexture = t;
            this.walkableMapDimension = new Vector2(w, h);
        }

        private IconPicker RarityToIconMapping(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Normal or Rarity.Magic or Rarity.Rare or Rarity.Unique => this.Settings.BaseIcons[
                    $"{rarity} Monster"],
                _ => this.Settings.BaseIcons[$"Normal Monster"],
            };
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

        private string DelveChestPathToIcon(string path)
        {
            return path.Replace(this.delveChestStarting, null, StringComparison.Ordinal);
        }

        private void AddNewPOIWidget()
        {
            var tgttilesInArea = Core.States.InGameStateObject.CurrentAreaInstance.TgtTilesLocations;
            ImGui.InputText("Area Name", ref this.currentAreaName, 200, ImGuiInputTextFlags.ReadOnly);
            ImGui.InputInt("Filter on Max POI frenquency", ref this.Settings.POIFrequencyFilter);
            if (ImGui.InputInt("Select POI via Index###tgtSelectorCounter", ref this.tmpTgtSelectionCounter) &&
                this.tmpTgtSelectionCounter < tgttilesInArea.Keys.Count)
            {
                this.tmpTileName = tgttilesInArea.Keys.ElementAt(this.tmpTgtSelectionCounter);
            }

            ImGuiHelper.IEnumerableComboBox("POI Path", tgttilesInArea.Keys, ref this.tmpTileName);
            ImGui.InputText("POI Display Name", ref this.tmpDisplayName, 200);
            if (ImGui.Button("Add POI"))
            {
                if (!string.IsNullOrEmpty(this.currentAreaName) &&
                    !string.IsNullOrEmpty(this.tmpTileName) &&
                    !string.IsNullOrEmpty(this.tmpDisplayName))
                {
                    if (!this.Settings.ImportantTgts.ContainsKey(this.currentAreaName))
                    {
                        this.Settings.ImportantTgts[this.currentAreaName] = new();
                    }

                    this.Settings.ImportantTgts[this.currentAreaName]
                        [this.tmpTileName] = this.tmpDisplayName;

                    this.tmpTileName = string.Empty;
                    this.tmpDisplayName = string.Empty;
                }
            }
        }

        private void ShowPOIWidget()
        {
            if (ImGui.TreeNode($"Important POIs in Area: {this.currentAreaName}##import_time_in_area"))
            {
                if (this.Settings.ImportantTgts.ContainsKey(this.currentAreaName))
                {
                    foreach (var tgt in this.Settings.ImportantTgts[this.currentAreaName])
                    {
                        if (ImGui.SmallButton($"Delete##{tgt.Key}"))
                        {
                            this.Settings.ImportantTgts[this.currentAreaName].Remove(tgt.Key);
                        }

                        ImGui.SameLine();
                        ImGui.Text($"POI Path: {tgt.Key}, Display: {tgt.Value}");
                        ImGuiHelper.ToolTip("Click me to Modify.");
                        if (ImGui.IsItemClicked())
                        {
                            this.tmpTileName = tgt.Key;
                            this.tmpDisplayName = tgt.Value;
                        }
                    }
                }

                ImGui.TreePop();
            }
        }

        private void CleanUpRadarPluginCaches()
        {
            this.heistChestCache.Clear();
            this.delveChestCache.Clear();
            this.RemoveMapTexture();
            this.currentAreaName = string.Empty;
        }
    }
}