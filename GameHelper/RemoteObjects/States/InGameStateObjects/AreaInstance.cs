// <copyright file="AreaInstance.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;
    using Components;
    using Coroutine;
    using CoroutineEvents;
    using GameHelper.Cache;
    using GameHelper.RemoteEnums;
    using GameHelper.Utils.Stas.GA;
    using GameOffsets.Natives;
    using GameOffsets.Objects.States.InGameState;
    using ImGuiNET;
    using Utils;

    /// <summary>
    ///     core.states.ingame_state=> curr_area_instance like mapper
    /// </summary>
    public class AreaInstance : RemoteObjectBase {
        int frame = 0;
        SW sw = new SW("UpdEntList");
        protected override void UpdateData(bool hasAddressChanged) {

            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<AreaInstanceOffsets>(this.Address);

            if (hasAddressChanged) {
                this.Cleanup(true);
                this.TerrainMetadata = data.TerrainMetadata;
                this.MonsterLevel = data.MonsterLevel;
                this.AreaHash = $"{data.CurrentAreaHash:X}";
                this.GridWalkableData = reader.ReadStdVector<byte>(
                    this.TerrainMetadata.GridWalkableData);
                this.GridHeightData = this.GetTerrainHeight();
                this.TgtTilesLocations = this.GetTgtFileData();
                sw.Restart(true);

            }

            this.UpdateEnvironmentAndCaches(data.Environments);
            this.ServerDataObject.Address = data.ServerDataPtr;
            this.Player.Address = data.LocalPlayerPtr;
            this.UpdateEntities(data.AwakeEntities, this.AwakeEntities, true);
        }
        private void UpdateEntities(StdMap ePtr, ConcurrentDictionary<EntityNodeKey, Entity> data, bool addToCache) {
           
            var reader = Core.Process.Handle;
            var areaDetails = Core.States.InGameStateObject.CurrentWorldInstance.AreaDetails;
            if (Core.GHSettings.DisableEntityProcessingInTownOrHideout && (areaDetails.IsHideout || areaDetails.IsTown)) {
                this.NetworkBubbleEntityCount = 0;
                return;
            }
            sw.Restart();
            var entities = reader.ReadStdMapAsList<EntityNodeKey, EntityNodeValue>(ePtr, EntityFilter.IgnoreVisualsAndDecorations);
            sw.Print("ReadStdMapAsList");
            sw.Restart();
            foreach (var kv in data) {
                if (!kv.Value.IsValid) {
                    if (kv.Value.EntityType == EntityTypes.FriendlyMonster ||
                        (kv.Value.CanExplode &&
                        this.Player.DistanceFrom(kv.Value) < AreaInstanceConstants.NETWORK_BUBBLE_RADIUS)) {
                        // This logic isn't perfect in case something happens to the entity before
                        // we can cache the location of that entity. In that case we will just
                        // delete that entity anyway. This activity is fine as long as it doesn't
                        // crash the GameHelper. This logic is to detect if entity exploded due to
                        // explodi-chest or just left the network bubble since entity leaving network
                        // bubble is same as entity exploded.
                        data.TryRemove(kv.Key, out _);
                    }
                }
                kv.Value.IsValid = false;
            }
            var e_added = 0;
            this.NetworkBubbleEntityCount = entities.Count;
            Parallel.For(0, entities.Count, index => {
                var (key, value) = entities[index];
                if (data.TryGetValue(key, out var entity)) {
                    entity.Address = value.EntityPtr;
                }
                else {
                    entity = new Entity(value.EntityPtr);
                    e_added += 1;
                    if (!string.IsNullOrEmpty(entity.Path)) {
                        data[key] = entity;
                        if (addToCache) {
                            this.AddToCacheParallel(key, entity.Path);
                        }
                    }
                    else {
                        entity = null;
                    }
                }
                entity?.UpdateNearby(this.Player);
            });
            sw.Print("e_added=[" + e_added + "] new/old=[" + entities.Count + "/" + data.Count + "]");
        }
        string entityIdFilter;
        string entityPathFilter;
        bool filterByPath;

        StdVector environmentPtr;
        readonly List<int> environments;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AreaInstance" /> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal AreaInstance(IntPtr address) : base(address) {
            entityIdFilter = string.Empty;
            entityPathFilter = string.Empty;
            filterByPath = false;

            environmentPtr = default;
            environments = new();

            MonsterLevel = 0;
            AreaHash = string.Empty;

            ServerDataObject = new(IntPtr.Zero);
            Player = new();
            AwakeEntities = new();
            EntityCaches = new()
            {
                new("Breach", 1104, 1108, this.AwakeEntities),
                new("LeagueAffliction", 1114, 1114, this.AwakeEntities),
                new("Hellscape", 1244, 1255, this.AwakeEntities)
            };

            NetworkBubbleEntityCount = 0;
            TerrainMetadata = default;
            GridHeightData = Array.Empty<float[]>();
            GridWalkableData = Array.Empty<byte>();
            TgtTilesLocations = new();

            Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(
                this.OnPerFrame(), "[AreaInstance] Update Area Data", int.MaxValue - 3));
        }

        /// <summary>
        ///     Gets the Monster Level of current Area.
        /// </summary>
        public int MonsterLevel { get; private set; }

        /// <summary>
        ///     Gets the Hash of the current Area/Zone.
        ///     This value is sent to the client from the server.
        /// </summary>
        public string AreaHash { get; private set; }

        /// <summary>
        ///     Gets the data related to the player the user is playing.
        /// </summary>
        public ServerData ServerDataObject { get; }

        /// <summary>
        ///     Gets the player Entity.
        /// </summary>
        public Entity Player { get; }

        /// <summary>
        ///     Gets the Awake Entities of the current Area/Zone.
        ///     Awake Entities are the ones which player can interact with
        ///     e.g. Monsters, Players, NPC, Chests and etc. Sleeping entities
        ///     are opposite of awake entities e.g. Decorations, Effects, particles and etc.
        /// </summary>
        public ConcurrentDictionary<EntityNodeKey, Entity> AwakeEntities { get; }

        /// <summary>
        ///     Gets important environments entity caches. This only contain awake entities.
        /// </summary>
        public List<DisappearingEntity> EntityCaches { get; }

        /// <summary>
        ///     Gets the total number of entities (awake as well as sleeping) in the network bubble.
        /// </summary>
        public int NetworkBubbleEntityCount { get; private set; }

        /// <summary>
        ///     Gets the terrain metadata data of the current Area/Zone instance.
        /// </summary>
        public TerrainStruct TerrainMetadata { get; private set; }

        /// <summary>
        ///     Gets the terrain height data.
        /// </summary>
        public float[][] GridHeightData { get; private set; }

        /// <summary>
        ///     Gets the terrain data of the current Area/Zone instance.
        /// </summary>
        public byte[] GridWalkableData { get; private set; }

        /// <summary>
        ///     Gets the Disctionary of Lists containing only the named tgt tiles locations.
        /// </summary>
        public Dictionary<string, List<Vector2>> TgtTilesLocations { get; private set; }

        /// <summary>
        ///    Gets the current zoom value of the world.
        /// </summary>
        public float Zoom {
            get {
                var player = this.Player;

                if (player.TryGetComponent(out Render render)) {
                    var wp = render.WorldPosition;
                    var p0 = Core.States.InGameStateObject.CurrentWorldInstance.WorldToScreen(wp);
                    wp.Z += render.ModelBounds.Z;
                    var p1 = Core.States.InGameStateObject.CurrentWorldInstance.WorldToScreen(wp);

                    return Math.Abs(p1.Y - p0.Y) / render.ModelBounds.Z;
                }

                return 0;
            }
        }

        /// <summary>
        ///     Converts the <see cref="AreaInstance" /> class data to ImGui.
        /// </summary>
        internal override void ToImGui() {
            base.ToImGui();
            if (ImGui.TreeNode("Environment Info")) {
                ImGuiHelper.IntPtrToImGui("Address", this.environmentPtr.First);
                if (ImGui.TreeNode($"All Environments ({this.environments.Count})###AllEnvironments")) {
                    for (var i = 0; i < this.environments.Count; i++) {
                        if (ImGui.Selectable($"{this.environments[i]}")) {
                            ImGui.SetClipboardText($"{this.environments[i]}");
                        }
                    }

                    ImGui.TreePop();
                }

                foreach (var eCache in this.EntityCaches) {
                    eCache.ToImGui();
                }

                ImGui.TreePop();
            }

            ImGui.Text($"Area Hash: {this.AreaHash}");
            ImGui.Text($"Monster Level: {this.MonsterLevel}");
            ImGui.Text($"World Zoom: {this.Zoom}");
            if (ImGui.TreeNode("Terrain Metadata")) {
                ImGui.Text($"Total Tiles: {this.TerrainMetadata.TotalTiles}");
                ImGui.Text($"Tiles Data Pointer: {this.TerrainMetadata.TileDetailsPtr}");
                ImGui.Text($"Tiles Height Multiplier: {this.TerrainMetadata.TileHeightMultiplier}");
                ImGui.Text($"Grid Walkable Data: {this.TerrainMetadata.GridWalkableData}");
                ImGui.Text($"Grid Landscape Data: {this.TerrainMetadata.GridLandscapeData}");
                ImGui.Text($"Data Bytes Per Row (for Walkable/Landscape Data): {this.TerrainMetadata.BytesPerRow}");
                ImGui.TreePop();
            }

            if (this.Player.TryGetComponent<Render>(out var pPos)) {
                var y = (int)pPos.GridPosition.Y;
                var x = (int)pPos.GridPosition.X;
                if (y < this.GridHeightData.Length) {
                    if (x < this.GridHeightData[0].Length) {
                        ImGui.Text("Player Pos to Terrain Height: " +
                                   $"{this.GridHeightData[y][x]}");
                    }
                }
            }

            ImGui.Text($"Entities in network bubble: {this.NetworkBubbleEntityCount}");
            this.EntitiesWidget("Awake", this.AwakeEntities);
        }

        /// <inheritdoc />
        protected override void CleanUpData() {
            this.Cleanup(false);
        }

       

        private void UpdateEnvironmentAndCaches(StdVector environments) {
            this.environments.Clear();
            var reader = Core.Process.Handle;
            this.environmentPtr = environments;
            var envData = reader.ReadStdVector<EnvironmentStruct>(environments);
            for (var i = 0; i < envData.Length; i++) {
                this.environments.Add(envData[i].Key);
            }

            this.EntityCaches.ForEach((eCache) => eCache.UpdateState(this.environments));
        }

        private void AddToCacheParallel(EntityNodeKey key, string path) {
            for (var i = 0; i < this.EntityCaches.Count; i++) {
                if (this.EntityCaches[i].TryAddParallel(key, path)) {
                    break;
                }
            }
        }

       

        private Dictionary<string, List<Vector2>> GetTgtFileData() {
            var reader = Core.Process.Handle;
            var tileData = reader.ReadStdVector<TileStructure>(this.TerrainMetadata.TileDetailsPtr);
            var ret = new Dictionary<string, List<Vector2>>();
            object mylock = new();
            Parallel.For(
                0,
                tileData.Length,
                // happens on every thread, rather than every iteration.
                () => new Dictionary<string, List<Vector2>>(),
                // happens on every iteration.
                (tileNumber, _, localstate) => {
                    var tile = tileData[tileNumber];
                    var tgtFile = reader.ReadMemory<TgtFileStruct>(tile.TgtFilePtr);
                    var tgtName = reader.ReadStdWString(tgtFile.TgtPath);
                    if (string.IsNullOrEmpty(tgtName)) {
                        return localstate;
                    }

                    if (tile.RotationSelector % 2 == 0) {
                        tgtName += $"x:{tile.tileIdX}-y:{tile.tileIdY}";
                    }
                    else {
                        tgtName += $"x:{tile.tileIdY}-y:{tile.tileIdX}";
                    }

                    var loc = new Vector2 {
                        Y = (tileNumber / this.TerrainMetadata.TotalTiles.X) * TileStructure.TileToGridConversion,
                        X = (tileNumber % this.TerrainMetadata.TotalTiles.X) * TileStructure.TileToGridConversion
                    };

                    if (localstate.ContainsKey(tgtName)) {
                        localstate[tgtName].Add(loc);
                    }
                    else {
                        localstate[tgtName] = new() { loc };
                    }

                    return localstate;
                },
                finalresult => // happens on every thread, rather than every iteration.
                {
                    lock (mylock) {
                        foreach (var kv in finalresult) {
                            if (!ret.ContainsKey(kv.Key)) {
                                ret[kv.Key] = new();
                            }

                            ret[kv.Key].AddRange(kv.Value);
                        }
                    }
                });

            return ret;
        }

        private float[][] GetTerrainHeight() {
            var rotationHelper = Core.RotationSelector.Values;
            var rotatorMetrixHelper = Core.RotatorHelper.Values;
            var reader = Core.Process.Handle;
            var tileData = reader.ReadStdVector<TileStructure>(this.TerrainMetadata.TileDetailsPtr);
            var tileHeightCache = new ConcurrentDictionary<IntPtr, sbyte[]>();
            Parallel.For(0, tileData.Length, index => {
                var val = tileData[index];
                tileHeightCache.AddOrUpdate(
                    val.SubTileDetailsPtr,
                    addr => {
                        var subTileData = reader.ReadMemory<SubTileStruct>(addr);
                        var subTileHeightData = reader.ReadStdVector<sbyte>(subTileData.SubTileHeight);
#if DEBUG
                        if (subTileHeightData.Length > TileStructure.TileToGridConversion * TileStructure.TileToGridConversion) {
                            Console.WriteLine($"found new length {subTileHeightData.Length}");
                        }
#endif
                        return subTileHeightData;
                    },
                    (addr, data) => data);
            });

            var gridSizeX = (int)this.TerrainMetadata.TotalTiles.X * TileStructure.TileToGridConversion;
            var gridSizeY = (int)this.TerrainMetadata.TotalTiles.Y * TileStructure.TileToGridConversion;
            var result = new float[gridSizeY][];
            Parallel.For(0, gridSizeY, y => {
                result[y] = new float[gridSizeX];
                for (var x = 0; x < gridSizeX; x++) {
                    var tileDataIndex = y / TileStructure.TileToGridConversion *
                        (int)this.TerrainMetadata.TotalTiles.X + x / TileStructure.TileToGridConversion;
                    var mytiledata = tileData[tileDataIndex];
                    var mytileHeight = tileHeightCache[mytiledata.SubTileDetailsPtr];
                    var exactHeight = 0;
                    if (mytileHeight.Length > 0) // SubTileHeightPtr == SubTileDetailsPtr[1]
                    {
                        var gridXremaining = x % TileStructure.TileToGridConversion;
                        var gridYremaining = y % TileStructure.TileToGridConversion;

                        var rotationSelected = rotationHelper[mytiledata.RotationSelector] * 3;
                        var finalRotatedX = 0;
                        var finalRotatedY = 0;
                        if (rotationSelected >= rotatorMetrixHelper.Length) {
#if DEBUG
                            Console.WriteLine($"rotationSelected: {rotationSelected} > rotatorMetrixHelper.Length: {rotatorMetrixHelper.Length}");
#endif
                            finalRotatedX = gridXremaining;
                            finalRotatedY = gridYremaining;
                        }
                        else {
                            var tmp = TileStructure.TileToGridConversion - 1;
                            var rotatorMetrix = new int[4] {
                                tmp - gridXremaining,
                                gridXremaining,
                                tmp - gridYremaining,
                                gridYremaining
                            };

                            int rotatedX0 = rotatorMetrixHelper[rotationSelected];
                            int rotatedX1 = rotatorMetrixHelper[rotationSelected + 1];
                            int rotatedY0 = rotatorMetrixHelper[rotationSelected + 2];
                            var rotatedY1 = 0;
                            if (rotatedX0 == 0) {
                                rotatedY1 = 2;
                            }

                            finalRotatedX = rotatorMetrix[rotatedX0 * 2 + rotatedX1];
                            finalRotatedY = rotatorMetrix[rotatedY0 + rotatedY1];
                        }

                        var mytileHeightIndex = finalRotatedY * TileStructure.TileToGridConversion + finalRotatedX;
                        exactHeight = mytileHeight[mytileHeightIndex];
                    }

                    result[y][x] = mytiledata.TileHeight * (float)this.TerrainMetadata.TileHeightMultiplier + exactHeight;
                    result[y][x] = result[y][x] * TerrainStruct.TileHeightFinalMultiplier * -1;
                }
            });

            return result;
        }

        /// <summary>
        ///     knows how to clean up the <see cref="AreaInstance"/> class.
        /// </summary>
        /// <param name="isAreaChange">
        ///     true in case it's a cleanup due to area change otherwise false.
        /// </param>
        private void Cleanup(bool isAreaChange) {
            this.AwakeEntities.Clear();
            this.EntityCaches.ForEach((e) => e.Clear());

            if (!isAreaChange) {
                this.environmentPtr = default;
                this.environments.Clear();
                this.MonsterLevel = 0;
                this.AreaHash = string.Empty;
                this.ServerDataObject.Address = IntPtr.Zero;
                this.Player.Address = IntPtr.Zero;
                this.NetworkBubbleEntityCount = 0;
                this.TerrainMetadata = default;
                this.GridHeightData = Array.Empty<float[]>();
                this.GridWalkableData = Array.Empty<byte>();
                this.TgtTilesLocations.Clear();
            }
        }

        private void EntitiesWidget(string label, ConcurrentDictionary<EntityNodeKey, Entity> data) {
            if (ImGui.TreeNode($"{label} Entities ({data.Count})###${label} Entities")) {
                if (ImGui.RadioButton("Filter by Id           ", this.filterByPath == false)) {
                    this.filterByPath = false;
                    this.entityPathFilter = string.Empty;
                }

                ImGui.SameLine();
                if (ImGui.RadioButton("Filter by Path", this.filterByPath)) {
                    this.filterByPath = true;
                    this.entityIdFilter = string.Empty;
                }

                if (this.filterByPath) {
                    ImGui.InputText(
                        "Entity Path Filter",
                        ref this.entityPathFilter,
                        100);
                }
                else {
                    ImGui.InputText(
                        "Entity Id Filter",
                        ref this.entityIdFilter,
                        10,
                        ImGuiInputTextFlags.CharsDecimal);
                }

                foreach (var entity in data) {
                    if (!(string.IsNullOrEmpty(this.entityIdFilter) ||
                          $"{entity.Key.id}".Contains(this.entityIdFilter))) {
                        continue;
                    }

                    if (!(string.IsNullOrEmpty(this.entityPathFilter) ||
                          entity.Value.Path.ToLower().Contains(this.entityPathFilter.ToLower()))) {
                        continue;
                    }

                    if (ImGui.TreeNode($"{entity.Value.Id} {entity.Value.Path}")) {
                        entity.Value.ToImGui();
                        ImGui.TreePop();
                    }

                    if (entity.Value.IsValid &&
                        entity.Value.TryGetComponent<Render>(out var eRender)) {
                        ImGuiHelper.DrawText(
                            eRender.WorldPosition,
                            $"ID: {entity.Key.id}");
                    }
                }

                ImGui.TreePop();
            }
        }

        private IEnumerator<Wait> OnPerFrame() {
            while (true) {
                yield return new Wait(GameHelperEvents.PerFrameDataUpdate);
                if (this.Address != IntPtr.Zero) {
                    this.UpdateData(false);
                }
            }
        }
    }
}