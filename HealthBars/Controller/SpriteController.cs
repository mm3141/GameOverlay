namespace HealthBars
{
    using System.Numerics;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// </summary>
    public class SpriteController
    {
        private readonly SpriteAtlas spriteAtlas;

        /// <summary>
        ///     Initialize SpriteController
        /// </summary>
        /// <param name="spriteAtlas"></param>
        public SpriteController(SpriteAtlas spriteAtlas)
        {
            this.spriteAtlas = spriteAtlas;
        }

        /// <summary>
        ///     Draw sprite from sprite atlas.
        /// </summary>
        /// <param name="spriteName"></param>
        /// <param name="drawCoordinates"></param>
        /// <param name="virtualWidth"></param>
        /// <param name="virtualHeight"></param>
        /// <param name="scale"></param>
        /// <param name="multiplyWidth"></param>
        /// <param name="multiplyHeight"></param>
        /// <param name="marks"></param>
        /// <param name="border"></param>
        /// <param name="borderColor"></param>
        /// <param name="inner"></param>
        /// <param name="fill"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public void DrawSprite(
            string spriteName,
            Vector2 drawCoordinates,
            float virtualWidth,
            float virtualHeight,
            float scale,
            float multiplyWidth,
            float multiplyHeight,
            bool marks,
            bool border = false,
            uint borderColor = 0,
            bool inner = false,
            bool fill = false,
            float offsetX = 10,
            float offsetY = 0
        )
        {
            var sprite = this.spriteAtlas.GetSprite(spriteName);

            if (sprite == null)
            {
                return;
            }
            
            var draw = ImGui.GetBackgroundDrawList();

            var uv0 = new Vector2(
                sprite.X / this.spriteAtlas.SpritesheetSize.W,
                sprite.Y / this.spriteAtlas.SpritesheetSize.H
            );
            var uv1 = new Vector2(
                (sprite.X + sprite.W) / this.spriteAtlas.SpritesheetSize.W,
                (sprite.Y + sprite.H) / this.spriteAtlas.SpritesheetSize.H
            );

            var bounds = new Vector2(
                virtualWidth * ((multiplyWidth < 0 ? 100 : multiplyWidth) / 100),
                virtualHeight * ((multiplyHeight < 0 ? 100 : multiplyHeight) / 100)
            ) * scale;
            var vBounds = new Vector2(virtualWidth, virtualHeight) * scale;
            var half = new Vector2(offsetX + vBounds.X / 2, offsetY);
            var pos = drawCoordinates - half;

            draw.AddImage(this.spriteAtlas.TexturePtr, pos, pos + bounds, uv0, uv1);

            if (marks)
            {
                var markColor = UiHelper.Color(255, 255, 255, 100);
                Vector2 markLine = new(0, vBounds.Y - 1.5f);
                Vector2 mark25 = new(vBounds.X * 0.25f, 0);
                Vector2 mark50 = new(vBounds.X * 0.5f, 0);
                Vector2 mark75 = new(vBounds.X * 0.75f, 0);

                draw.AddLine(pos + mark25, pos + markLine + mark25, markColor);
                draw.AddLine(pos + mark50, pos + markLine + mark50, markColor);
                draw.AddLine(pos + mark75, pos + markLine + mark75, markColor);
            }

            if (border)
            {
                var b1 = pos;
                var b2 = pos + (inner ? bounds : vBounds);

                if (fill)
                {
                    draw.AddRectFilled(b1, b2, borderColor);
                }
                else
                {
                    draw.AddRect(b1, b2, borderColor);
                }
            }
        }
    }
}