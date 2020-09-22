using GameHelper.Plugin;
using System.Numerics;

namespace PreloadAlert
{
    public sealed class PreloadSettings : IPSettings
    {
        public Vector2 Pos = Vector2.Zero;
        public Vector2 Size = Vector2.Zero;
        public Vector4 BackgroundColor = Vector4.Zero;
        public bool Locked = false;
        public bool DebugMode = false;
    }
}
