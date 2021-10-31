using System.Numerics;

namespace HealthBars.View.Entities
{
    public class CurrentPlayer : IEntity
    {
        public void Draw(EntityParams entityParams, SpriteController spriteController)
        {
            var scale = RarityBarScale(entityParams);
            var location = entityParams.Pos;
            var manaOffset = new Vector2(0, 10) * scale;
            var hpOffset = new Vector2(0, 1) * scale;
            var manaReserved = entityParams.ManaReserved;
            var manaPercent = entityParams.ManaPercent;
            var hpPercent = entityParams.HpPercent;
            var hpReserved = entityParams.HpReserved;
            var manaPos = location + manaOffset;

            spriteController.DrawSprite("EmptyDoubleBar", scale, 1, 68, 108, 19, 110, 88, location, 108, 19, -1,
                -1, false);
            spriteController.DrawSprite("EmptyMana", scale, 1, 19, 1, 8, 110, 88, manaPos, 104, 8,
                100f - manaReserved, -1, false);
            spriteController.DrawSprite("Mana", scale, 1, 47, 1, 8, 110, 88, manaPos, 104, 8, manaPercent, -1,
                false);
            spriteController.DrawSprite("EmptyHP", scale, 1, 10, 1, 7, 110, 88, location + hpOffset, 104, 7,
                100f - hpReserved,
                -1,
                false);
            spriteController.DrawSprite("HP", scale, 1, 38, 1, 7, 110, 88, location + hpOffset, 104, 7, hpPercent,
                -1,
                entityParams.Settings.ShowFriendlyGradationMarks);
            if (entityParams.ESTotal > 0)
            {
                spriteController.DrawSprite("ES", scale, 1, 1, 1, 7, 110, 88, location + hpOffset, 104, 7,
                    entityParams.ESPercent,
                    -1, false);
            }
        }

        private static float RarityBarScale(EntityParams entityParams)
        {
            return entityParams.Settings.PlayerBarScale;
        }
    }
}