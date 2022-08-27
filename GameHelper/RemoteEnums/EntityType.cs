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
        ///     Unknown entity type i.e. entity isn't categorized yet.
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
        ///     Contains the Player component and player ID is the same as user player id.
        /// </summary>
        SelfPlayer,

        /// <summary>
        ///     Contains the Player component and player ID isn't the same as user player id..
        /// </summary>
        OtherPlayer,

        /// <summary>
        ///     Contains the TriggerableBlockage component.
        /// </summary>
        Blockage,

        /// <summary>
        ///     Contains the Chest component.
        /// </summary>
        Chest,

        /// <summary>
        ///     Contains the Chest component and display some label on the chest.
        /// </summary>
        ChestWithLabels,

        /// <summary>
        ///     Contains the Chest component and found in the delve area.
        /// </summary>
        DelveChest,

        /// <summary>
        ///     Contains the Chest component and found in the heist area.
        /// </summary>
        HeistChest,

        /// <summary>
        ///      Contains the Chest component and found in Expedition area.
        /// </summary>
        ExpeditionChest,

        /// <summary>
        ///     Contains the Chest component and is a strongbox.
        /// </summary>
        ImportantStrongboxChest,

        /// <summary>
        ///     Contains the Chest component and is a strongbox.
        /// </summary>
        StrongboxChest,

        /// <summary>
        ///     Contains the Chest component and found in a breach.
        /// </summary>
        BreachChest,

        /// <summary>
        ///     Contains the Shrine component.
        /// </summary>
        Shrine,

        /// <summary>
        ///     Contains life and position component and is friendly.
        /// </summary>
        FriendlyMonster,

        /// <summary>
        ///     Contains life and ObjectMagicProperties component.
        /// </summary>
        Monster,

        /// <summary>
        ///     Important legion monster or chest when legion isn't opened.
        /// </summary>
        Stage0RewardFIT,

        /// <summary>
        ///     Legion epic chest when legion isn't opened.
        /// </summary>
        Stage0EChestFIT,

        /// <summary>
        ///     Regular legion monster when legion isn't opened.
        /// </summary>
        Stage0FIT,

        /// <summary>
        ///     Important legion monster or chest when legion is opened.
        /// </summary>
        Stage1RewardFIT,

        /// <summary>
        ///     Legion epic chest when legion is opened.
        /// </summary>
        Stage1EChestFIT,

        /// <summary>
        ///     Regular legion monster after legion is opened.
        /// </summary>
        Stage1FIT,

        /// <summary>
        ///     Legion monster after legion is opened and killed by user.
        /// </summary>
        Stage1DeadFIT,

        /// <summary>
        ///     Delirium monster that explode when player steps on it.
        /// </summary>
        DeliriumBomb,

        /// <summary>
        ///     Delirium monster that creates new monster when player steps on it.
        /// </summary>
        DeliriumSpawner,

        /// <summary>
        ///     Entity of type <see cref="RemoteObjects.States.InGameStateObjects.Item"/> class.
        /// </summary>
        InventoryItem,
    }
}
