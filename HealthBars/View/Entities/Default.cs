namespace HealthBars.View.Entities
{
    using System.Numerics;

    /// <inheritdoc />
    public abstract class Default : IEntity
    {
        /// <inheritdoc />
        public abstract void Draw(EntityParams eP, SpriteController spriteController);

        /// <inheritdoc />
        public abstract bool ShouldDraw(EntityParams entityParams);

        /// <summary>
        ///     Draws an empty bar sprite
        /// </summary>
        /// <param name="spriteController"></param>
        /// <param name="eP"></param>
        /// <param name="scale"></param>
        protected static void AddEmptyBar(SpriteController spriteController, EntityParams eP, float scale)
        {
            spriteController.DrawSprite("EmptyBar", scale, 1, 57, 108, 9, 110, 88, eP.Pos, 108, 9, -1, -1,
                false, eP.DrawBorder, eP.BorderColor);
        }

        /// <summary>
        ///     Draws a double empty bar sprite
        /// </summary>
        /// <param name="spriteController"></param>
        /// <param name="eP"></param>
        /// <param name="scale"></param>
        protected static void AddDoubleEmptyBar(SpriteController spriteController, EntityParams eP, float scale)
        {
            spriteController.DrawSprite("EmptyDoubleBar", scale, 1, 68, 108, 19, 110, 88, eP.Pos, 108, 19, -1,
                -1, false, eP.DrawBorder, eP.BorderColor);
        }


        /// <summary>
        ///     Draws a mana bar sprite
        /// </summary>
        /// <param name="spriteController"></param>
        /// <param name="eP"></param>
        /// <param name="scale"></param>
        /// <param name="manaSprite"></param>
        protected void AddManaBar(SpriteController spriteController, EntityParams eP, float scale, string manaSprite)
        {
            var manaPos = eP.Pos + new Vector2(0, 10) * scale;

            spriteController.DrawSprite("EmptyMana", scale, 1, 19, 1, 8, 110, 88, manaPos, 104, 8,
                100f - eP.ManaReserved, -1, false);
            spriteController.DrawSprite(manaSprite, scale, 1, 47, 1, 8, 110, 88, manaPos, 104, 8,
                eP.ManaPercent, -1, false);
        }
    }
}