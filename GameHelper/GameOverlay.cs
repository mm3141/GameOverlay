// <copyright file="GameOverlay.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClickableTransparentOverlay;
    using Coroutine;
    using GameHelper.Plugin;
    using GameHelper.Settings;
    using ImGuiNET;

    /// <inheritdoc/>
    internal class GameOverlay : Overlay
    {
        /// <summary>
        /// To submit ImGui code for generating the UI.
        /// </summary>
        internal static readonly Event OnRender = new Event();

        /// <summary>
        /// Initializes a new instance of the <see cref="GameOverlay"/> class.
        /// </summary>
        internal GameOverlay()
        {
            CoroutineHandler.Start(this.UpdateOverlayBounds());
            SettingsWindow.InitializeCoroutines();
            PManager.InitializePlugins();
            Core.InitializeCororutines();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            Core.Dispose();
        }

        /// <inheritdoc/>
        protected override Task Render()
        {
            CoroutineHandler.Tick(ImGui.GetIO().DeltaTime);
            CoroutineHandler.RaiseEvent(OnRender);
            if (!Core.GHSettings.IsOverlayRunning)
            {
                this.Close();
            }

            return Task.CompletedTask;
        }

        private IEnumerator<Wait> UpdateOverlayBounds()
        {
            while (true)
            {
                yield return new Wait(Core.Process.OnMoved);
                this.Position = new Veldrid.Point(Core.Process.WindowArea.Location.X, Core.Process.WindowArea.Location.Y);
                this.Size = new Veldrid.Point(Core.Process.WindowArea.Size.Width, Core.Process.WindowArea.Size.Height);
            }
        }
    }
}
