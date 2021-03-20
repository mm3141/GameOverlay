// <copyright file="GameOverlay.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClickableTransparentOverlay;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.Plugin;
    using GameHelper.Settings;
    using GameHelper.Ui;
    using ImGuiNET;

    /// <inheritdoc/>
    internal class GameOverlay : Overlay
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameOverlay"/> class.
        /// </summary>
        internal GameOverlay()
        {
            CoroutineHandler.Start(this.UpdateOverlayBounds(), priority: int.MaxValue);
            SettingsWindow.InitializeCoroutines();
            PerformanceStats.InitializeCoroutines();
            DataVisualization.InitializeCoroutines();
            GameUiExplorer.InitializeCoroutines();
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
            CoroutineHandler.RaiseEvent(GameHelperEvents.PerFrameDataUpdate);
            CoroutineHandler.RaiseEvent(GameHelperEvents.OnRender);
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
                yield return new Wait(GameHelperEvents.OnMoved);
                this.Position = new Veldrid.Point(Core.Process.WindowArea.Location.X, Core.Process.WindowArea.Location.Y);
                this.Size = new Veldrid.Point(Core.Process.WindowArea.Size.Width, Core.Process.WindowArea.Size.Height);
            }
        }
    }
}
