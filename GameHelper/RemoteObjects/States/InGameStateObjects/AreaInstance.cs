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
    using GameOffsets.Objects.States.InGameState;

    /// <summary>
    /// Points to the InGameState -> LocalData Object.
    /// </summary>
    public class AreaInstance : RemoteObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AreaInstance"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal AreaInstance(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnAreaChange());
            Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(
                this.OnPerFrame(), "[AreaInstance] Update Area Data"));
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

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.MonsterLevel = 0x00;
            this.AreaHash = string.Empty;
            this.AwakeEntities.Clear();
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            // TODO: Find new stuff
            // TODO: Create patterns for stuff, for easy finding.
            // TODO: Create ToRender & DevTree type plugin for base classes
            // TODO: moveable Ui Element ptr. so person can triverse all UiElements via this dev tree and UiRoot.
            // TODO: HoverUi debugger. Should popup (beside mouse) "You are hovering over a UIElement, press J to debug it in DevTree.".
            // TODO: Change preload to checklist manager.
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<CurrentAreaDataOffsets>(this.Address);
            this.MonsterLevel = data.MonsterLevel;
            this.AreaHash = $"{data.CurrentAreaHash:X}";
            var entitiesInNetworkBubble = reader.ReadStdMapAsList<EntityNodeKey, EntityNodeValue>(
                data.AwakeEntities, false, EntityFilter.IgnoreSleepingEntities);
            this.Player.Address = data.LocalPlayerPtr;
            foreach (var kv in this.AwakeEntities)
            {
                if (!kv.Value.IsValid &&

                    // This isn't perfect in case entity is deleted before
                    // we can cache the location of that entity. This is fine
                    // as long as it doesn't crash the GameHelper.
                    this.Player.DistanceFrom(kv.Value) <
                    InGameStateDataConstants.NETWORK_BUBBLE_RADIUS)
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

        private IEnumerator<Wait> OnAreaChange()
        {
            while (true)
            {
                yield return new Wait(RemoteEvents.AreaChangeDetected);
                if (this.Address != IntPtr.Zero)
                {
                    this.CleanUpData();
                }
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
