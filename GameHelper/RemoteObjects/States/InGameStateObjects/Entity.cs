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
        /// <summary>
        /// Store the component name and addresses.
        /// </summary>
        private Dictionary<string, IntPtr> components;

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
            this.components = new Dictionary<string, IntPtr>();
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
        /// <returns>the distance from the other entity.</returns>
        public int DistanceFrom(Entity other)
        {
            return 0;
        }

        /// <summary>
        /// Gets the Component data associated with the template.
        /// </summary>
        /// <typeparam name="T">Component data type.</typeparam>
        /// <returns>Component data.</returns>
        public T GetComponent<T>()
            where T : ComponentBase
        {
            return default(T);
        }

        /// <summary>
        /// Gets a value indicating whether the component exists or not.
        /// </summary>
        /// <typeparam name="T">Component data type.</typeparam>
        /// <returns>true if the component exists otherwise false.</returns>
        public bool HasComponent<T>()
            where T : ComponentBase
        {
            return this.components.ContainsKey(typeof(T).Name);
        }

        /// <summary>
        /// Reads and updates only the data that is suppose to change.
        /// </summary>
        /// <param name="address">Is the entity in current entity list.</param>
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
        }

        /// <summary>
        /// Reads and updates all the data related to the Entity.
        /// </summary>
        private void UpdateAll()
        {
            this.components = new Dictionary<string, IntPtr>(20);
            var reader = Core.Process.Handle;
            EntityOffsets entityData = reader.ReadMemory<EntityOffsets>(this.Address);
            this.IsValid = entityData.IsValid == EntityOffsets.Valid;
            this.Id = entityData.Id;

            EntityDetails entityDetails = reader.ReadMemory<EntityDetails>(entityData.EntityDetailsPtr);
            this.Path = reader.ReadStdWString(entityDetails.name);

            var lookupPtr = reader.ReadMemory<ComponentLookUpStruct>(
                entityDetails.ComponentLookUpPtr);
            var nameAndIndex = reader.ReadStdList<ComponentNameAndIndexStruct>(
                lookupPtr.ComponentNameAndIndexPtr);
            var entityComponentArray = reader.ReadStdVector<IntPtr>(
                entityData.ComponentListPtr);

            for (int i = 0; i < nameAndIndex.Count; i++)
            {
                var data = nameAndIndex[i];
                string name = reader.ReadString(data.NamePtr);
                this.components.Add(name, entityComponentArray[data.Index]);
            }
        }
    }
}
