namespace HealthBars.View.Entities
{
    using System.Numerics;
    using Controller;

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
            spriteController.DrawSprite("MonsterBar", eP.Pos, scale, -1, -1, false, eP.DrawBorder, eP.BorderColor);
        }

        /// <summary>
        ///     Draws a double empty bar sprite
        /// </summary>
        /// <param name="spriteController"></param>
        /// <param name="eP"></param>
        /// <param name="scale"></param>
        protected static void AddDoubleEmptyBar(SpriteController spriteController, EntityParams eP, float scale)
        {
            spriteController.DrawSprite("PlayerBars", eP.Pos, scale, -1, -1, false, eP.DrawBorder, eP.BorderColor);
        }


        /// <summary>
        ///     Draws a mana bar sprite
        /// </summary>
        /// <param name="spriteController"></param>
        /// <param name="eP"></param>
        /// <param name="scale"></param>
        protected static void AddManaBar(SpriteController spriteController, EntityParams eP, float scale)
        {
            var manaPos = eP.Pos + new Vector2(0, 10) * scale;

            spriteController.DrawSprite("EmptyMana", manaPos, scale, 100f - eP.ManaReserved, -1, false);
            spriteController.DrawSprite("Mana", manaPos, scale, eP.ManaPercent, -1, false);
        }
    }
}