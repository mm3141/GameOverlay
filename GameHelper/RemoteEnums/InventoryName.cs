// <copyright file="InventoryName.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteEnums
{
    /// <summary>
    /// Read Inventories.dat file for inventory name and index mapping.
    /// Remember, in the game, inventory index starts from 0x01.
    /// In the Inventories.dat file, inventory index starts from 0x00.
    /// </summary>
    public enum InventoryName
    {
#pragma warning disable SA1602, CS1591 // Got it from the game, no need to document it.
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
        AtlasRegionUpgrades1,
        AtlasRegionUpgradesStorage,
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
        MapCrusader,
        MapEyrie,
        MapBasilisk,
        MapAdjudicator,
        RESERVED1,
        RESERVED2,
        RESERVED3,
        RESERVED4,
        UltimatumCraftingItem,
        RESERVED5,
        RESERVED6,
        ExpeditionStorage1,
#pragma warning restore SA1602, CS1591
    }
}
