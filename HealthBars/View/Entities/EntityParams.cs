namespace HealthBars.View.Entities
{
    using System.Numerics;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.RemoteObjects.States.InGameStateObjects;
    using GameHelper.Utils;

    /// <summary>
    /// </summary>
    public class EntityParams
    {
        /// <summary>
        /// </summary>
        public readonly float EsPercent;

        /// <summary>
        /// </summary>
        public readonly float EsTotal;

        /// <summary>
        /// </summary>
        public readonly bool HasMagicProperties;

        /// <summary>
        /// </summary>
        public readonly float HpPercent;

        /// <summary>
        /// </summary>
        public readonly float HpReserved;

        /// <summary>
        /// </summary>
        public readonly float ManaPercent;

        /// <summary>
        /// </summary>
        public readonly float ManaReserved;

        /// <summary>
        /// </summary>
        public readonly Vector2 Pos;

        /// <summary>
        /// </summary>
        public readonly Rarity Rarity;

        /// <summary>
        /// </summary>
        public readonly HealthBarsSettings Settings;


        /// <summary>
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="pos"></param>
        /// <param name="entity"></param>
        public EntityParams(HealthBarsSettings settings, Vector2 pos, Entity entity)
        {
            Settings = settings;
            Pos = pos;
            HasMagicProperties = entity.TryGetComponent<ObjectMagicProperties>(out var magicProperties);
            Rarity = HasMagicProperties ? magicProperties.Rarity : Rarity.Normal;

            entity.TryGetComponent<Life>(out var entityLife);

            HpReserved = entityLife.Health.ReservedPercent / 100f;
            HpPercent = entityLife.Health.CurrentInPercent() * ((100 - HpReserved) / 100);

            ManaReserved = entityLife.Mana.ReservedPercent / 100f;
            ManaPercent = entityLife.Mana.CurrentInPercent() * ((100 - ManaReserved) / 100);

            var esReserved = entityLife.EnergyShield.ReservedPercent / 100f;
            EsPercent = entityLife.EnergyShield.CurrentInPercent() * ((100 - esReserved) / 100);
            EsTotal = entityLife.EnergyShield.Total;
        }

        /// <summary>
        /// </summary>
        public uint BorderColor => HasMagicProperties && DrawBorder ? RarityColor(Rarity) : 0;

        /// <summary>
        /// </summary>
        public bool DrawBorder => HasMagicProperties && Settings.ShowRarityBorders && (
            Rarity == Rarity.Normal && Settings.ShowNormalBorders ||
            Rarity == Rarity.Magic && Settings.ShowMagicBorders ||
            Rarity == Rarity.Rare && Settings.ShowRareBorders ||
            Rarity == Rarity.Unique && Settings.ShowUniqueBorders
        );

        /// <summary>
        /// </summary>
        public bool ShowCulling => HasMagicProperties && Settings.ShowCullRange && (
            Rarity == Rarity.Normal && Settings.ShowNormalCull ||
            Rarity == Rarity.Magic && Settings.ShowMagicCull ||
            Rarity == Rarity.Rare && Settings.ShowRareCull ||
            Rarity == Rarity.Unique && Settings.ShowUniqueCull
        );

        private uint RarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Unique => UiHelper.Color(Settings.UniqueColor * 255f),
                Rarity.Rare => UiHelper.Color(Settings.RareColor * 255f),
                Rarity.Magic => UiHelper.Color(Settings.MagicColor * 255f),
                _ => UiHelper.Color(Settings.NormalColor * 255f)
            };
        }
    }
}