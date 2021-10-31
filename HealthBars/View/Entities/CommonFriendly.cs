namespace HealthBars.View.Entities {
    using System.Numerics;

    /// <inheritdoc />
    public abstract class CommonFriendly : Default {
        /// <summary>
        /// Adds an Energy shield bar
        /// </summary>
        /// <param name="spriteController"></param>
        /// <param name="eP"></param>
        /// <param name="scale"></param>
        protected static void AddEnergyShieldBar(SpriteController spriteController, EntityParams eP, float scale) {
            var healthPos = eP.Pos + new Vector2(0, 1) * scale;
            spriteController.DrawSprite("ES", scale, 1, 1, 1, 7, 110, 88, healthPos, 104, 7,
                eP.EsPercent, -1, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteController"></param>
        /// <param name="eP"></param>
        /// <param name="scale"></param>
        protected static void AddHealthBar(SpriteController spriteController, EntityParams eP, float scale) {
            var hpPos = eP.Pos + new Vector2(0, 1) * scale;
            spriteController.DrawSprite("EmptyHP", scale, 1, 10, 1, 7, 110, 88, hpPos, 104, 7,
                100f - eP.HpReserved, -1, false);
            spriteController.DrawSprite("HP", scale, 1, 38, 1, 7, 110, 88, hpPos, 104, 7, eP.HpPercent, -1,
                eP.Settings.ShowFriendlyGradationMarks);
        }
    }
}