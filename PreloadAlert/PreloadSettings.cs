// <copyright file="PreloadSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PreloadAlert
{
    using System.Numerics;
    using GameHelper.Plugin;

    /// <summary>
    /// Preload GUI settings.
    /// </summary>
    public sealed class PreloadSettings : IPSettings
    {
#pragma warning disable SA1401, SA1600 // Fields should be private
        public Vector2 Pos = Vector2.Zero;
        public Vector2 Size = Vector2.Zero;
        public Vector4 BackgroundColor = Vector4.One;
        public bool Locked = false;
#pragma warning restore SA1401, SA1600 // Fields should be private
    }
}
