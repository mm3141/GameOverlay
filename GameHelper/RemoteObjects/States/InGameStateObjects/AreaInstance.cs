// <copyright file="AreaInstance.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Components;
    using Coroutine;
    using CoroutineEvents;
    using FilesStructures;
    using GameHelper.Cache;
    using GameOffsets.Natives;
    using GameOffsets.Objects.States.InGameState;
    using ImGuiNET;
    using Utils;

    /// <summary>
    ///     Points to the InGameState -> LocalData Object.
    /// </summary>
    public class AreaInstance : RemoteObjectBase
    {
        private string entityIdFilter;
        private string entityPathFilter;
        private bool filterByPath;

        private StdVector environmentPtr;
        private readonly List<int> environments;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AreaInstance" /> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal AreaInstance(IntPtr address)
            : base(address)
        {
            this.entityIdFilter = string.Empty;
            this.entityPathFilter = string.Empty;
            this.filterByPath = false;

            this.environmentPtr = default;
            this.environments = new();

            this.MonsterLevel = 0;
            this.AreaHash = string.Empty;
            this.AreaDetails = new(IntPtr.Zero);

            this.ServerDataObject = new(IntPtr.Zero);
            this.Player = new();
            this.AwakeEntities = new();
            this.EntityCaches = new()
            {
                new("Breach", 1088, 1092, this.AwakeEntities),
                new("LeagueAffliction", 1098, 1098, this.AwakeEntities),
                new("Hellscape", 1228, 1239, this.AwakeEntities)
            };

            this.NetworkBubbleEntityCount = 0;
            this.TerrainMetadata = default;
            this.GridHeightData = Array.Empty<float[]>();
            this.GridWalkableData = Array.Empty<byte>();
            this.TgtTilesLocations = new();

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
        ///     Gets the Area Details.
        /// </summary>
        public WorldAreaDat AreaDetails { get; }

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
        public Dictionary<string, List<StdTuple2D<int>>> TgtTilesLocations { get; private set; }

        /// <summary>
        ///    Gets the current zoom value of the world.
        /// </summary>
        public float Zoom
        {
            get
            {
                var player = this.Player;

                if (player.TryGetComponent(out Render render))
                {
                    var wp = render.WorldPosition;
                    var p0 = Core.States.InGameStateObject.WorldToScreen(wp);
                    wp.Z += render.ModelBounds.Z;
                    var p1 = Core.States.InGameStateObject.WorldToScreen(wp);

                    return Math.Abs(p1.Y - p0.Y) / render.ModelBounds.Z;
                }

                return 0;
            }
        }

        /// <summary>
        ///     Converts the <see cref="AreaInstance" /> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            if (ImGui.TreeNode("Environment Info"))
            {
                ImGuiHelper.IntPtrToImGui("Address", this.environmentPtr.First);
                if (ImGui.TreeNode($"All Environments ({this.environments.Count})###AllEnvironments"))
                {
                    for (var i = 0; i < this.environments.Count; i++)
                    {
                        if (ImGui.Selectable($"{this.environments[i]}"))
                        {
                            ImGui.SetClipboardText($"{this.environments[i]}");
                        }
                    }

                    ImGui.TreePop();
                }

                foreach (var eCache in this.EntityCaches)
                {
                    eCache.ToImGui();
                }

                ImGui.TreePop();
            }

            ImGui.Text($"Area Hash: {this.AreaHash}");
            ImGui.Text($"Monster Level: {this.MonsterLevel}");
            ImGui.Text($"World Zoom: {this.Zoom}");
            if (ImGui.TreeNode("Terrain Metadata"))
            {
                ImGui.Text($"Total Tiles: {this.TerrainMetadata.TotalTiles}");
                ImGui.Text($"Tiles Data Pointer: {this.TerrainMetadata.TileDetailsPtr}");
                ImGui.Text($"Tiles Height Multiplier: {this.TerrainMetadata.TileHeightMultiplier}");
                ImGui.Text($"Grid Walkable Data: {this.TerrainMetadata.GridWalkableData}");
                ImGui.Text($"Grid Landscape Data: {this.TerrainMetadata.GridLandscapeData}");
                ImGui.Text($"Data Bytes Per Row (for Walkable/Landscape Data): {this.TerrainMetadata.BytesPerRow}");
                ImGui.TreePop();
            }

            if (this.Player.TryGetComponent<Render>(out var pPos))
            {
                var y = (int)pPos.GridPosition.Y;
                var x = (int)pPos.GridPosition.X;
                if (y < this.GridHeightData.Length)
                {
                    if (x < this.GridHeightData[0].Length)
                    {
                        ImGui.Text("Player Pos to Terrain Height: " +
                                   $"{this.GridHeightData[y][x]}");
                    }
                }
            }

            ImGui.Text($"Entities in network bubble: {this.NetworkBubbleEntityCount}");
            this.EntitiesWidget("Awake", this.AwakeEntities);
        }

        /// <inheritdoc />
        protected override void CleanUpData()
        {
            this.Cleanup(false);
        }

        /// <inheritdoc />
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<AreaInstanceOffsets>(this.Address);

            if (hasAddressChanged)
            {
                this.Cleanup(true);
                this.AreaDetails.Address = data.AreaDetailsPtr;
                this.TerrainMetadata = data.TerrainMetadata;
                this.MonsterLevel = data.MonsterLevel;
                this.AreaHash = $"{data.CurrentAreaHash:X}";
                this.GridWalkableData = reader.ReadStdVector<byte>(
                    this.TerrainMetadata.GridWalkableData);
                this.GridHeightData = this.GetTerrainHeight();
                this.TgtTilesLocations = this.GetTgtFileData();
            }

            this.UpdateEnvironmentAndCaches(data.Environments);
            this.ServerDataObject.Address = data.ServerDataPtr;
            this.Player.Address = data.LocalPlayerPtr;

            // reading Render to avoid concurrent modification in UpdateEntites func.
            this.Player.TryGetComponent<Render>(out var _);
            this.UpdateEntities(data.AwakeEntities, this.AwakeEntities, true);
        }

        private void UpdateEnvironmentAndCaches(StdVector environments)
        {
            this.environments.Clear();
            var reader = Core.Process.Handle;
            this.environmentPtr = environments;
            var envData = reader.ReadStdVector<EnvironmentStruct>(environments);
            for (var i = 0; i < envData.Length; i++)
            {
                this.environments.Add(envData[i].Key);
            }

            this.EntityCaches.ForEach((eCache) => eCache.UpdateState(this.environments));
        }

        private void AddToCacheParallel(EntityNodeKey key, string path)
        {
            for (var i = 0; i < this.EntityCaches.Count; i++)
            {
                if (this.EntityCaches[i].TryAddParallel(key, path))
                {
                    break;
                }
            }
        }

        private void UpdateEntities(
            StdMap ePtr,
            ConcurrentDictionary<EntityNodeKey, Entity> data,
            bool addToCache)
        {
            var reader = Core.Process.Handle;
            if (Core.GHSettings.DisableEntityProcessingInTownOrHideout &&
                (this.AreaDetails.IsHideout || this.AreaDetails.IsTown))
            {
                this.NetworkBubbleEntityCount = 0;
                return;
            }

            var entities = reader.ReadStdMapAsList<EntityNodeKey, EntityNodeValue>(
                ePtr, EntityFilter.IgnoreVisualsAndDecorations);
            foreach (var kv in data)
            {
                if (!kv.Value.IsValid)
                {
                    // This logic isn't perfect in case something happens to the entity before
                    // we can cache the location of that entity. In that case we will just
                    // delete that entity anyway. This activity is fine as long as it doesn't
                    // crash the GameHelper. This logic is to detect if entity exploded due to
                    // explodi-chest or just left the network bubble since entity leaving network
                    // bubble is same as entity exploded.
                    if (this.Player.DistanceFrom(kv.Value) < AreaInstanceConstants.NETWORK_BUBBLE_RADIUS)
                    {
                        data.TryRemove(kv.Key, out _);
                    }
                }

                kv.Value.IsValid = false;
            }

            this.NetworkBubbleEntityCount = entities.Count;
            Parallel.For(0, entities.Count, index =>
            {
                var (key, value) = entities[index];
                if (data.TryGetValue(key, out var entity))
                {
                    entity.Address = value.EntityPtr;
                }
                else
                {
                    entity = new Entity(value.EntityPtr);
                    if (!string.IsNullOrEmpty(entity.Path))
                    {
                        data[key] = entity;
                        if (addToCache)
                        {
                            this.AddToCacheParallel(key, entity.Path);
                        }
                    }
                    else
                    {
                        entity = null;
                    }
                }

                entity?.UpdateNearby(this.Player);
            });
        }

        private Dictionary<string, List<StdTuple2D<int>>> GetTgtFileData()
        {
            var reader = Core.Process.Handle;
            var tileData = reader.ReadStdVector<TileStructure>(this.TerrainMetadata.TileDetailsPtr);
            var ret = new Dictionary<string, List<StdTuple2D<int>>>();
            object mylock = new();
            Parallel.For(
                0,
                tileData.Length,
                () => new Dictionary<string, List<StdTuple2D<int>>>(), // happens on every thread, rather than every iteration.
                (tileNumber, _, localstate) => // happens on every iteration.
                {
                    var tile = tileData[tileNumber];
                    var tgtFile = reader.ReadMemory<TgtFileStruct>(tile.TgtFilePtr);
                    var tgtDetail = reader.ReadMemory<TgtDetailStruct>(tgtFile.TgtDetailPtr);
                    var tgtName = reader.ReadStdWString(tgtDetail.name);
                    if (string.IsNullOrEmpty(tgtName))
                    {
                        return localstate;
                    }

                    var loc = new StdTuple2D<int>
                    {
                        Y = (int)(tileNumber / this.TerrainMetadata.TotalTiles.X) * TileStructure.TileToGridConversion,
                        X = (int)(tileNumber % this.TerrainMetadata.TotalTiles.X) * TileStructure.TileToGridConversion
                    };

                    if (localstate.ContainsKey(tgtName))
                    {
                        localstate[tgtName].Add(loc);
                    }
                    else
                    {
                        localstate[tgtName] = new List<StdTuple2D<int>> { loc };
                    }

                    return localstate;
                },
                finalresult => // happens on every thread, rather than every iteration.
                {
                    lock (mylock)
                    {
                        foreach (var kv in finalresult)
                        {
                            if (!ret.ContainsKey(kv.Key))
                            {
                                ret[kv.Key] = new List<StdTuple2D<int>>();
                            }

                            ret[kv.Key].AddRange(kv.Value);
                        }
                    }
                });
            return ret;
        }

        private float[][] GetTerrainHeight()
        {
            var rotationHelper = Core.RotationSelector.Values;
            var rotatorMetrixHelper = Core.RotatorHelper.Values;
            var reader = Core.Process.Handle;
            var tileData = reader.ReadStdVector<TileStructure>(this.TerrainMetadata.TileDetailsPtr);
            var tileHeightCache = new ConcurrentDictionary<IntPtr, sbyte[]>();
            Parallel.For(0, tileData.Length, index =>
            {
                var val = tileData[index];
                tileHeightCache.AddOrUpdate(
                    val.SubTileDetailsPtr,
                    addr =>
                    {
                        var subTileData = reader.ReadMemory<SubTileStruct>(addr);
                        return reader.ReadStdVector<sbyte>(subTileData.SubTileHeight);
                    },
                    (addr, data) => data);
            });

            var gridSizeX = (int)this.TerrainMetadata.TotalTiles.X * TileStructure.TileToGridConversion;
            var gridSizeY = (int)this.TerrainMetadata.TotalTiles.Y * TileStructure.TileToGridConversion;
            var result = new float[gridSizeY][];
            Parallel.For(0, gridSizeY, y =>
            {
                result[y] = new float[gridSizeX];
                for (var x = 0; x < gridSizeX; x++)
                {
                    var tileDataIndex = y / TileStructure.TileToGridConversion *
                        (int)this.TerrainMetadata.TotalTiles.X + x / TileStructure.TileToGridConversion;
                    var mytiledata = tileData[tileDataIndex];
                    var mytileHeight = tileHeightCache[mytiledata.SubTileDetailsPtr];
                    var exactHeight = 0;
                    if (mytileHeight.Length > 0)
                    {
                        var gridXremaining = x % TileStructure.TileToGridConversion;
                        var gridYremaining = y % TileStructure.TileToGridConversion;
                        var tmp = TileStructure.TileToGridConversion - 1;
                        var rotatorMetrix = new int[4]
                        {
                            tmp - gridXremaining,
                            gridXremaining,
                            tmp - gridYremaining,
                            gridYremaining
                        };

                        var rotationSelected = rotationHelper[mytiledata.RotationSelector] * 3;
                        int rotatedX0 = rotatorMetrixHelper[rotationSelected];
                        int rotatedX1 = rotatorMetrixHelper[rotationSelected + 1];
                        int rotatedY0 = rotatorMetrixHelper[rotationSelected + 2];
                        var rotatedY1 = 0;
                        if (rotatedX0 == 0)
                        {
                            rotatedY1 = 2;
                        }

                        var finalRotatedX = rotatorMetrix[rotatedX0 * 2 + rotatedX1];
                        var finalRotatedY = rotatorMetrix[rotatedY0 + rotatedY1];
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
        private void Cleanup(bool isAreaChange)
        {
            this.AwakeEntities.Clear();
            this.EntityCaches.ForEach((e) => e.Clear());

            if (!isAreaChange)
            {
                this.environmentPtr = default;
                this.environments.Clear();
                this.MonsterLevel = 0;
                this.AreaHash = string.Empty;
                this.AreaDetails.Address = IntPtr.Zero;
                this.ServerDataObject.Address = IntPtr.Zero;
                this.Player.Address = IntPtr.Zero;
                this.NetworkBubbleEntityCount = 0;
                this.TerrainMetadata = default;
                this.GridHeightData = Array.Empty<float[]>();
                this.GridWalkableData = Array.Empty<byte>();
                this.TgtTilesLocations.Clear();
            }
        }

        private void EntitiesWidget(string label, ConcurrentDictionary<EntityNodeKey, Entity> data)
        {
            if (ImGui.TreeNode($"{label} Entities ({data.Count})###${label} Entities"))
            {
                if (ImGui.RadioButton("Filter by Id           ", this.filterByPath == false))
                {
                    this.filterByPath = false;
                    this.entityPathFilter = string.Empty;
                }

                ImGui.SameLine();
                if (ImGui.RadioButton("Filter by Path", this.filterByPath))
                {
                    this.filterByPath = true;
                    this.entityIdFilter = string.Empty;
                }

                if (this.filterByPath)
                {
                    ImGui.InputText(
                        "Entity Path Filter",
                        ref this.entityPathFilter,
                        100);
                }
                else
                {
                    ImGui.InputText(
                        "Entity Id Filter",
                        ref this.entityIdFilter,
                        10,
                        ImGuiInputTextFlags.CharsDecimal);
                }

                foreach (var entity in data)
                {
                    if (!(string.IsNullOrEmpty(this.entityIdFilter) ||
                          $"{entity.Key.id}".Contains(this.entityIdFilter)))
                    {
                        continue;
                    }

                    if (!(string.IsNullOrEmpty(this.entityPathFilter) ||
                          entity.Value.Path.ToLower().Contains(this.entityPathFilter.ToLower())))
                    {
                        continue;
                    }

                    if (ImGui.TreeNode($"{entity.Value.Id} {entity.Value.Path}"))
                    {
                        entity.Value.ToImGui();
                        ImGui.TreePop();
                    }

                    if (entity.Value.IsValid &&
                        entity.Value.TryGetComponent<Render>(out var eRender))
                    {
                        ImGuiHelper.DrawText(
                            eRender.WorldPosition,
                            $"ID: {entity.Key.id}");
                    }
                }

                ImGui.TreePop();
            }
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