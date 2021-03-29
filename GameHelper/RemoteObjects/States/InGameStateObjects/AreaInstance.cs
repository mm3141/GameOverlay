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
        private string entityFilter = string.Empty;

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
        /// Converts the <see cref="AreaInstance"/> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Area Hash: {this.AreaHash}");
            ImGui.Text($"Monster Level: {this.MonsterLevel}");
            ImGui.Text($"Entities in network bubble: {this.NetworkBubbleEntityCount}");
            if (ImGui.TreeNode($"Awake Entities ({this.AwakeEntities.Count})###Awake Entities"))
            {
                ImGui.InputText("Entity Filter", ref this.entityFilter, 10, ImGuiInputTextFlags.CharsDecimal);
                foreach (var awakeEntity in this.AwakeEntities)
                {
                    if (!(string.IsNullOrEmpty(this.entityFilter) ||
                        $"{awakeEntity.Key.id}".Contains(this.entityFilter)))
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
                            $"ID: {awakeEntity.Key.id} - Type: {awakeEntity.Key.type:X}");
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
            this.AwakeEntities.Clear();
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            // TODO: Find new stuff
            // TODO: Create patterns for stuff, for easy finding.
            // TODO: Enabling Docking/Multi-view-port
            //          docking enabled will allow setting window to use tabs rather than 1 screen.
            //          multi-view-port will allow folks to use multiple monitors
            // TODO: HoverUi debugger. Should popup (beside mouse) "You are hovering over a UIElement, press J to debug it in DevTree.".
            // TODO: UiElement explorer that also handle InGameUi array (try/catch).
            // TODO: loaded file searcher should allow comma seperated words
            // TODO: Search entities by pathname (allow comma seperated words here too)
            // TODO: Use grid pos for detecting exploding entities and make it variable.
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
            foreach (var kv in this.AwakeEntities)
            {
                if (!kv.Value.IsValid &&

                    // This isn't perfect in case something happens to the entity before
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
