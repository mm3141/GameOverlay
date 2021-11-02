// <copyright file="HealthBarsSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HealthBars
{
    using System.Numerics;
    using GameHelper.Plugin;

    /// <summary>
    ///     <see cref="HealthBars" /> plugin settings class.
    /// </summary>
    public sealed class HealthBarsSettings : IPSettings
    {
        /// <summary>
        ///     Culling range value.
        /// </summary>
        public int CullingRange = 10;

        /// <summary>
        ///     Cull range color.
        /// </summary>
        public Vector4 CullRangeColor = new Vector4(200, 200, 200, 200) / 255f;

        /// <summary>
        ///     Player bar render scale.
        /// </summary>
        public float FriendlyBarScale = 1f;

        /// <summary>
        ///     Normal monster bar render scale.
        /// </summary>
        public float MagicBarScale = 1f;

        /// <summary>
        ///     Normal monster color.
        /// </summary>
        public Vector4 MagicColor = new Vector4(30, 30, 200, 240) / 255f;

        /// <summary>
        ///     Normal monster bar render scale.
        /// </summary>
        public float NormalBarScale = 1f;

        /// <summary>
        ///     Normal monster color.
        /// </summary>
        public Vector4 NormalColor = new Vector4(255, 255, 255, 100) / 255f;

        /// <summary>
        ///     Player bar render scale.
        /// </summary>
        public float PlayerBarScale = 1f;

        /// <summary>
        ///     Normal monster bar render scale.
        /// </summary>
        public float RareBarScale = 1f;

        /// <summary>
        ///     Normal monster color.
        /// </summary>
        public Vector4 RareColor = new Vector4(255, 225, 0, 240) / 255f;

        /// <summary>
        ///     Show friendly gradation marks.
        /// </summary>
        public bool ShowCullRange = false;

        /// <summary>
        ///     Show enemy gradation marks.
        /// </summary>
        public bool ShowEnemyGradationMarks = false;

        /// <summary>
        ///     Show enemy mana.
        /// </summary>
        public bool ShowEnemyMana = false;

        /// <summary>
        ///     Show friendly bars.
        /// </summary>
        public bool ShowFriendlyBars = false;

        /// <summary>
        ///     Show friendly gradation marks.
        /// </summary>
        public bool ShowFriendlyGradationMarks = false;

        /// <summary>
        ///     Show bar in hideout.
        /// </summary>
        public bool ShowInHideout = false;

        /// <summary>
        ///     Show bar in town.
        /// </summary>
        public bool ShowInTown = false;

        /// <summary>
        ///     Show magic monster bar.
        /// </summary>
        public bool ShowMagicBar = true;

        /// <summary>
        ///     Show rarity border around magic monster bar.
        /// </summary>
        public bool ShowMagicBorders = true;

        /// <summary>
        ///     Show culling range for magic monsters.
        /// </summary>
        public bool ShowMagicCull = false;

        /// <summary>
        ///     Show normal monster bar.
        /// </summary>
        public bool ShowNormalBar = true;

        /// <summary>
        ///     Show rarity border around normal monster bar.
        /// </summary>
        public bool ShowNormalBorders = true;

        /// <summary>
        ///     Show culling range for normal monsters.
        /// </summary>
        public bool ShowNormalCull = false;

        /// <summary>
        ///     Show player bars.
        /// </summary>
        public bool ShowPlayerBars = true;

        /// <summary>
        ///     Show rare monster bar.
        /// </summary>
        public bool ShowRareBar = true;

        /// <summary>
        ///     Show rarity border around rare monster bar.
        /// </summary>
        public bool ShowRareBorders = true;

        /// <summary>
        ///     Show culling range for rare monsters.
        /// </summary>
        public bool ShowRareCull = false;

        /// <summary>
        ///     Show rarity border around bar.
        /// </summary>
        public bool ShowRarityBorders = true;

        /// <summary>
        ///     Show unique monster bar.
        /// </summary>
        public bool ShowUniqueBar = true;

        /// <summary>
        ///     Show rarity border around unique monster bar.
        /// </summary>
        public bool ShowUniqueBorders = true;

        /// <summary>
        ///     Show culling range for unique monsters.
        /// </summary>
        public bool ShowUniqueCull = true;

        /// <summary>
        ///     Normal monster bar render scale.
        /// </summary>
        public float UniqueBarScale = 1f;

        /// <summary>
        ///     Normal monster color.
        /// </summary>
        public Vector4 UniqueColor = new Vector4(206, 42, 0, 240) / 255f;
    }
}