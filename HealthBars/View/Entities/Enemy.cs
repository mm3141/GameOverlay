using System.Numerics;
using GameHelper.RemoteEnums;
using GameHelper.Utils;

namespace HealthBars.View.Entities
{
    public class Enemy : IEntity
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
            var drawBorder = entityParams.DrawBorder;
            var borderColor = entityParams.BorderColor;
            if (entityParams.Settings.ShowEnemyMana)
            {
                spriteController.DrawSprite("EmptyDoubleBar", scale, 1, 68, 108, 19, 110, 88, location, 108, 19, -1,
                    -1, false,
                    drawBorder, borderColor);

                spriteController.DrawSprite("EmptyMana", scale, 1, 19, 1, 8, 110, 88, location + manaOffset, 104, 8,
                    100f - manaReserved, -1, false);
                spriteController.DrawSprite("Mana", scale, 1, 47, 1, 8, 110, 88, location + manaOffset, 104, 8,
                    manaPercent, -1,
                    false);
            }
            else
            {
                spriteController.DrawSprite("EmptyBar", scale, 1, 57, 108, 9, 110, 88, location, 108, 9, -1, -1,
                    false,
                    drawBorder,
                    borderColor);
            }

            var inCullingRange = entityParams.ShowCulling && hpPercent > 0 &&
                                 hpPercent < entityParams.Settings.CullingRange;
            var cullingColor = UiHelper.Color(entityParams.Settings.CullRangeColor * 255f);
            spriteController.DrawSprite("EmptyHP", scale, 1, 10, 1, 7, 110, 88, location + hpOffset, 104, 7,
                100f - hpReserved,
                -1,
                false);
            spriteController.DrawSprite("EnemyHP", scale, 1, 29, 1, 7, 110, 88, location + hpOffset, 104, 7,
                hpPercent, -1,
                entityParams.Settings.ShowEnemyGradationMarks, inCullingRange, cullingColor, true, true);
        }

        private float RarityBarScale(EntityParams entityParams)
        {
            return entityParams.Rarity switch
            {
                Rarity.Unique => entityParams.Settings.UniqueBarScale,
                Rarity.Rare => entityParams.Settings.RareBarScale,
                Rarity.Magic => entityParams.Settings.MagicBarScale,
                _ => entityParams.Settings.NormalBarScale,
            };
        }
    }
}