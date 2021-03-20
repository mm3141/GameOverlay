// <copyright file="RadarSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Radar
{
    using GameHelper.Plugin;

    /// <summary>
    /// <see cref="Radar"/> plugin settings class.
    /// </summary>
    public sealed class RadarSettings : IPSettings
    {
#pragma warning disable SA1401, SA1600
        public float LargeMapScaleMultiplier = 1f;
        public float LargeMapYFineTune = 1f;
#pragma warning restore SA1401, SA1600
    }
}
