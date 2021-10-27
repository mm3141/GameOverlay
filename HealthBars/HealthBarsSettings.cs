// <copyright file="HealthBarsSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HealthBars
{
    using System.Numerics;
    using GameHelper.Plugin;

    /// <summary>
    /// <see cref="HealthBars"/> plugin settings class.
    /// </summary>
    public sealed class HealthBarsSettings : IPSettings
    {
#pragma warning disable SA1401
        /// <summary>
        /// Show bar in town.
        /// </summary>
        public bool ShowInTown = false;

        /// <summary>
        /// Show bar in hideout.
        /// </summary>
        public bool ShowInHideout = false;

        /// <summary>
        /// Show enemy mana.
        /// </summary>
        public bool ShowEnemyMana = false;

        /// <summary>
        /// Show friendly bars.
        /// </summary>
        public bool ShowFriendlyBars = false;

        /// <summary>
        /// Show rarity border around bar.
        /// </summary>
        public bool ShowRarityBorders = true;

        /// <summary>
        /// Normal monster color.
        /// </summary>
        public Vector4 NormalColor = new Vector4(255, 255, 255, 100) / 255f;

        /// <summary>
        /// Normal monster color.
        /// </summary>
        public Vector4 MagicColor = new Vector4(30, 30, 200, 240) / 255f;

        /// <summary>
        /// Normal monster color.
        /// </summary>
        public Vector4 RareColor = new Vector4(255, 225, 0, 240) / 255f;

        /// <summary>
        /// Normal monster color.
        /// </summary>
        public Vector4 UniqueColor = new Vector4(206, 42, 0, 240) / 255f;
    }
}
