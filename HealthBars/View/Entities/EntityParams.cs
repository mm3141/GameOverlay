using System.Numerics;
using GameHelper.RemoteEnums;
using GameHelper.RemoteObjects.Components;
using GameHelper.RemoteObjects.States.InGameStateObjects;
using GameHelper.Utils;

namespace HealthBars.View.Entities
{
    public class EntityParams
    {
        public readonly HealthBarsSettings Settings;
        public readonly Vector2 Pos;
        public readonly Rarity Rarity;
        public readonly float HpReserved;
        public readonly float HpPercent;
        public readonly float ManaReserved;
        public readonly float ManaPercent;
        public readonly float ESTotal;
        public readonly float ESPercent;
        public readonly uint BorderColor;
        public readonly bool DrawBorder;
        private readonly bool hasMagicProperties;

        public bool ShowCulling =>
            hasMagicProperties && Settings.ShowCullRange && (
                (Rarity == Rarity.Normal) && Settings.ShowNormalCull ||
                (Rarity == Rarity.Magic) && Settings.ShowMagicCull ||
                (Rarity == Rarity.Rare) && Settings.ShowRareCull ||
                (Rarity == Rarity.Unique) && Settings.ShowUniqueCull);

        public EntityParams(HealthBarsSettings settings, Vector2 pos, Entity entity)
        {
            Settings = settings;
            Pos = pos;
            entity.TryGetComponent<Life>(out var entityLife);
            entity.TryGetComponent<Positioned>(out var positioned);
            hasMagicProperties = entity.TryGetComponent<ObjectMagicProperties>(out var magicProperties);
            var rarity = hasMagicProperties ? magicProperties.Rarity : Rarity.Normal;
            var drawBorder = hasMagicProperties && this.Settings.ShowRarityBorders && (
                (rarity == Rarity.Normal) && this.Settings.ShowNormalBorders ||
                (rarity == Rarity.Magic) && this.Settings.ShowMagicBorders ||
                (rarity == Rarity.Rare) && this.Settings.ShowRareBorders ||
                (rarity == Rarity.Unique) && this.Settings.ShowUniqueBorders
            );
            BorderColor = hasMagicProperties && drawBorder ? this.RarityColor(rarity) : 0;
            HpReserved = (float)entityLife.Health.ReservedPercent / 100;
            HpPercent = entityLife.Health.CurrentInPercent() * ((100 - HpReserved) / 100);
            ManaReserved = (float)entityLife.Mana.ReservedPercent / 100;
            ManaPercent = entityLife.Mana.CurrentInPercent() * ((100 - ManaReserved) / 100);
            Rarity = rarity;
            var esReserved = (float)entityLife.EnergyShield.ReservedPercent / 100;
            ESPercent = entityLife.EnergyShield.CurrentInPercent() * ((100 - esReserved) / 100);
            ESTotal = entityLife.EnergyShield.Total;
            DrawBorder = drawBorder;
        }

        private uint RarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Unique => UiHelper.Color(this.Settings.UniqueColor * 255f),
                Rarity.Rare => UiHelper.Color(this.Settings.RareColor * 255f),
                Rarity.Magic => UiHelper.Color(this.Settings.MagicColor * 255f),
                _ => UiHelper.Color(this.Settings.NormalColor * 255f),
            };
        }
    }
}