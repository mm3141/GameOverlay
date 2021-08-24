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
        /// Gets the terrain data of the current Area/Zone instance.
        /// WARNING: This should only be used together with AreaChange event!.
        /// </summary>
        public byte[] WalkableData =>
            Core.Process.Handle.ReadStdVector<byte>(this.TerrainMetadata.WalkableData);

        /// <summary>
        /// Gets the terrain data of the current Area/Zone instance.
        /// WARNING: This should only be used together with AreaChange event!.
        /// </summary>
        public byte[] LandscapeData =>
            Core.Process.Handle.ReadStdVector<byte>(this.TerrainMetadata.LandscapeData);

        /// <summary>
        /// Converts the <see cref="AreaInstance"/> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Area Hash: {this.AreaHash}");
            ImGui.Text($"Monster Level: {this.MonsterLevel}");
            ImGui.Text($"Terrain Metadata: {this.TerrainMetadata}");
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
