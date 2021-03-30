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
#pragma warning disable SA1401 // Fields should be private
        /// <summary>
        /// Position of the preload alert window.
        /// </summary>
        public Vector2 Pos = Vector2.Zero;

        /// <summary>
        /// Size of the preload alert window.
        /// </summary>
        public Vector2 Size = Vector2.Zero;

        /// <summary>
        /// Background color of the preload alert window.
        /// </summary>
        public Vector4 BackgroundColor = new Vector4(Vector3.Zero, 0.8f);

        /// <summary>
        /// Gets a value indicating whether the preload alert window is locked or not.
        /// </summary>
        public bool Locked = false;

        /// <summary>
        /// Gets a value indicating whether to hide the Ui when not in the game or game in background.
        /// </summary>
        public bool EnableHideUi = false;
#pragma warning restore SA1401 // Fields should be private
    }
}
