// <copyright file="InventoryName.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteEnums
{
    /// <summary>
    ///     Read Inventories.dat file for inventory name and index mapping.
    ///     Remember, in the game, inventory index starts from 0x01.
    ///     In the Inventories.dat file, inventory index starts from 0x00.
    /// </summary>
    public enum InventoryName
    {
#pragma warning disable CS1591, SA1602
        NoInvSelected,
        MainInventory1,
        BodyArmour1,
        Weapon1,
        Offhand1,
        Helm1,
        Amulet1,
        Ring1,
        Ring2,
        Gloves1,
        Boots1,
        Belt1,
        Flask1,
        Cursor1,
        Map1,
        Weapon2,
        Offhand2,
        StrMasterCrafting,
        StrDexMasterCrafting,
        DexMasterCrafting,
        DexIntMasterCrafting,
        IntMasterCrafting,
        StrIntMasterCrafting,
        PVPMasterCrafting,
        PassiveJewels1,
        AnimatedArmour1,
        GuildTag1,
        StashInventoryId,
        DivinationCardTrade,
        Darkshrine,
        TalismanTrade,
        Leaguestone1,
        BestiaryCrafting,
        IncursionSacrifice,
        BetrayalUnveiling,
        ItemSynthesisInput,
        ItemSynthesisOutput,
        BlightCraftingInput,
        BlightCraftingItem,
        MetamorphosisGenomeInput,
        AtlasUpgrades1,
        AtlasUpgradesStorage,
        ExpeditionMapMission,
        ExpeditionDeal1,
        HarvestCraftingItem,
        HeistContractMission,
        HeistNpcEquipment1,
        HeistNpcEquipment2,
        HeistNpcEquipment3,
        HeistNpcEquipment4,
        HeistNpcEquipment5,
        HeistNpcEquipment6,
        HeistNpcEquipment7,
        HeistNpcEquipment8,
        HeistNpcEquipment9,
        Trinket1,
        HeistBlueprintMission,
        HeistStorage1,
        RitualSavedRewards1,
        DONOTUSE1,
        DONOTUSE2,
        DONOTUSE3,
        DONOTUSE4,
        RESERVED1,
        RESERVED2,
        RESERVED3,
        RESERVED4,
        UltimatumCraftingItem,
        RESERVED5,
        RESERVED6,
        ExpeditionStorage1,
        HellscapeModificationInventory1,
        LabyrinthCraftingInput,
        SentinelDroneInventory1,
        SentinelStorage1,
        LakeTabletInventory1,
        MemoryLineMaps
#pragma warning restore CS1591, SA1602
    }
}