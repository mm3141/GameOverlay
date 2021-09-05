// <copyright file="AreaInstance.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.RemoteObjects.FilesStructures;
    using GameHelper.Utils;
    using GameOffsets.Natives;
    using GameOffsets.Objects.States.InGameState;
    using ImGuiNET;

    /// <summary>
    /// Points to the InGameState -> LocalData Object.
    /// </summary>
    public class AreaInstance : RemoteObjectBase
    {
        private bool filterByPath = false;
        private string entityIdFilter = string.Empty;
        private string entityPathFilter = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="AreaInstance"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal AreaInstance(IntPtr address)
            : base(address)
        {
            Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(
                this.OnPerFrame(), "[AreaInstance] Update Area Data", int.MaxValue - 3));
        }

        /// <summary>
        /// Gets the Monster Level of current Area.
        /// </summary>
        public int MonsterLevel { get; private set; } = 0x00;

        /// <summary>
        /// Gets the Hash of the current Area/Zone.
        /// This value is sent to the client from the server.
        /// </summary>
        public string AreaHash { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the Area Details.
        /// </summary>
        public WorldAreaDat AreaDetails { get; private set; } = new WorldAreaDat(IntPtr.Zero);

        /// <summary>
        /// Gets the player Entity.
        /// </summary>
        public Entity Player { get; private set; } = new Entity();

        /// <summary>
        /// Gets the Awake Entities of the current Area/Zone.
        ///
        /// Awake Entities are the ones which player can interact with
        /// e.g. Monsters, Players, NPC, Chests and etc. Sleeping entities
        /// are opposite of awake entities e.g. Decorations, Effects, particles and etc.
        /// </summary>
        public ConcurrentDictionary<EntityNodeKey, Entity> AwakeEntities { get; private set; } =
            new ConcurrentDictionary<EntityNodeKey, Entity>();

        /// <summary>
        /// Gets the total number of entities in the network bubble.
        /// </summary>
        public int NetworkBubbleEntityCount { get; private set; } = 0x00;

        /// <summary>
        /// Gets the terrain metadata data of the current Area/Zone instance.
        /// </summary>
        public TerrainStruct TerrainMetadata { get; private set; } = default;

        /// <summary>
        /// Gets the terrain height data.
        /// </summary>
        public int[][] GridHeightData { get; private set; } = new int[0][];

        /// <summary>
        /// Gets the terrain data of the current Area/Zone instance.
        /// </summary>
        public byte[] GridWalkableData { get; private set; } = new byte[0];

        /// <summary>
        /// Gets the Disctionary of Lists containing only the named tgt tiles locations.
        /// </summary>
        public Dictionary<string, List<StdTuple2D<int>>> TgtTilesLocations { get; private set; } =
            new Dictionary<string, List<StdTuple2D<int>>>();

        /// <summary>
        /// Converts the <see cref="AreaInstance"/> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Area Hash: {this.AreaHash}");
            ImGui.Text($"Monster Level: {this.MonsterLevel}");
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

            if (this.Player.TryGetComponent<Positioned>(out var pPos))
            {
                if (pPos.GridPosition.Y < this.GridHeightData.Length)
                {
                    if (pPos.GridPosition.X < this.GridHeightData[0].Length)
                    {
                        ImGui.Text("Player Pos to Terrain Height: " +
                            $"{this.GridHeightData[pPos.GridPosition.Y][pPos.GridPosition.X]}");
                    }
                }
            }

            ImGui.Text($"Entities in network bubble: {this.NetworkBubbleEntityCount}");
            if (ImGui.TreeNode($"Awake Entities ({this.AwakeEntities.Count})###Awake Entities"))
            {
                if (ImGui.RadioButton("Filter by Id           ", this.filterByPath == false))
                {
                    this.filterByPath = false;
                    this.entityPathFilter = string.Empty;
                }

                ImGui.SameLine();
                if (ImGui.RadioButton("Filter by Path", this.filterByPath == true))
                {
                    this.filterByPath = true;
                    this.entityIdFilter = string.Empty;
                }

                if (this.filterByPath)
                {
                    ImGui.InputText("Entity Path Filter", ref this.entityPathFilter, 100);
                }
                else
                {
                    ImGui.InputText("Entity Id Filter", ref this.entityIdFilter, 10, ImGuiInputTextFlags.CharsDecimal);
                }

                foreach (var awakeEntity in this.AwakeEntities)
                {
                    if (!(string.IsNullOrEmpty(this.entityIdFilter) ||
                        $"{awakeEntity.Key.id}".Contains(this.entityIdFilter)))
                    {
                        continue;
                    }

                    if (!(string.IsNullOrEmpty(this.entityPathFilter) ||
                        awakeEntity.Value.Path.ToLower().Contains(this.entityPathFilter.ToLower())))
                    {
                        continue;
                    }

                    if (ImGui.TreeNode($"{awakeEntity.Value.Id} {awakeEntity.Value.Path}"))
                    {
                        awakeEntity.Value.ToImGui();
                        ImGui.TreePop();
                    }

                    if (awakeEntity.Value.IsValid &&
                        awakeEntity.Value.TryGetComponent<Render>(out var eRender))
                    {
                        UiHelper.DrawText(
                            eRender.WorldPosition3D,
                            $"ID: {awakeEntity.Key.id}");
                    }
                }

                ImGui.TreePop();
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.MonsterLevel = 0x00;
            this.AreaHash = string.Empty;
            this.Player.Address = IntPtr.Zero;
            this.AreaDetails.Address = IntPtr.Zero;
            this.NetworkBubbleEntityCount = 0x00;
            this.TerrainMetadata = default;
            this.AwakeEntities.Clear();
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<AreaInstanceOffsets>(this.Address);
            if (hasAddressChanged)
            {
                this.AwakeEntities.Clear();
                this.AreaDetails.Address = data.AreaDetailsPtr;
            }

            this.MonsterLevel = data.MonsterLevel;
            this.AreaHash = $"{data.CurrentAreaHash:X}";
#if DEBUG
            var entitiesInNetworkBubble = reader.ReadStdMapAsList<EntityNodeKey, EntityNodeValue>(
                data.AwakeEntities, false, null);
#else
            var entitiesInNetworkBubble = reader.ReadStdMapAsList<EntityNodeKey, EntityNodeValue>(
                data.AwakeEntities, false, EntityFilter.IgnoreSleepingEntities);
#endif
            this.NetworkBubbleEntityCount = entitiesInNetworkBubble.Count;
            this.Player.Address = data.LocalPlayerPtr;
            this.TerrainMetadata = data.TerrainMetadata;

            // No point in saving out of NetworkBubble entities when in BattleRoyale.
            // since other players would have killed that entity anyway. Also, in BR
            // when you die and teleport to other ppl (i.e. spectate) the AwakeEntities
            // gets corrupted anyway.
            if (this.AreaDetails.IsHideout ||
                this.AreaDetails.IsTown ||
                this.AreaDetails.IsBattleRoyale)
            {
                this.AwakeEntities.Clear();
            }

            foreach (var kv in this.AwakeEntities)
            {
                if (!kv.Value.IsValid &&

                    // This logic isn't perfect in case something happens to the entity before
                    // we can cache the location of that entity. In that case we will just
                    // delete that entity anyway. This activity is fine as long as it doesn't
                    // crash the GameHelper.
                    this.Player.DistanceFrom(kv.Value) <
                    AreaInstanceConstants.NETWORK_BUBBLE_RADIUS)
                {
                    this.AwakeEntities.TryRemove(kv.Key, out _);
                    continue;
                }

                kv.Value.IsValid = false;
            }

            Parallel.For(0, entitiesInNetworkBubble.Count, (index) =>
            {
                var (key, value) = entitiesInNetworkBubble[index];
                if (this.AwakeEntities.ContainsKey(key))
                {
                    this.AwakeEntities[key].Address = value.EntityPtr;
                }
                else
                {
                    var entity = new Entity(value.EntityPtr);
                    if (!string.IsNullOrEmpty(entity.Path))
                    {
                        this.AwakeEntities[key] = entity;
                    }
                }
            });

            if (hasAddressChanged)
            {
                this.GridWalkableData = reader.ReadStdVector<byte>(this.TerrainMetadata.GridWalkableData);
                this.GridHeightData = this.GetTerrainHeight();
                this.TgtTilesLocations = this.GetTgtFileData();
            }
        }

        private Dictionary<string, List<StdTuple2D<int>>> GetTgtFileData()
        {
            var reader = Core.Process.Handle;
            var tileData = reader.ReadStdVector<TileStructure>(this.TerrainMetadata.TileDetailsPtr);
            var ret = new Dictionary<string, List<StdTuple2D<int>>>();
            object mylock = new object();
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

                    var loc = new StdTuple2D<int>()
                    {
                        Y = (int)(tileNumber / this.TerrainMetadata.TotalTiles.X) * TileStructure.TileToGridConversion,
                        X = (int)(tileNumber % this.TerrainMetadata.TotalTiles.X) * TileStructure.TileToGridConversion,
                    };

                    if (localstate.ContainsKey(tgtName))
                    {
                        localstate[tgtName].Add(loc);
                    }
                    else
                    {
                        localstate[tgtName] = new List<StdTuple2D<int>>() { loc };
                    }

                    return localstate;
                },
                (finalresult) => // happens on every thread, rather than every iteration.
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

        private int[][] GetTerrainHeight()
        {
            var rotationHelper = Core.RotationSelector.Values;
            var rotatorMetrixHelper = Core.RotatorHelper.Values;
            var reader = Core.Process.Handle;
            var tileData = reader.ReadStdVector<TileStructure>(this.TerrainMetadata.TileDetailsPtr);
            var tileHeightCache = new ConcurrentDictionary<IntPtr, sbyte[]>();
            Parallel.For(0, tileData.Length, (index) =>
            {
                var val = tileData[index];
                tileHeightCache.AddOrUpdate(
                    val.SubTileDetailsPtr,
                    (addr) =>
                    {
                        var subTileData = reader.ReadMemory<SubTileStruct>(addr);
                        return reader.ReadStdVector<sbyte>(subTileData.SubTileHeight);
                    },
                    (addr, data) => data);
            });

            int gridSizeX = (int)this.TerrainMetadata.TotalTiles.X * TileStructure.TileToGridConversion;
            int gridSizeY = (int)this.TerrainMetadata.TotalTiles.Y * TileStructure.TileToGridConversion;
            int[][] result = new int[gridSizeY][];
            Parallel.For(0, gridSizeY, (y) =>
            {
                result[y] = new int[gridSizeX];
                for (int x = 0; x < gridSizeX; x++)
                {
                    int tileDataIndex = (y / TileStructure.TileToGridConversion *
                    (int)this.TerrainMetadata.TotalTiles.X) + (x / TileStructure.TileToGridConversion);
                    var mytiledata = tileData[tileDataIndex];
                    var mytileHeight = tileHeightCache[mytiledata.SubTileDetailsPtr];
                    int exactHeight = 0;
                    if (mytileHeight.Length > 0)
                    {
                        int gridXremaining = x % TileStructure.TileToGridConversion;
                        int gridYremaining = y % TileStructure.TileToGridConversion;
                        int tmp = TileStructure.TileToGridConversion - 1;
                        int[] rotatorMetrix = new int[4]
                        {
                            tmp - gridXremaining,
                            gridXremaining,
                            tmp - gridYremaining,
                            gridYremaining,
                        };

                        int rotationSelected = rotationHelper[mytiledata.RotationSelector] * 3;
                        int rotatedX0 = rotatorMetrixHelper[rotationSelected];
                        int rotatedX1 = rotatorMetrixHelper[rotationSelected + 1];
                        int rotatedY0 = rotatorMetrixHelper[rotationSelected + 2];
                        int rotatedY1 = 0;
                        if (rotatedX0 == 0)
                        {
                            rotatedY1 = 2;
                        }

                        int finalRotatedX = rotatorMetrix[(rotatedX0 * 2) + rotatedX1];
                        int finalRotatedY = rotatorMetrix[rotatedY0 + rotatedY1];
                        int mytileHeightIndex = (finalRotatedY * TileStructure.TileToGridConversion) + finalRotatedX;
                        exactHeight = mytileHeight[mytileHeightIndex];
                    }

                    result[y][x] = (mytiledata.TileHeight * (int)this.TerrainMetadata.TileHeightMultiplier) + exactHeight;
                    result[y][x] = (int)Math.Round(result[y][x] * TerrainStruct.TileHeightFinalMultiplier);
                }
            });

            return result;
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
