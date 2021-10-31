namespace HealthBars.View.Entities {
    using System.Numerics;

    public class Friendly : IEntity {
        public void Draw(EntityParams entityParams, SpriteController spriteController) {
            if (!ShouldDraw(entityParams)) {
                return;
            }

            var scale = RarityBarScale(entityParams);
            var location = entityParams.Pos;
            var hpOffset = new Vector2(0, 1) * scale;
            var hpReserved = entityParams.HpReserved;

            spriteController.DrawSprite("EmptyBar", scale, 1, 57, 108, 9, 110, 88, location, 108, 9, -1, -1,
                false);
            spriteController.DrawSprite("EmptyHP", scale, 1, 10, 1, 7, 110, 88, location + hpOffset, 104, 7,
                100f - hpReserved,
                -1,
                false);
            spriteController.DrawSprite("HP", scale, 1, 38, 1, 7, 110, 88, location + hpOffset, 104, 7,
                entityParams.HpPercent,
                -1,
                entityParams.Settings.ShowFriendlyGradationMarks);
            if (entityParams.ESTotal > 0) {
                spriteController.DrawSprite("ES", scale, 1, 1, 1, 7, 110, 88, location + hpOffset, 104, 7,
                    entityParams.ESPercent,
                    -1, false);
            }
        }

        public bool ShouldDraw(EntityParams entityParams) {
            return entityParams.Settings.ShowFriendlyBars;
        }

        private static float RarityBarScale(EntityParams entityParams) {
            return entityParams.Settings.FriendlyBarScale;
        }
    }
}