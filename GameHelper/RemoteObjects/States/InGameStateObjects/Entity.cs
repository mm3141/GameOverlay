// <copyright file="Entity.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects
{
    using System;
    using System.Collections.Generic;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using GameOffsets.Objects.States.InGameState;
    using ImGuiNET;

    /// <summary>
    /// Points to an Entity/Object in the game.
    /// </summary>
    public class Entity : RemoteObjectBase
    {
        private Dictionary<string, IntPtr> componentAddresses =
            new Dictionary<string, IntPtr>();

        private Dictionary<string, RemoteObjectBase> componentCache =
            new Dictionary<string, RemoteObjectBase>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="address">address of the Entity.</param>
        internal Entity(IntPtr address)
            : base(address, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// NOTE: Without providing an address, only invalid and empty entity is created.
        /// </summary>
        internal Entity()
            : base(IntPtr.Zero, true)
        {
        }

        /// <summary>
        /// Gets the Path (e.g. Metadata/Character/int/int) assocaited to the entity.
        /// </summary>
        public string Path { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the Id associated to the entity. This is unique per map/Area.
        /// </summary>
        public uint Id { get; private set; } = 0x00;

        /// <summary>
        /// Gets or Sets a value indicating whether the entity
        /// exists in the game or not.
        /// </summary>
        internal bool IsValid { get; set; } = false;

        /// <summary>
        /// Calculate the distance from the other entity.
        /// </summary>
        /// <param name="other">Other entity object.</param>
        /// <returns>
        /// the distance from the other entity
        /// if it can calculate; otherwise, return 0.
        /// </returns>
        public int DistanceFrom(Entity other)
        {
            if (this.TryGetComponent<Positioned>(out var myPosComp) &&
                other.TryGetComponent<Positioned>(out var otherPosComp))
            {
                var dx = myPosComp.WorldPosition.X - otherPosComp.WorldPosition.X;
                var dy = myPosComp.WorldPosition.Y - otherPosComp.WorldPosition.Y;
                return (int)Math.Sqrt((dx * dx) + (dy * dy));
            }
            else
            {
                Console.WriteLine($"Position Component missing in {this.Path} or {other.Path}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the Component data associated with the entity.
        /// </summary>
        /// <typeparam name="T">Component type to get.</typeparam>
        /// <param name="component">component data.</param>
        /// <returns>true if the entity contains the component; otherwise, false.</returns>
        public bool TryGetComponent<T>(out T component)
            where T : RemoteObjectBase
        {
            var componenName = typeof(T).Name;
            if (this.componentCache.TryGetValue(componenName, out var comp))
            {
                component = (T)comp;
                return true;
            }

            if (this.componentAddresses.TryGetValue(componenName, out IntPtr compAddr))
            {
                if (compAddr != IntPtr.Zero)
                {
                    component = (T)Activator.CreateInstance(typeof(T), compAddr);
                    this.componentCache[componenName] = component;
                    return true;
                }
            }

            component = null;
            return false;
        }

        /// <summary>
        /// Converts the <see cref="Entity"/> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Path: {this.Path}");
            ImGui.Text($"Id: {this.Id}");
            ImGui.Text($"Is Valid: {this.IsValid}");
            if (ImGui.TreeNode($"Components"))
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
                        UiHelper.IntPtrToImGui(kv.Key, kv.Value);
                    }
                }

                ImGui.TreePop();
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            EntityOffsets entityData = reader.ReadMemory<EntityOffsets>(this.Address);
            this.IsValid = EntityHelper.IsValidEntity(entityData.IsValid);
            if (!this.IsValid)
            {
                // Invalid entity data is normally corrupted. let's not parse it.
                return;
            }

            this.Id = entityData.Id;
            if (hasAddressChanged)
            {
                this.componentAddresses.Clear();
                this.componentCache.Clear();
                var entityComponent = reader.ReadStdVector<IntPtr>(entityData.ComponentListPtr);
                var entityDetails = reader.ReadMemory<EntityDetails>(entityData.EntityDetailsPtr);
                this.Path = reader.ReadStdWString(entityDetails.name);
                var lookupPtr = reader.ReadMemory<ComponentLookUpStruct>(
                    entityDetails.ComponentLookUpPtr);
                var nameAndIndex = reader.ReadStdList<ComponentNameAndIndexStruct>(
                    lookupPtr.ComponentNameAndIndexPtr);

                for (int i = 0; i < nameAndIndex.Count; i++)
                {
                    var data = nameAndIndex[i];
                    string name = reader.ReadString(data.NamePtr);
                    if (string.IsNullOrEmpty(name) || this.componentAddresses.ContainsKey(name))
                    {
                        continue;
                    }

                    this.componentAddresses.Add(name, entityComponent[data.Index]);
                }

                this.TryGetComponent<Life>(out var _);
            }
            else
            {
                foreach (var kv in this.componentCache)
                {
                    kv.Value.Address = kv.Value.Address;
                }
            }
        }
    }
}
