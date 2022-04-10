namespace HealthBars.Controller
{
    using System;
    using System.Globalization;
    using System.Numerics;
    using GameHelper;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// </summary>
    public class SpriteController
    {
        private readonly SpriteAtlas spriteAtlas;
        private readonly uint markColor = ImGuiHelper.Color(255, 255, 255, 100);

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
        /// <param name="scale"></param>
        /// <param name="virtualWidthFill"></param>
        /// <param name="virtualHeightFill"></param>
        /// <param name="marks"></param>
        /// <param name="border"></param>
        /// <param name="borderColor"></param>
        public void DrawSprite(
            string spriteName,
            Vector2 drawCoordinates,
            float scale,
            float virtualWidthFill,
            float virtualHeightFill,
            bool marks,
            bool border = false,
            uint borderColor = 0)
        {
            var sprite = this.spriteAtlas.GetSprite(spriteName);
            var offsetX = 0f;
            var offsetY = 23.17313f + Core.States.InGameStateObject.CurrentAreaInstance.Zoom * 14.69196f;

            if (sprite == null)
            {
                return;
            }

            var draw = ImGui.GetBackgroundDrawList();

            var bounds = new Vector2(
                sprite.VirtualBounds.X * ((virtualWidthFill < 0 ? 100 : virtualWidthFill) / 100),
                sprite.VirtualBounds.Y * ((virtualHeightFill < 0 ? 100 : virtualHeightFill) / 100)
            ) * scale;
            var vBounds = sprite.VirtualBounds * scale;
            var half = new Vector2(offsetX + vBounds.X / 2, offsetY);
            var pos = drawCoordinates - half;

            draw.AddImage(this.spriteAtlas.TexturePtr, pos, pos + bounds, sprite.Uv[0], sprite.Uv[1]);

            if (marks)
            {
                Vector2 markLine = new(0, vBounds.Y - 1.5f);
                Vector2 mark25 = new(vBounds.X * 0.25f, 0);
                Vector2 mark50 = new(vBounds.X * 0.5f, 0);
                Vector2 mark75 = new(vBounds.X * 0.75f, 0);

                draw.AddLine(pos + mark25, pos + markLine + mark25, this.markColor);
                draw.AddLine(pos + mark50, pos + markLine + mark50, this.markColor);
                draw.AddLine(pos + mark75, pos + markLine + mark75, this.markColor);
            }

            if (border)
            {
                draw.AddRect(pos, pos + vBounds, borderColor, 0f, ImDrawFlags.RoundCornersNone, 4f);
            }
        }
    }
}