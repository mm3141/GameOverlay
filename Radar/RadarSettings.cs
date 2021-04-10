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

    /// <summary>
    /// <see cref="Radar"/> plugin settings class.
    /// </summary>
    public sealed class RadarSettings : IPSettings
    {
#pragma warning disable SA1401
        /// <summary>
        /// Multipler to apply to the Large Map icons
        /// so they display correctly on the screen.
        /// </summary>
        public float LargeMapScaleMultiplier = 0.174f;

        /// <summary>
        /// Hides all the entities that doesn't have life or chest component.
        /// </summary>
        public bool HideUseless = true;

        /// <summary>
        /// Gets a value indicating whether user wants to modify large map culling window or not.
        /// </summary>
        public bool ModifyCullWindow = true;

        /// <summary>
        /// Gets the position of the cull window that the user wants.
        /// </summary>
        public Vector2 CullWindowPos = Vector2.Zero;

        /// <summary>
        /// Get the size of the cull window that the user wants.
        /// </summary>
        public Vector2 CullWindowSize = Vector2.Zero;

        /// <summary>
        /// Icons to display on the map. Base game includes normal chests, strongboxes, monsters etc.
        /// </summary>
        public Dictionary<string, IconPicker> BaseIcons = new Dictionary<string, IconPicker>();

        /// <summary>
        /// Icons to display on the map. Legion includes special legion monster chests.
        /// since they can't be covered by base icons.
        /// </summary>
        public Dictionary<string, IconPicker> LegionIcons = new Dictionary<string, IconPicker>();

        /// <summary>
        /// Icons to display on the map. Delirium includes the special spawners and bombs that
        /// delirium brings and they can't be convered by base icons.
        /// </summary>
        public Dictionary<string, IconPicker> DeliriumIcons = new Dictionary<string, IconPicker>();

        /// <summary>
        /// Icons to display on the map. Heist includes the special heist chests that can't
        /// be distinguished by base chests.
        /// </summary>
        public Dictionary<string, IconPicker> HeistIcons = new Dictionary<string, IconPicker>();

        /// <summary>
        /// Icons to display on the map. Delve includes the special delve chests which can't be
        /// distinguished by using base chests icons.
        /// </summary>
        public Dictionary<string, IconPicker> DelveIcons = new Dictionary<string, IconPicker>();
#pragma warning restore SA1401

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
            if (ImGui.TreeNode($"{headingText}##treeNode"))
            {
                if (!string.IsNullOrEmpty(helpingText))
                {
                    ImGui.Text(helpingText);
                }

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
            this.AddDefaultLegionIcons(basicIconPathName);
            this.AddDefaultDeliriumIcons(basicIconPathName);
            this.AddDefaultHeistIcons(basicIconPathName);

            var delveIconPathName = Path.Join(dllDirectory, "delve.png");
            this.AddDefaultDelveIcons(delveIconPathName);
        }

        private void AddDefaultBaseGameIcons(string iconPathName)
        {
            this.BaseIcons.TryAdd("Blockage OR DelveWall", new IconPicker(iconPathName, 14, 41, 1, 11, 40));

            this.BaseIcons.TryAdd("Chest", new IconPicker(iconPathName, 14, 41, 1, 13, 24));

            this.BaseIcons.TryAdd("Shrine", new IconPicker(iconPathName, 14, 41, 7, 0, 30));

            this.BaseIcons.TryAdd("Friendly", new IconPicker(iconPathName, 14, 41, 1, 0, 20));
            this.BaseIcons.TryAdd("Normal Monster", new IconPicker(iconPathName, 14, 41, 0, 14, 20));
            this.BaseIcons.TryAdd("Magic Monster", new IconPicker(iconPathName, 14, 41, 6, 3, 20));
            this.BaseIcons.TryAdd("Rare Monster", new IconPicker(iconPathName, 14, 41, 3, 14, 20));
            this.BaseIcons.TryAdd("Unique Monster", new IconPicker(iconPathName, 14, 41, 5, 14, 30));
        }

        private void AddDefaultLegionIcons(string iconPathName)
        {
            this.LegionIcons.TryAdd("Legion Monster Chest", new IconPicker(iconPathName, 14, 41, 1, 13, 50));
        }

        private void AddDefaultDeliriumIcons(string iconPathName)
        {
            this.DeliriumIcons.TryAdd("Delirium Bomb", new IconPicker(iconPathName, 14, 41, 5, 0, 30));
            this.DeliriumIcons.TryAdd("Delirium Spawner", new IconPicker(iconPathName, 14, 41, 6, 0, 30));
        }

        private void AddDefaultHeistIcons(string iconPathName)
        {
            this.HeistIcons.TryAdd("Heist Armour", new IconPicker(iconPathName, 14, 41, 1, 39, 30));
            this.HeistIcons.TryAdd("Heist Corrupted", new IconPicker(iconPathName, 14, 41, 7, 12, 30));
            this.HeistIcons.TryAdd("Heist Currency", new IconPicker(iconPathName, 14, 41, 10, 38, 30));
            this.HeistIcons.TryAdd("Heist DivinationCards", new IconPicker(iconPathName, 14, 41, 11, 39, 30));
            this.HeistIcons.TryAdd("Heist Essences", new IconPicker(iconPathName, 14, 41, 7, 39, 30));
            this.HeistIcons.TryAdd("Heist Gems", new IconPicker(iconPathName, 14, 41, 12, 38, 30));
            this.HeistIcons.TryAdd("Heist Jewellery", new IconPicker(iconPathName, 14, 41, 0, 39, 30));
            this.HeistIcons.TryAdd("Heist Jewels", new IconPicker(iconPathName, 14, 41, 0, 39, 30));
            this.HeistIcons.TryAdd("Heist Maps", new IconPicker(iconPathName, 14, 41, 13, 38, 30));
            this.HeistIcons.TryAdd("Heist Prophecies", new IconPicker(iconPathName, 14, 41, 10, 39, 30));
            this.HeistIcons.TryAdd("Heist QualityCurrency", new IconPicker(iconPathName, 14, 41, 9, 12, 30));
            this.HeistIcons.TryAdd("Heist StackedDecks", new IconPicker(iconPathName, 14, 41, 10, 12, 30));
            this.HeistIcons.TryAdd("Heist Uniques", new IconPicker(iconPathName, 14, 41, 11, 38, 30));
            this.HeistIcons.TryAdd("Heist Weapons", new IconPicker(iconPathName, 14, 41, 2, 39, 30));
        }

        private void AddDefaultDelveIcons(string iconPathName)
        {

        }
    }
}
