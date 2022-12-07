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
            this.Settings = settings;
            this.Pos = pos;
            this.HasMagicProperties = entity.GetComp<ObjectMagicProperties>(out var magicProperties);
            this.Rarity = this.HasMagicProperties ? magicProperties.Rarity : Rarity.Normal;

            entity.GetComp<Life>(out var entityLife);

            this.HpReserved = entityLife.Health.ReservedPercent / 100f;
            this.HpPercent = entityLife.Health.CurrentInPercent() * ((100 - this.HpReserved) / 100);

            this.ManaReserved = entityLife.Mana.ReservedPercent / 100f;
            this.ManaPercent = entityLife.Mana.CurrentInPercent() * ((100 - this.ManaReserved) / 100);

            var esReserved = entityLife.EnergyShield.ReservedPercent / 100f;
            this.EsPercent = entityLife.EnergyShield.CurrentInPercent() * ((100 - esReserved) / 100);
            this.EsTotal = entityLife.EnergyShield.Total;
        }

        /// <summary>
        /// </summary>
        public uint BorderColor => this.HasMagicProperties && this.DrawBorder ? this.RarityColor(this.Rarity) : 0;

        /// <summary>
        /// </summary>
        public bool DrawBorder =>
            this.HasMagicProperties && this.Settings.ShowRarityBorders &&
            (this.Rarity == Rarity.Normal && this.Settings.ShowNormalBorders ||
             this.Rarity == Rarity.Magic && this.Settings.ShowMagicBorders ||
             this.Rarity == Rarity.Rare && this.Settings.ShowRareBorders ||
             this.Rarity == Rarity.Unique && this.Settings.ShowUniqueBorders
            );

        /// <summary>
        /// </summary>
        public bool ShowCulling =>
            this.HasMagicProperties && this.Settings.ShowCullRange && (this.Rarity == Rarity.Normal && this.Settings.ShowNormalCull ||
                                                                       this.Rarity == Rarity.Magic && this.Settings.ShowMagicCull ||
                                                                       this.Rarity == Rarity.Rare && this.Settings.ShowRareCull ||
                                                                       this.Rarity == Rarity.Unique && this.Settings.ShowUniqueCull
            );

        private uint RarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Unique => ImGuiHelper.Color(this.Settings.UniqueColor * 255f),
                Rarity.Rare => ImGuiHelper.Color(this.Settings.RareColor * 255f),
                Rarity.Magic => ImGuiHelper.Color(this.Settings.MagicColor * 255f),
                _ => ImGuiHelper.Color(this.Settings.NormalColor * 255f)
            };
        }
    }
}