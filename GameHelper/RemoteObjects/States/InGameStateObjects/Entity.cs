// <copyright file="Entity.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects
{
    using System;
    using System.Collections.Generic;
    using GameHelper.RemoteObjects.Components;
    using GameOffsets.Objects.States.InGameState;

    /// <summary>
    /// Points to an Entity/Object in the game.
    /// </summary>
    public class Entity : RemoteObjectBase
    {
        private Dictionary<string, IntPtr> componentAddresses;
        private Dictionary<string, ComponentBase> componentCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="address">address of the Entity.</param>
        internal Entity(IntPtr address)
            : base(address)
        {
            this.UpdateAll();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// NOTE: Without providing an address, only invalid and empty entity is created.
        /// </summary>
        internal Entity()
            : base(IntPtr.Zero)
        {
            this.Id = 0x00;
            this.IsValid = false;
            this.Path = string.Empty;
            this.componentAddresses = new Dictionary<string, IntPtr>();
            this.componentCache = new Dictionary<string, ComponentBase>();
        }

        /// <summary>
        /// Gets the Path (e.g. Metadata/Character/int/int) assocaited to the entity.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the Id associated to the entity. This is unique per map/Area.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Gets or Sets a value indicating whether the entity
        /// exists in the game or not.
        /// </summary>
        internal bool IsValid { get; set; }

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
            where T : ComponentBase
        {
            var componenName = typeof(T).Name;
            if (this.componentCache.TryGetValue(componenName, out var comp))
            {
                component = (T)comp;
                return true;
            }

            if (this.componentAddresses.TryGetValue(componenName, out IntPtr compAddr))
            {
                component = (T)Activator.CreateInstance(typeof(T), compAddr);
                this.componentCache[componenName] = component;
                return true;
            }

            component = null;
            return false;
        }

        /// <summary>
        /// Reads and updates only the data that is suppose to change.
        /// </summary>
        /// <param name="address">address of this entity.</param>
        internal void Update(IntPtr address)
        {
            if (this.Address == address)
            {
                this.UpdateData();
            }
            else
            {
                this.Address = address;
                this.UpdateAll();
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            string message = "Entity can't be cleaned, they can only be updated/deleted.";
            throw new NotImplementedException(message);
        }

        /// <summary>
        /// Reads and updates only the data that is suppose to change in an entity.
        /// </summary>
        protected override void UpdateData()
        {
            var reader = Core.Process.Handle;
            EntityOffsets entityData = reader.ReadMemory<EntityOffsets>(this.Address);
            this.IsValid = entityData.IsValid == EntityOffsets.Valid;
            foreach (var kv in this.componentCache)
            {
                kv.Value.UpdateData();
            }
        }

        /// <summary>
        /// Reads and updates all the data related to the Entity.
        /// </summary>
        private void UpdateAll()
        {
            this.componentAddresses = new Dictionary<string, IntPtr>(20);
            this.componentCache = new Dictionary<string, ComponentBase>();
            var reader = Core.Process.Handle;
            EntityOffsets entityData = reader.ReadMemory<EntityOffsets>(this.Address);
            this.IsValid = entityData.IsValid == EntityOffsets.Valid;
            this.Id = entityData.Id;

            var entityComponentArray = reader.ReadStdVector<IntPtr>(
                entityData.ComponentListPtr);
            EntityDetails entityDetails = reader.ReadMemory<EntityDetails>(entityData.EntityDetailsPtr);
            this.Path = reader.ReadStdWString(entityDetails.name);

            var lookupPtr = reader.ReadMemory<ComponentLookUpStruct>(
                entityDetails.ComponentLookUpPtr);
            var nameAndIndex = reader.ReadStdList<ComponentNameAndIndexStruct>(
                lookupPtr.ComponentNameAndIndexPtr);

            for (int i = 0; i < nameAndIndex.Count; i++)
            {
                var data = nameAndIndex[i];
                string name = reader.ReadString(data.NamePtr);
                this.componentAddresses.Add(name, entityComponentArray[data.Index]);
            }
        }
    }
}
