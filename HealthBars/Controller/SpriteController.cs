namespace HealthBars
{
    using System.Collections.Generic;
    using System.Numerics;
    using GameHelper.Utils;
    using ImGuiNET;

    public class SpriteController
    {
        private readonly Dictionary<string, IconPicker> sprites = new();

        public void DrawSprite(
            string spriteName,
            float scale,
            float sx,
            float sy,
            float sw,
            float sh,
            float ssw,
            float ssh,
            Vector2 t,
            float tw,
            float th,
            float mulw,
            float mulh,
            bool marks,
            bool border = false,
            uint borderColor = 0,
            bool inner = false,
            bool fill = false)
        {
            var draw = ImGui.GetBackgroundDrawList();
            var uv0 = new Vector2(sx / ssw, sy / ssh);
            var uv1 = new Vector2((sx + sw) / ssw, (sy + sh) / ssh);
            var sprite = sprites[spriteName];
            var bounds = new Vector2(tw * ((mulw < 0 ? 100 : mulw) / 100), th * ((mulh < 0 ? 100 : mulh) / 100)) *
                         scale;
            var vBounds = new Vector2(tw, th) * scale;
            var half = new Vector2(10 + vBounds.X / 2, 0);
            var pos = t - half;

            draw.AddImage(sprite.TexturePtr, pos, pos + bounds, uv0, uv1);

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
                    draw.AddRectFilled(b1, b2, borderColor);
                else
                    draw.AddRect(b1, b2, borderColor);
            }
        }

        public void AddSprites(string spriteSheetPathName)
        {
            sprites.TryAdd("ES", new IconPicker(spriteSheetPathName, 1, 8, 108, 19, 1));
            sprites.TryAdd("EmptyHP", new IconPicker(spriteSheetPathName, 1, 8, 108, 19, 1));
            sprites.TryAdd("EmptyMana", new IconPicker(spriteSheetPathName, 1, 8, 108, 19, 1));
            sprites.TryAdd("EnemyHP", new IconPicker(spriteSheetPathName, 1, 8, 108, 19, 1));
            sprites.TryAdd("HP", new IconPicker(spriteSheetPathName, 1, 8, 108, 19, 1));
            sprites.TryAdd("Mana", new IconPicker(spriteSheetPathName, 1, 8, 108, 19, 1));
            sprites.TryAdd("EmptyBar", new IconPicker(spriteSheetPathName, 1, 8, 108, 19, 1));
            sprites.TryAdd("EmptyDoubleBar", new IconPicker(spriteSheetPathName, 1, 8, 108, 19, 1));
        }
    }
}