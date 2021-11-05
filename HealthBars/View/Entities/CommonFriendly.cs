namespace HealthBars.View.Entities
{
    using System.Numerics;
    using Controller;

    /// <inheritdoc />
    public abstract class CommonFriendly : Default
    {
        /// <summary>
        ///     Adds an Energy shield bar
        /// </summary>
        /// <param name="spriteController"></param>
        /// <param name="eP"></param>
        /// <param name="scale"></param>
        protected static void AddEnergyShieldBar(SpriteController spriteController, EntityParams eP, float scale)
        {
            var healthPos = eP.Pos + new Vector2(0, 1) * scale;
            spriteController.DrawSprite("ES", healthPos, scale, eP.EsPercent, -1, false, eP.DrawBorder, eP.BorderColor);
        }

        /// <summary>
        /// </summary>
        /// <param name="spriteController"></param>
        /// <param name="eP"></param>
        /// <param name="scale"></param>
        protected static void AddHealthBar(SpriteController spriteController, EntityParams eP, float scale)
        {
            var hpPos = eP.Pos + new Vector2(0, 1) * scale;
            spriteController.DrawSprite("EmptyHP", hpPos, scale, 100f - eP.HpReserved, -1, false);
            spriteController.DrawSprite("HP", hpPos, scale, eP.HpPercent, -1, eP.Settings.ShowFriendlyGradationMarks);
        }
    }
}