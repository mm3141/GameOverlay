// <copyright file="SimpleFlaskManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager
{
    using GameHelper;
    using GameHelper.Plugin;
    using GameHelper.RemoteEnums;

    /// <summary>
    /// <see cref="SimpleFlaskManager"/> plugin.
    /// </summary>
    public sealed class SimpleFlaskManager : PCore<SimpleFlaskManagerSettings>
    {
        /// <inheritdoc/>
        public override void DrawSettings()
        {
        }

        /// <inheritdoc/>
        public override void DrawUI()
        {
            if (Core.States.GameCurrentState != GameStateTypes.InGameState)
            {
                return;
            }
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
        }

        /// <inheritdoc/>
        public override void OnEnable(bool isGameOpened)
        {
        }

        /// <inheritdoc/>
        public override void SaveSettings()
        {
        }
    }
}
