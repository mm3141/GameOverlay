// <copyright file="RadarSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Radar
{
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using GameHelper.Plugin;
    using ImGuiNET;
    using Newtonsoft.Json;
    using GameHelper.Utils;

    /// <summary>
    /// <see cref="Radar"/> plugin settings class.
    /// </summary>
    public sealed class RadarSettings : IPSettings
    {
        /// <summary>
        /// Multipler to apply to the Large Map icons
        /// so they display correctly on the screen.
        /// </summary>
        public float LargeMapScaleMultiplier = 0.1738f;

        /// <summary>
        /// Do not draw the Radar plugin stuff when game is in the background.
        /// </summary>
        public bool DrawWhenForeground = true;

        /// <summary>
        /// Do not draw the Radar plugin stuff when user is in hideout/town.
        /// </summary>
        public bool DrawWhenNotInHideoutOrTown = true;

        /// <summary>
        /// Hides all the entities that are outside the network bubble.
        /// </summary>
        public bool HideOutsideNetworkBubble = false;

        /// <summary>
        /// Gets a value indicating whether user wants to modify large map culling window or not.
        /// </summary>
        public bool ModifyCullWindow = false;

        /// <summary>
        /// Gets a value indicating whether user wants culling window
        /// to cover the full game or not.
        /// </summary>
        public bool MakeCullWindowFullScreen = true;

        /// <summary>
        /// Gets a value indicating whether to draw the map in culling window or not.
        /// </summary>
        public bool DrawMapInCull = true;

        /// <summary>
        /// Gets a value indicating whether to draw the POI in culling window or not.
        /// </summary>
        public bool DrawPOIInCull = true;

        /// <summary>
        /// Gets a value indicating whether user wants to draw walkable map or not.
        /// </summary>
        public bool DrawWalkableMap = true;

        /// <summary>
        /// Gets a value indicating what color to use for drawing walkable map.
        /// </summary>
        public Vector4 WalkableMapColor = new Vector4(150f) / 255f;

        /// <summary>
        /// Gets the position of the cull window that the user wants.
        /// </summary>
        public Vector2 CullWindowPos = Vector2.Zero;

        /// <summary>
        /// Get the size of the cull window that the user wants.
        /// </summary>
        public Vector2 CullWindowSize = Vector2.Zero;

        /// <summary>
        /// Gets a value indicating wether user wants to show Player icon or names.
        /// </summary>
        public bool ShowPlayersNames = false;

        /// <summary>
        /// Gets a value indicating what is the maximum frequency a POI should have
        /// </summary>
        public int POIFrequencyFilter = 0;

        /// <summary>
        /// Gets a value indicating wether user want to show important tgt names or not.
        /// </summary>
        public bool ShowImportantPOI = true;

        /// <summary>
        /// Gets a value indicating what color to use for drawing the POI.
        /// </summary>
        public Vector4 POIColor = new(1f, 0.5f, 0.5f, 1f);

        /// <summary>
        /// Gets a value indicating wether user want to draw a background when drawing the POI.
        /// </summary>
        public bool EnablePOIBackground = true;

        /// <summary>
        /// Gets the Tgts and their expected clusters per area/zone/map.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, Dictionary<string, string>> ImportantTgts = new();

        /// <summary>
        /// Icons to display on the map. Base game includes normal chests, strongboxes, monsters etc.
        /// </summary>
        public Dictionary<string, IconPicker> BaseIcons = new();

        /// <summary>
        /// Icons to display on the map. Breach includes breach chests.
        /// </summary>
        public Dictionary<string, IconPicker> BreachIcons = new();

        /// <summary>
        /// Icons to display on the map. Legion includes special legion monster chests.
        /// since they can't be covered by base icons.
        /// </summary>
        public Dictionary<string, IconPicker> LegionIcons = new();

        /// <summary>
        /// Icons to display on the map. Delirium includes the special spawners and bombs that
        /// delirium brings and they can't be convered by base icons.
        /// </summary>
        public Dictionary<string, IconPicker> DeliriumIcons = new();

        /// <summary>
        /// Icons to display on the map. Delirium includes the special spawners and bombs that
        /// delirium brings and they can't be convered by base icons.
        /// </summary>
        public Dictionary<string, IconPicker> ExpeditionIcons = new();

        /// <summary>
        /// Icons to display on the map. Heist includes the special heist chests that can't
        /// be distinguished by base chests.
        /// </summary>
        public Dictionary<string, IconPicker> HeistIcons = new();

        /// <summary>
        /// Icons to display on the map. Delve includes the special delve chests which can't be
        /// distinguished by using base chests icons.
        /// </summary>
        public Dictionary<string, IconPicker> DelveIcons = new();

        private static readonly int IconsPngRows = 44;
        private static readonly int IconsPngCols = 14;
        private static readonly int DelvePngRows = 44;
        private static readonly int DelvePngCols = 14;

        /// <summary>
        /// Draws the icons setting via the ImGui widgets.
        /// </summary>
        /// <param name="headingText">Text to display as heading.</param>
        /// <param name="icons">Icons settings to draw.</param>
        /// <param name="helpingText">helping text to display at the top.</param>
        public void DrawIconsSettingToImGui(
            string headingText,
            Dictionary<string, IconPicker> icons,
            string helpingText)
        {
            var isOpened = ImGui.TreeNode($"{headingText}##treeNode");
            if (!string.IsNullOrEmpty(helpingText))
            {
                ImGuiHelper.ToolTip(helpingText);
            }

            if (isOpened)
            {
                ImGui.Columns(2, $"icons columns##{headingText}", false);
                foreach (var icon in icons)
                {
                    ImGui.Text(icon.Key);
                    ImGui.NextColumn();
                    icon.Value.ShowSettingWidget();
                    ImGui.NextColumn();
                }

                ImGui.Columns(1);
                ImGui.TreePop();
            }
        }

        /// <summary>
        /// Adds the default icons if the setting file isn't available.
        /// </summary>
        /// <param name="dllDirectory">directory where the plugin dll is located.</param>
        public void AddDefaultIcons(string dllDirectory)
        {
            var basicIconPathName = Path.Join(dllDirectory, "icons.png");
            this.AddDefaultBaseGameIcons(basicIconPathName);
            this.AddDefaultBreachIcons(basicIconPathName);
            this.AddDefaultLegionIcons(basicIconPathName);
            this.AddDefaultDeliriumIcons(basicIconPathName);
            this.AddDefaultHeistIcons(basicIconPathName);
            this.AddDefaultDelveIcons(basicIconPathName);
            this.AddDefaultExpeditionIcons(basicIconPathName);
        }

        private void AddDefaultBaseGameIcons(string iconPathName)
        {
            this.BaseIcons.TryAdd("Player", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 2, 0, 20));
            this.BaseIcons.TryAdd("Leader", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 3, 0, 20));
            this.BaseIcons.TryAdd("Strongbox", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 8, 38, 30));
            this.BaseIcons.TryAdd("Important Strongboxes", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 6, 41, 35));
            this.BaseIcons.TryAdd("Chests With Label", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 1, 13, 20));
            this.BaseIcons.TryAdd("Chests Without Label", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 6, 9, 20));

            this.BaseIcons.TryAdd("Shrine", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 7, 0, 30));

            this.BaseIcons.TryAdd("Friendly", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 1, 0, 20));
            this.BaseIcons.TryAdd("Normal Monster", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 0, 14, 20));
            this.BaseIcons.TryAdd("Magic Monster", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 6, 3, 20));
            this.BaseIcons.TryAdd("Rare Monster", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 5, 4, 20));
            this.BaseIcons.TryAdd("Unique Monster", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 5, 14, 30));
        }

        private void AddDefaultLegionIcons(string iconPathName)
        {
            this.LegionIcons.TryAdd("Legion Reward Monster/Chest", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 4, 41, 30));
            this.LegionIcons.TryAdd("Legion Epic Chest", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 6, 41, 30));
        }

        private void AddDefaultBreachIcons(string iconPathName)
        {
            this.BreachIcons.TryAdd("Breach Chest", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 6, 41, 30));
        }

        private void AddDefaultDeliriumIcons(string iconPathName)
        {
            this.DeliriumIcons.TryAdd("Delirium Bomb", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 5, 0, 30));
            this.DeliriumIcons.TryAdd("Delirium Spawner", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 6, 0, 30));
        }

        private void AddDefaultExpeditionIcons(string iconPathName)
        {
            this.ExpeditionIcons.TryAdd("Generic Expedition Chests", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 5, 41, 30));
        }

        private void AddDefaultHeistIcons(string iconPathName)
        {
            this.HeistIcons.TryAdd("Heist Armour", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 1, 39, 30));
            this.HeistIcons.TryAdd("Heist Corrupted", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 7, 12, 30));
            this.HeistIcons.TryAdd("Heist Currency", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 10, 38, 30));
            this.HeistIcons.TryAdd("Heist DivinationCards", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 11, 39, 30));
            this.HeistIcons.TryAdd("Heist Essences", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 7, 39, 30));
            this.HeistIcons.TryAdd("Heist Gems", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 12, 38, 30));
            this.HeistIcons.TryAdd("Heist Jewellery", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 0, 39, 30));
            this.HeistIcons.TryAdd("Heist Jewels", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 0, 39, 30));
            this.HeistIcons.TryAdd("Heist Maps", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 13, 38, 30));
            this.HeistIcons.TryAdd("Heist Prophecies", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 10, 39, 30));
            this.HeistIcons.TryAdd("Heist QualityCurrency", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 9, 12, 30));
            this.HeistIcons.TryAdd("Heist StackedDecks", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 10, 12, 30));
            this.HeistIcons.TryAdd("Heist Uniques", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 11, 38, 30));
            this.HeistIcons.TryAdd("Heist Weapons", new IconPicker(iconPathName, IconsPngCols, IconsPngRows, 2, 39, 30));
        }

        private void AddDefaultDelveIcons(string iconPathName)
        {
            this.DelveIcons.TryAdd("Blockage OR DelveWall", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 1, 11, 30));
            this.DelveIcons.TryAdd("AberrantFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("AethericFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("AethericFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("BloodstainedFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("BoundFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("BoundFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("ClothMajorSpider", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("CorrodedFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("CorrodedFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveAzuriteChest1_1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 7, 18, 30));
            this.DelveIcons.TryAdd("DelveAzuriteChest1_2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 7, 18, 30));
            this.DelveIcons.TryAdd("DelveAzuriteChest1_3", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 5, 18, 60));
            this.DelveIcons.TryAdd("DelveAzuriteChest2_1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 5, 18, 60));
            this.DelveIcons.TryAdd("DelveAzuriteVein1_1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 7, 18, 30));
            this.DelveIcons.TryAdd("DelveAzuriteVein1_2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 7, 18, 30));
            this.DelveIcons.TryAdd("DelveAzuriteVein1_3", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 5, 18, 60));
            this.DelveIcons.TryAdd("DelveAzuriteVein2_1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 5, 18, 60));
            this.DelveIcons.TryAdd("DelveChestArmour1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmour2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmour3", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmour6LinkedUniqueBody", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourAtlas", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourBody2AdditionalSockets", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourCorrupted", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourDivination", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourElder", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourExtraQuality", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourFullyLinkedBody", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourLife", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourMovementSpeed", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourMultipleResists", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourMultipleUnique", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourOfCrafting", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourShaper", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourUnique", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestAssortedFossils", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrency1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrency2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrency3", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrencyDivination", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrencyHighShards", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrencyMaps", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrencyMaps2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrencySilverCoins", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrencySockets", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrencySockets2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrencyVaal", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrencyWisdomScrolls", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGem1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGem2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGem3", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGemGCP", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGemHighLevel", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGemHighLevelQuality", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGemHighQuality", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGemLevel4Special", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGeneric1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGeneric2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGeneric3", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericAdditionalUnique", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericAdditionalUniques", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericAtziriFragment", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericCurrency", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericDelveUnique", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericDivination", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericElderItem", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueAbyss", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueAmbushInvasion", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueAnarchy", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueBestiary", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueBeyond", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueBloodlines", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueBreach", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueDomination", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueHarbinger", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueIncursion", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueLegion", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueNemesis", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueOnslaught", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueRampage", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueTalisman", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueTempest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueTorment", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericLeagueWarbands", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericOffering", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericPaleCourtFragment", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericProphecyItem", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericRandomEnchant", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericRandomEssence", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericShaperItem", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericTrinkets", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestMap1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestMap2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestMap3", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestMapChisels", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestMapCorrupted", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestMapElderFragment", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestMapHorizon", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestMapSextants", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestMapShaped", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestMapShaperFragment", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestMapUnique", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialArmourAspect", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialArmourChaos", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialArmourCold", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialArmourFire", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialArmourLightning", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialArmourMana", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialArmourMinion", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialArmourPhysical", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialGenericChaos", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialGenericCold", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialGenericEssence", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialGenericFire", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialGenericLightning", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialGenericMana", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialGenericMinion", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialGenericPhysical", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialTrinketsAbyss", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialTrinketsAbyss2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialTrinketsAspect", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialTrinketsChaos", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialTrinketsCold", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialTrinketsFire", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialTrinketsLightning", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialTrinketsMana", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialTrinketsMinion", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialTrinketsPhysical", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialTrinketsTalisman", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialUniqueChaos", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialUniqueCold", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialUniqueFire", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialUniqueLightning", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialUniqueMana", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialUniqueMinion", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialUniquePhysical", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialWeaponChaos", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialWeaponCold", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialWeaponFire", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialWeaponLightning", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestSpecialWeaponPhysical", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinkets1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinkets2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinkets3", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsAmulet", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsAtlas", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsCorrupted", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsDivination", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsElder", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsEyesOfTheGreatwolf", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsJewel", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsMultipleUnique", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsOfCrafting", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsRing", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsShaper", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsUnique", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeapon1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeapon2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeapon3", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeapon30QualityUnique", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeapon6LinkedTwoHanded", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponCannotRollAttacker", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponCannotRollCaster", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponCaster", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponCorrupted", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponDivination", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponElder", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponExtraQuality", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponMultipleUnique", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponPhysicalDamage", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponShaper", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponUnique", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveMiningSupplies1_1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 11, 60));
            this.DelveIcons.TryAdd("DelveMiningSupplies1_2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 11, 60));
            this.DelveIcons.TryAdd("DelveMiningSupplies2_1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 11, 60));
            this.DelveIcons.TryAdd("DelveMiningSuppliesDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 11, 60));
            this.DelveIcons.TryAdd("DelveMiningSuppliesDynamite2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 11, 60));
            this.DelveIcons.TryAdd("DelveMiningSuppliesFlares1_1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 11, 60));
            this.DelveIcons.TryAdd("DelveMiningSuppliesFlares1_2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 11, 60));
            this.DelveIcons.TryAdd("DenseFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DynamiteArmour", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DynamiteCurrency", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DynamiteCurrency2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DynamiteCurrency3", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DynamiteGeneric", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DynamiteTrinkets", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DynamiteWeapon", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("EnchantedFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("EnchantedFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("EncrustedFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("FacetedFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("FracturedFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("FrigidFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("GildedFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("GlyphicFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("HollowFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("JaggedFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("LucentFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("MetallicFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("OffPathArmour", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("OffPathCurrency", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("OffPathGeneric", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("OffPathTrinkets", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("OffPathWeapon", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("PathArmour", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("PathCurrency", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("PathGeneric", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("PathTrinkets", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("PathWeapon", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("PerfectFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("PerfectFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("PrismaticFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("PrismaticFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("PristineFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("Resonator1", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("Resonator2", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("Resonator3", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("SanctifiedFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("ScorchedFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("SerratedFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("SerratedFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("ShudderingFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("TangledFossilChest", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("AberrantFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DenseFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("EncrustedFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("FrigidFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("JaggedFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("LucentFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("MetallicFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("PristineFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("ScorchedFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("ShudderingFossilChestDynamite", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmour30QualityUnique", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourConqueror", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourFractured", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestArmourSynthesised", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestCurrencyHigh", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestGenericScarab", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsConqueror", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsFractured", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsJewelCluster", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsJewelCorrupted", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsQuality", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestTrinketsSynthesised", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponConqueror", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponFractured", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
            this.DelveIcons.TryAdd("DelveChestWeaponSynthesised", new IconPicker(iconPathName, DelvePngCols, DelvePngRows, 0, 0, 30));
        }
    }
}
