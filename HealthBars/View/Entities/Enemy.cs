namespace HealthBars.View.Entities
{
    using System.Numerics;
    using Controller;
    using GameHelper.RemoteEnums;
    using GameHelper.Utils;

    /// <inheritdoc />
    public class Enemy : Default
    {
        /// <inheritdoc />
        public override void Draw(EntityParams eP, SpriteController spriteController)
        {
            var scale = RarityBarScale(eP);

            if (eP.Settings.ShowEnemyMana)
            {
                AddDoubleEmptyBar(spriteController, eP, scale);
                AddManaBar(spriteController, eP, scale);
            }
            else
            {
                AddEmptyBar(spriteController, eP, scale);
            }

            AddHealthBar(spriteController, eP, scale);
        }

        /// <inheritdoc />
        public override bool ShouldDraw(EntityParams entityParams)
        {
            var rarity = entityParams.Rarity;
            var settings = entityParams.Settings;

            return entityParams.HasMagicProperties && (
                rarity == Rarity.Normal && settings.ShowNormalBar ||
                rarity == Rarity.Magic && settings.ShowMagicBar ||
                rarity == Rarity.Rare && settings.ShowRareBar ||
                rarity == Rarity.Unique && settings.ShowUniqueBar
            );
        }

        private static void AddHealthBar(SpriteController spriteController, EntityParams eP, float scale)
        {
            var hpPos = eP.Pos + new Vector2(0, 1) * scale;
            spriteController.DrawSprite("EmptyHP", hpPos, scale, 100f - eP.HpReserved, -1, false);

            var inCullingRange = InCullingRange(eP, eP.HpPercent);
            var cullingColor = ImGuiHelper.Color(eP.Settings.CullRangeColor * 255f);
            spriteController.DrawSprite("EnemyHP", hpPos, scale, eP.HpPercent, -1, eP.Settings.ShowEnemyGradationMarks,
                inCullingRange, cullingColor);
        }

        private static float RarityBarScale(EntityParams entityParams)
        {
            return entityParams.Rarity switch
            {
                Rarity.Unique => entityParams.Settings.UniqueBarScale,
                Rarity.Rare => entityParams.Settings.RareBarScale,
                Rarity.Magic => entityParams.Settings.MagicBarScale,
                _ => entityParams.Settings.NormalBarScale
            };
        }

        private static bool InCullingRange(EntityParams entityParams, float hpPercent)
        {
            return entityParams.ShowCulling && hpPercent > 0 && hpPercent < entityParams.Settings.CullingRange;
        }
    }
}