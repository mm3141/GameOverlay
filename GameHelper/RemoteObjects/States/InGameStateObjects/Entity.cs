// <copyright file="Entity.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects
{
    using System;
    using System.Collections.Generic;
    using Components;
    using GameOffsets.Objects.States.InGameState;
    using ImGuiNET;
    using Utils;

    /// <summary>
    ///     Points to an Entity/Object in the game.
    ///     Entity is basically item/monster/effect/player/etc on the ground.
    /// </summary>
    public class Entity : RemoteObjectBase
    {
        private readonly Dictionary<string, IntPtr> componentAddresses;
        private readonly Dictionary<string, RemoteObjectBase> componentCache;
        private bool isnearby;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Entity" /> class.
        /// </summary>
        /// <param name="address">address of the Entity.</param>
        internal Entity(IntPtr address)
            : this()
        {
            this.Address = address;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Entity" /> class.
        ///     NOTE: Without providing an address, only invalid and empty entity is created.
        /// </summary>
        internal Entity()
            : base(IntPtr.Zero, true)
        {
            this.componentAddresses = new();
            this.componentCache = new();
            this.isnearby = false;
            this.Path = string.Empty;
        }

        /// <summary>
        ///     Gets the Path (e.g. Metadata/Character/int/int) assocaited to the entity.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        ///     Gets the Id associated to the entity. This is unique per map/Area.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the entity is nearby the player or not.
        /// </summary>
        public bool IsNearby => this.IsValid && this.isnearby;

        /// <summary>
        ///     Gets or Sets a value indicating whether the entity
        ///     exists in the game or not.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        ///     Calculate the distance from the other entity.
        /// </summary>
        /// <param name="other">Other entity object.</param>
        /// <returns>
        ///     the distance from the other entity
        ///     if it can calculate; otherwise, return 0.
        /// </returns>
        public int DistanceFrom(Entity other)
        {
            if (this.TryGetComponent<Render>(out var myPosComp) &&
                other.TryGetComponent<Render>(out var otherPosComp))
            {
                var dx = myPosComp.GridPosition.X - otherPosComp.GridPosition.X;
                var dy = myPosComp.GridPosition.Y - otherPosComp.GridPosition.Y;
                return (int)Math.Sqrt(dx * dx + dy * dy);
            }

            Console.WriteLine($"Position Component missing in {this.Path} or {other.Path}");
            return 0;
        }

        /// <summary>
        ///     Gets the Component data associated with the entity.
        /// </summary>
        /// <typeparam name="T">Component type to get.</typeparam>
        /// <param name="component">component data.</param>
        /// <returns>true if the entity contains the component; otherwise, false.</returns>
        public bool TryGetComponent<T>(out T component)
            where T : RemoteObjectBase
        {
            component = null;
            var componenName = typeof(T).Name;
            if (this.componentCache.TryGetValue(componenName, out var comp))
            {
                component = (T)comp;
                return true;
            }

            if (this.componentAddresses.TryGetValue(componenName, out var compAddr))
            {
                if (compAddr != IntPtr.Zero)
                {
                    component = (T)Activator.CreateInstance(typeof(T), compAddr);
                    this.componentCache[componenName] = component;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Calling this function will make sure entity isn't deleted
        ///     in the very next frame even if GameHelper consider it invalid.
        /// </summary>
        public void ForceKeepEntity()
        {
            this.IsValid = true;
        }

        /// <summary>
        ///     Converts the <see cref="Entity" /> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Path: {this.Path}");
            ImGui.Text($"Id: {this.Id}");
            ImGui.Text($"Is Valid: {this.IsValid}");
            if (ImGui.TreeNode("Components"))
            {
                foreach (var kv in this.componentAddresses)
                {
                    if (this.componentCache.ContainsKey(kv.Key))
                    {
                        if (ImGui.TreeNode($"{kv.Key}"))
                        {
                            this.componentCache[kv.Key].ToImGui();
                            ImGui.TreePop();
                        }
                    }
                    else
                    {
                        ImGuiHelper.IntPtrToImGui(kv.Key, kv.Value);
                    }
                }

                ImGui.TreePop();
            }
        }

        internal void UpdateNearby(Entity player)
        {
            if (this.DistanceFrom(player) < Core.GHSettings.NearbyMeaning)
            {
                this.isnearby = true;
            }
            else
            {
                this.isnearby = false;
            }
        }

        /// <summary>
        ///     Updates the component data associated with the Entity base object (i.e. item).
        /// </summary>
        /// <param name="idata">Entity base (i.e. item) data.</param>
        /// <param name="hasAddressChanged">has this class Address changed or not.</param>
        protected void UpdateComponentData(ItemStruct idata, bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            if (hasAddressChanged)
            {
                this.componentAddresses.Clear();
                this.componentCache.Clear();
                var entityComponent = reader.ReadStdVector<IntPtr>(idata.ComponentListPtr);
                var entityDetails = reader.ReadMemory<EntityDetails>(idata.EntityDetailsPtr);
                this.Path = reader.ReadStdWString(entityDetails.name);
                var lookupPtr = reader.ReadMemory<ComponentLookUpStruct>(
                    entityDetails.ComponentLookUpPtr);

                var namesAndIndexes = reader.ReadStdBucket<ComponentNameAndIndexStruct>(lookupPtr.ComponentsNameAndIndex);
                for (var i = 0; i < namesAndIndexes.Count; i++)
                {
                    var nameAndIndex = namesAndIndexes[i];
                    var name = reader.ReadString(nameAndIndex.NamePtr);
                    if (!(string.IsNullOrEmpty(name) || this.componentAddresses.ContainsKey(name)))
                    {
                        this.componentAddresses.TryAdd(name, entityComponent[nameAndIndex.Index]);
                    }
                }
            }
            else
            {
                foreach (var kv in this.componentCache)
                {
                    kv.Value.Address = kv.Value.Address;
                }
            }
        }

        /// <inheritdoc />
        protected override void CleanUpData()
        {
            this.componentAddresses?.Clear();
            this.componentCache?.Clear();
        }

        /// <inheritdoc />
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var entityData = reader.ReadMemory<EntityOffsets>(this.Address);
            this.IsValid = EntityHelper.IsValidEntity(entityData.IsValid);
            if (!this.IsValid)
            {
                // Invalid entity data is normally corrupted. let's not parse it.
                return;
            }

            this.Id = entityData.Id;
            this.UpdateComponentData(entityData.ItemBase, hasAddressChanged);
        }
    }
}