namespace HealthBars.View.Entities
{
    using System;
    using System.Numerics;
    using Controller;
    using GameHelper.RemoteEnums;
    using GameHelper.Utils;

    /// <inheritdoc />
    public class Invalid : Default
    {
        /// <inheritdoc />
        public override void Draw(EntityParams eP, SpriteController spriteController)
        {
            throw new Exception("Can not draw invalid entity.");
        }

        /// <inheritdoc />
        public override bool ShouldDraw(EntityParams entityParams)
        {
            throw new Exception("Can not draw invalid entity.");
        }
    }
}