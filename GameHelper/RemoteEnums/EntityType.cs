// <copyright file="EntityType.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteEnums
{
    /// <summary>
    ///     Enum for entity Categorization system.
    /// </summary>
    public enum EntityTypes
    {
        /// <summary>
        ///     Unknown entity type. should only be used for init purposes.
        /// </summary>
        Unidentified,

        /// <summary>
        ///     A special entity type that will make sure that this entity
        ///     components are never updated again from the game memory.
        ///     This is to save the CPU cycles on such entity. All plugins
        ///     are expected to skip this entity.
        ///
        ///     WARNING: if an entity reaches this type it can never go back to not-useless
        ///              unless current area/zone changes.
        /// </summary>
        Useless,

        /// <summary>
        ///     Contains the Chest component.
        /// </summary>
        Player,

        /// <summary>
        ///     Contains the TriggerableBlockage component.
        /// </summary>
        Blockage,

        /// <summary>
        ///     Contains the Chest component.
        /// </summary>
        Chest,

        /// <summary>
        ///     Contains the Shrine component.
        /// </summary>
        Shrine,

        /// <summary>
        ///     Contains the Npc component.
        /// </summary>
        Npc,

        /// <summary>
        ///     Contains life component.
        /// </summary>
        Monster,

        /// <summary>
        ///     Contains WorldItem component.
        /// </summary>
        WorldItem,

        /// <summary>
        ///     Entity of type <see cref="RemoteObjects.States.InGameStateObjects.Item"/> class.
        /// </summary>
        InventoryItem,
    }
}
