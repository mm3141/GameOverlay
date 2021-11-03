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
        // Legion Cache.
        private readonly Dictionary<uint, byte> frozenInTimeEntities = new();

        private readonly List<string> diesAfterTimeIgnore = new()
        {
            "Metadata/Monsters/AtlasExiles/CrusaderInfluenceMonsters/CrusaderArcaneRune",
            "Metadata/Monsters/Daemon/DaemonLaboratoryBlackhole",
            "Metadata/Monsters/AtlasExiles/AtlasExile",
        };
        private readonly HashSet<uint> diesAfterTimeCache = new();

        private readonly string heistUsefullChestContains = "HeistChestSecondary";
        private readonly string heistAllChestStarting = "Metadata/Chests/LeagueHeist";
        private readonly Dictionary<uint, string> heistChestCache = new();

        // Delirium Hidden Monster cache.
        private readonly Dictionary<uint, string> deliriumHiddenMonster = new();

        private readonly string deliriumHiddenMonsterStarting =
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemon";

        private readonly string delveChestStarting = "Metadata/Chests/DelveChests/";
        private readonly Dictionary<uint, string> delveChestCache = new();
        private bool isAzuriteMine = false;

        /// <summary>
        /// If we don't do this, user will be asked to
        /// setup the culling window everytime they open the game.
        /// </summary>
        private bool skipOneSettingChange = false;

        private ActiveCoroutine onMove;
        private ActiveCoroutine onForegroundChange;
        private ActiveCoroutine onGameClose;
        private ActiveCoroutine onAreaChange;

        private string currentAreaName = string.Empty;
        private string tmpTileName = string.Empty;
        private string tmpDisplayName = string.Empty;
        private int tmpExpectedClusters = 1;
        private string leaderName = string.Empty;

        private Vector2 miniMapCenterWithDefaultShift = Vector2.Zero;
        private double miniMapDiagonalLength = 0x00;

        private double largeMapDiagonalLength = 0x00;

        private IntPtr walkableMapTexture = IntPtr.Zero;
        private Vector2 walkableMapDimension = Vector2.Zero;

        private Dictionary<string, TgtClusters> currentAreaImportantTiles = new();

        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");

        private string ImportantTgtPathName => Path.Join(this.DllDirectory, "important_tgt_files.txt");

        /// <inheritdoc/>
        public override void DrawSettings()
        {
            ImGui.TextWrapped("Following slider is for fixing large map icons. " +
                              "You have to use it if you feel that LargeMap Icons " +
                              "are moving while your player is moving. You only have " +
                              "to find a value that works for you per game window resolution. " +
                              "Basically, you don't have to change it unless you change your " +
                              "game window resolution. Also, please contribute back, let me know " +
                              "what resolution you use and what value works best for you. " +
                              "This slider has no impact on mini-map icons. For windowed-full-screen " +
                              "default value should be good enough.");
            ImGui.DragFloat(
                "Large Map Fix",
                ref this.Settings.LargeMapScaleMultiplier,
                0.001f,
                0.01f,
                0.3f);
            ImGui.TextWrapped("If your mini/large map icon are not working/visible. Open this " +
                              "Overlay setting window, click anywhere on it and then hide this Overlay " +
                              "setting window. It will fix the issue.");

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
                ImGui.Checkbox("Draw map in culling window", ref this.Settings.DrawMapInCull);
                ImGui.Checkbox("Draw tiles in culling window", ref this.Settings.DrawTileInCull);
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
                        this.GenerateMapTexture();
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
                    this.GenerateMapTexture();
                }
            }

            ImGui.Separator();
            ImGui.NewLine();
            if (ImGui.RadioButton("Show all tile names", this.Settings.ShowAllTgtNames))
            {
                this.Settings.ShowAllTgtNames = true;
                this.Settings.ShowImportantTgtNames = false;
            }

            ImGui.SameLine();
            if (ImGui.RadioButton("Show important tile names", this.Settings.ShowImportantTgtNames))
            {
                this.Settings.ShowAllTgtNames = false;
                this.Settings.ShowImportantTgtNames = true;
            }

            ImGui.SameLine();
            if (ImGui.RadioButton("Don't show tile name",
                !this.Settings.ShowAllTgtNames && !this.Settings.ShowImportantTgtNames))
            {
                this.Settings.ShowAllTgtNames = false;
                this.Settings.ShowImportantTgtNames = false;
            }

            ImGui.ColorEdit4("Tile text color", ref this.Settings.TgtNameColor);
            ImGui.Checkbox("Put black box around tile text, makes easier to read.",
                ref this.Settings.TgtNameBackground);
            if (ImGui.CollapsingHeader("Important Tile Setting"))
            {
                this.AddNewTileBox();
                this.DisplayAllImportantTile();
            }

            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Checkbox("Hide Entities without Life/Chest component", ref this.Settings.HideUseless);
            ImGui.Checkbox("Hide Entities outside the network bubble", ref this.Settings.HideOutsideNetworkBubble);
            ImGui.Checkbox("Show Player Names", ref this.Settings.ShowPlayersNames);
            ImGui.InputText("Party Leader Name", ref this.leaderName, 200);
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
                    "Selecting first icon from the list of icons will display chest path name rather than the icon.");
            }
        }

        /// <inheritdoc/>
        public override void DrawUI()
        {
            var largeMap = Core.States.InGameStateObject.GameUi.LargeMap;
            var miniMap = Core.States.InGameStateObject.GameUi.MiniMap;
            var areaDetails = Core.States.InGameStateObject.CurrentAreaInstance.AreaDetails;
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
                var largeMapModifiedZoom = largeMap.Zoom * this.Settings.LargeMapScaleMultiplier;
                Helper.DiagonalLength = this.largeMapDiagonalLength;
                Helper.Scale = largeMapModifiedZoom;
                ImGui.SetNextWindowPos(this.Settings.CullWindowPos);
                ImGui.SetNextWindowSize(this.Settings.CullWindowSize);
                ImGui.SetNextWindowBgAlpha(0f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
                ImGui.Begin("Large Map Culling Window", UiHelper.TransparentWindowFlags);
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
                var miniMapRealCenter = this.miniMapCenterWithDefaultShift + miniMap.Shift;
                ImGui.SetNextWindowPos(miniMap.Postion);
                ImGui.SetNextWindowSize(miniMap.Size);
                ImGui.SetNextWindowBgAlpha(0f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
                ImGui.Begin("###minimapRadar", UiHelper.TransparentWindowFlags);
                ImGui.PopStyleVar();
                this.DrawMapIcons(miniMapRealCenter, miniMap.Zoom);
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

            if (File.Exists(this.ImportantTgtPathName))
            {
                var tgtfiles = File.ReadAllText(this.ImportantTgtPathName);
                this.Settings.ImportantTgts = JsonConvert.DeserializeObject<
                    Dictionary<string, Dictionary<string, TgtClusters>>>(tgtfiles);
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

            var tgtfiles = JsonConvert.SerializeObject(
                this.Settings.ImportantTgts, Formatting.Indented);
            File.WriteAllText(this.ImportantTgtPathName, tgtfiles);
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
            var col = UiHelper.Color(
                (uint)(this.Settings.TgtNameColor.X * 255),
                (uint)(this.Settings.TgtNameColor.Y * 255),
                (uint)(this.Settings.TgtNameColor.Z * 255),
                (uint)(this.Settings.TgtNameColor.W * 255));

            ImDrawListPtr fgDraw;
            if (this.Settings.DrawTileInCull)
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
            if (this.Settings.ShowAllTgtNames)
            {
                foreach (var tgtKV in currentAreaInstance.TgtTilesLocations)
                {
                    var pNameSizeH = ImGui.CalcTextSize(tgtKV.Key) / 2;
                    for (var i = 0; i < tgtKV.Value.Count; i++)
                    {
                        var val = tgtKV.Value[i];
                        var ePos = new Vector2(val.X, val.Y);
                        var fpos = Helper.DeltaInWorldToMapDelta(
                            ePos - pPos,
                            -playerRender.TerrainHeight - currentAreaInstance.GridHeightData[val.Y][val.X]);
                        if (this.Settings.TgtNameBackground)
                        {
                            fgDraw.AddRectFilled(mapCenter + fpos - pNameSizeH, mapCenter + fpos + pNameSizeH,
                                UiHelper.Color(0, 0, 0, 200));
                        }

                        fgDraw.AddText(ImGui.GetFont(), ImGui.GetFontSize(), mapCenter + fpos - pNameSizeH, col,
                            tgtKV.Key);
                    }
                }
            }
            else if (this.Settings.ShowImportantTgtNames)
            {
                foreach (var tile in this.currentAreaImportantTiles)
                {
                    if (!tile.Value.IsValid())
                    {
                        continue;
                    }

                    for (var i = 0; i < tile.Value.ClustersCount; i++)
                    {
                        float height = 0;
                        var loc = tile.Value.Clusters[i];
                        if (loc.X < currentAreaInstance.GridHeightData[0].Length &&
                            loc.Y < currentAreaInstance.GridHeightData.Length)
                        {
                            height = currentAreaInstance.GridHeightData[(int)loc.Y][(int)loc.X];
                        }

                        var display = tile.Value.Display;
                        var pNameSizeH = ImGui.CalcTextSize(display) / 2;
                        var fpos = Helper.DeltaInWorldToMapDelta(
                            loc - pPos, -playerRender.TerrainHeight + height);
                        if (this.Settings.TgtNameBackground)
                        {
                            fgDraw.AddRectFilled(mapCenter + fpos - pNameSizeH, mapCenter + fpos + pNameSizeH,
                                UiHelper.Color(0, 0, 0, 200));
                        }

                        fgDraw.AddText(ImGui.GetFont(), ImGui.GetFontSize(), mapCenter + fpos - pNameSizeH, col,
                            display);
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

                var hasVital = entity.Value.TryGetComponent<Life>(out var lifeComp);
                var hasBuffs = entity.Value.TryGetComponent<Buffs>(out var buffsComp);
                var isChest = entity.Value.TryGetComponent<Chest>(out var chestComp);
                var hasOMP = entity.Value.TryGetComponent<ObjectMagicProperties>(out var omp);
                var isShrine = entity.Value.TryGetComponent<Shrine>(out var shrineComp);
                var isBlockage = entity.Value.TryGetComponent<TriggerableBlockage>(out var blockageComp);
                var isPlayer = entity.Value.TryGetComponent<Player>(out var playerComp);
                var isPosAvailable = entity.Value.TryGetComponent<Positioned>(out var entityPos);
                var isRenderAvailable = entity.Value.TryGetComponent<Render>(out var entityRender);
                var isDiesAfterTime = entity.Value.TryGetComponent<DiesAfterTime>(out var _);

                if (!isPosAvailable || !isRenderAvailable)
                {
                    continue;
                }
                else if (this.Settings.HideUseless)
                {
                    if (isDiesAfterTime)
                    {
                        if (this.diesAfterTimeCache.Contains(entity.Value.Id))
                        {
                            continue;
                        }
                        else if (this.diesAfterTimeIgnore.Any(ignorePath =>
                        entity.Value.Path.StartsWith(ignorePath)))
                        {
                            this.diesAfterTimeCache.Add(entity.Value.Id);
                            continue;
                        }
                    }

                    if (!(hasVital || isChest || isPlayer))
                    {
                        continue;
                    }
                    else if (isChest && chestComp.IsOpened)
                    {
                        continue;
                    }
                    else if (hasVital && (!lifeComp.IsAlive || (!hasOMP && !isBlockage && !isPlayer)))
                    {
                        continue;
                    }
                    else if (isBlockage && !blockageComp.IsBlocked)
                    {
                        continue;
                    }
                    else if (isPlayer && entity.Value.Address ==
                        Core.States.InGameStateObject.CurrentAreaInstance.Player.Address)
                    {
                        continue;
                    }
                }

                var ePos = new Vector2(entityRender.GridPosition.X, entityRender.GridPosition.Y);
                var fpos = Helper.DeltaInWorldToMapDelta(
                    ePos - pPos, entityRender.TerrainHeight - playerRender.TerrainHeight);
                var iconSizeMultiplierVector = Vector2.One * iconSizeMultiplier;
                if (isPlayer)
                {
                    if (this.Settings.ShowPlayersNames)
                    {
                        var pNameSizeH = ImGui.CalcTextSize(playerComp.Name) / 2;
                        fgDraw.AddRectFilled(mapCenter + fpos - pNameSizeH, mapCenter + fpos + pNameSizeH,
                            UiHelper.Color(0, 0, 0, 200));
                        fgDraw.AddText(ImGui.GetFont(), ImGui.GetFontSize(), mapCenter + fpos - pNameSizeH,
                            UiHelper.Color(255, 128, 128, 255), playerComp.Name);
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
                }
                else if (isBlockage)
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
                else if (isChest)
                {
                    if (this.isAzuriteMine)
                    {
                        if (this.delveChestCache.TryGetValue(entity.Key.id, out var iconFinder))
                        {
                            if (this.Settings.DelveIcons.TryGetValue(iconFinder, out var delveChestIcon))
                            {
                                // Have to force keep the Delve Chest since GGG reduced
                                // the network bubble radius for them.
                                entity.Value.ForceKeepEntity();
                                if (delveChestIcon.UV0 == Vector2.Zero)
                                {
                                    var s = ImGui.CalcTextSize(iconFinder) / 2;
                                    fgDraw.AddRectFilled(mapCenter + fpos - s, mapCenter + fpos + s,
                                        UiHelper.Color(0, 0, 0, 255));
                                    fgDraw.AddText(mapCenter + fpos - s, UiHelper.Color(255, 128, 128, 255),
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

                        continue;
                    }

                    if (entity.Value.TryGetComponent<MinimapIcon>(out var _))
                    {
                        if (this.heistChestCache.TryGetValue(entity.Key.id, out var iconFinder))
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

                            continue;
                        }
                        else if (entity.Value.Path.StartsWith(
                            this.heistAllChestStarting, StringComparison.Ordinal))
                        {
                            this.heistChestCache[entity.Key.id] =
                                this.HeistChestPathToIcon(entity.Value.Path);
                            continue;
                        }

                        continue;
                    }

                    var chestIcon = chestComp.IsBreachOrLarge
                        ? currentAreaInstance.DisappearingEntities.TryGetValue(entity.Key, out var league) &&
                          league == LeagueMechanicType.Breach ? this.Settings.BreachIcons["Breach Chest"] :
                        this.Settings.BaseIcons["Large Chest"]
                        : this.Settings.BaseIcons["Mini Breakable Chest"];
                    if (chestComp.IsStrongbox && !chestComp.IsBreachOrLarge)
                    {
                        if (entity.Value.Path.StartsWith("Metadata/Chests/StrongBoxes/Arcanist") ||
                            entity.Value.Path.StartsWith("Metadata/Chests/StrongBoxes/Cartographer") ||
                            entity.Value.Path.StartsWith("Metadata/Chests/StrongBoxes/StrongboxDivination"))
                        {
                            chestIcon = this.Settings.BaseIcons["Arcanist/Cartographer/Divination"];
                        }
                        else
                        {
                            chestIcon = this.Settings.BaseIcons["Strongbox"];
                        }
                    }

                    iconSizeMultiplierVector *= chestIcon.IconScale;
                    fgDraw.AddImage(
                        chestIcon.TexturePtr,
                        mapCenter + fpos - iconSizeMultiplierVector,
                        mapCenter + fpos + iconSizeMultiplierVector,
                        chestIcon.UV0,
                        chestIcon.UV1);
                }
                else if (isShrine)
                {
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

                    continue;
                }
                else if (hasVital)
                {
                    if (hasBuffs && buffsComp.StatusEffects.ContainsKey("frozen_in_time"))
                    {
                        this.frozenInTimeEntities.TryAdd(entity.Key.id, 1);
                        if (buffsComp.StatusEffects.ContainsKey("legion_reward_display") ||
                            entity.Value.Path.Contains("Chest"))
                        {
                            var monsterChestIcon = this.Settings.LegionIcons["Legion Monster Chest"];
                            iconSizeMultiplierVector *= monsterChestIcon.IconScale;
                            fgDraw.AddImage(
                                monsterChestIcon.TexturePtr,
                                mapCenter + fpos - iconSizeMultiplierVector,
                                mapCenter + fpos + iconSizeMultiplierVector,
                                monsterChestIcon.UV0,
                                monsterChestIcon.UV1);
                            continue;
                        }
                    }

                    if (hasBuffs && buffsComp.StatusEffects.ContainsKey("hidden_monster"))
                    {
                        if (this.frozenInTimeEntities.ContainsKey(entity.Key.id))
                        {
                            continue;
                        }

                        if (this.deliriumHiddenMonster.TryGetValue(entity.Key.id, out var iconFinder))
                        {
                            if (this.Settings.DeliriumIcons.TryGetValue(iconFinder, out var dHiddenMIcon))
                            {
                                iconSizeMultiplierVector *= dHiddenMIcon.IconScale;
                                fgDraw.AddImage(
                                    dHiddenMIcon.TexturePtr,
                                    mapCenter + fpos - iconSizeMultiplierVector,
                                    mapCenter + fpos + iconSizeMultiplierVector,
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

                    var monsterIcon = entityPos.IsFriendly
                        ? this.Settings.BaseIcons["Friendly"]
                        : this.RarityToIconMapping(omp.Rarity);
                    iconSizeMultiplierVector *= monsterIcon.IconScale;
                    fgDraw.AddImage(
                        monsterIcon.TexturePtr,
                        mapCenter + fpos - iconSizeMultiplierVector,
                        mapCenter + fpos + iconSizeMultiplierVector,
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
                this.diesAfterTimeCache.Clear();
                this.currentAreaName = Core.States.InGameStateObject.CurrentAreaInstance.AreaDetails.Id;
                this.isAzuriteMine = this.currentAreaName == "Delve_Main";
                this.GenerateMapTexture();
                this.ClusterImportantTgtName();
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
                this.currentAreaName = string.Empty;
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

        private void RemoveMapTexture()
        {
            this.walkableMapTexture = IntPtr.Zero;
            this.walkableMapDimension = Vector2.Zero;
            Core.Overlay.RemoveImage("walkable_map");
        }

        private void GenerateMapTexture()
        {
            this.RemoveMapTexture();
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

        private void ClusterImportantTgtName()
        {
            if (!this.Settings.ImportantTgts.ContainsKey(this.currentAreaName))
            {
                this.currentAreaImportantTiles = new Dictionary<string, TgtClusters>();
                return;
            }

            var currentArea = Core.States.InGameStateObject.CurrentAreaInstance;
            this.currentAreaImportantTiles = this.Settings.ImportantTgts[this.currentAreaName];
            Parallel.ForEach(this.currentAreaImportantTiles, (kv) =>
            {
                if (!currentArea.TgtTilesLocations.ContainsKey(kv.Key))
                {
#if DEBUG
                    Console.WriteLine($"Couldn't find tile name {kv.Key} in area {this.currentAreaName}." +
                                      " Please delete/fix Radar plugin config file.");
#endif
                    kv.Value.MakeInvalid();
                }
                else
                {
                    kv.Value.MakeValid();
                    if (kv.Value.ClustersCount == currentArea.TgtTilesLocations[kv.Key].Count)
                    {
                        for (var i = 0; i < kv.Value.ClustersCount; i++)
                        {
                            kv.Value.Clusters[i].X = currentArea.TgtTilesLocations[kv.Key][i].X;
                            kv.Value.Clusters[i].Y = currentArea.TgtTilesLocations[kv.Key][i].Y;
                        }
                    }
                    else
                    {
                        var tgttile = currentArea.TgtTilesLocations[kv.Key];
                        var rawData = new double[tgttile.Count][];
                        var result = new double[kv.Value.ClustersCount][];
                        for (var i = 0; i < kv.Value.ClustersCount; i++)
                        {
                            result[i] = new double[3] { 0, 0, 0 }; // x-sum, y-sum, total-count.
                        }

                        for (var i = 0; i < tgttile.Count; i++)
                        {
                            rawData[i] = new double[2];
                            rawData[i][0] = tgttile[i].X;
                            rawData[i][1] = tgttile[i].Y;
                        }

                        var cluster = KMean.Cluster(rawData, kv.Value.ClustersCount);
                        for (var i = 0; i < tgttile.Count; i++)
                        {
                            var result_index = cluster[i];
                            result[result_index][0] += rawData[i][0];
                            result[result_index][1] += rawData[i][1];
                            result[result_index][2] += 1;
                        }

                        for (var i = 0; i < result.Length; i++)
                        {
                            kv.Value.Clusters[i].X = (float)(result[i][0] / result[i][2]);
                            kv.Value.Clusters[i].Y = (float)(result[i][1] / result[i][2]);
                        }
                    }
                }
            });
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
            var truncatedPath = path.Replace(
                this.delveChestStarting,
                null,
                StringComparison.Ordinal);

            if (truncatedPath.Length != path.Length)
            {
                return truncatedPath;
            }

            return "Delve Ignore";
        }

        private void AddNewTileBox()
        {
            var tgttilesInArea = Core.States.InGameStateObject.CurrentAreaInstance.TgtTilesLocations;
            ImGui.Text("Leave display name empty if you want to use tile name as display name.");
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 1.3f);
            ImGui.InputText("Area Name", ref this.currentAreaName, 200, ImGuiInputTextFlags.ReadOnly);
            UiHelper.IEnumerableComboBox("Tile Name", tgttilesInArea.Keys, ref this.tmpTileName);
            ImGui.InputText("Display Name", ref this.tmpDisplayName, 200);
            ImGui.Text("Set expected tile count to zero to show all tiles of that name.");
            ImGui.DragInt("Expected Tile Count", ref this.tmpExpectedClusters, 0.01f, 0, 10);
            ImGui.PopItemWidth();
            if (ImGui.Button("Add Tile Name"))
            {
                if (!string.IsNullOrEmpty(this.currentAreaName) &&
                    !string.IsNullOrEmpty(this.tmpTileName))
                {
                    if (this.tmpExpectedClusters == 0)
                    {
                        this.tmpExpectedClusters = tgttilesInArea[this.tmpTileName].Count;
                    }

                    if (string.IsNullOrEmpty(this.tmpDisplayName))
                    {
                        this.tmpDisplayName = this.tmpTileName;
                    }

                    if (!this.Settings.ImportantTgts.ContainsKey(this.currentAreaName))
                    {
                        this.Settings.ImportantTgts[this.currentAreaName] = new Dictionary<string, TgtClusters>();
                    }

                    this.Settings.ImportantTgts[this.currentAreaName][this.tmpTileName] = new TgtClusters()
                    {
                        Display = this.tmpDisplayName,
                        ClustersCount = this.tmpExpectedClusters,
                        Clusters = new Vector2[this.tmpExpectedClusters],
                    };

                    this.tmpTileName = string.Empty;
                    this.tmpDisplayName = string.Empty;
                    this.tmpExpectedClusters = 1;
                }
            }
        }

        private void DisplayAllImportantTile()
        {
            if (ImGui.TreeNode($"Important Tiles in Area: {this.currentAreaName}##import_time_in_area"))
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
                        ImGui.Text(
                            $"Tile Name: {tgt.Key}, Expected Clusters: {tgt.Value.ClustersCount}, Display: {tgt.Value.Display}");
                    }
                }

                ImGui.TreePop();
            }
        }
    }
}