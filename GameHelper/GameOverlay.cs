// <copyright file="GameOverlay.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper {
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClickableTransparentOverlay;
    using Coroutine;
    using CoroutineEvents;
    using GameHelper.Utils;
    using ImGuiNET;
    using Plugin;
    using Settings;
    using Ui;
    using Veldrid;

    /// <inheritdoc />
    public sealed class GameOverlay : Overlay {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameOverlay" /> class.
        /// </summary>
        internal GameOverlay(string windowTitle) : base(windowTitle, true) {
            CoroutineHandler.Start(this.UpdateOverlayBounds(), priority: int.MaxValue);
            SettingsWindow.InitializeCoroutines();
            PerformanceStats.InitializeCoroutines();
            DataVisualization.InitializeCoroutines();
            GameUiExplorer.InitializeCoroutines();
            OverlayKiller.InitializeCoroutines();
        }

        /// <summary>
        ///     Gets the fonts loaded in the overlay.
        /// </summary>
        public ImFontPtr[] Fonts { get; private set; }

        /// <inheritdoc />
        public override async Task Run() {
            Core.Initialize();
            Core.InitializeCororutines();
            await base.Run();
        }

        /// <inheritdoc />
        public override void Dispose() {
            base.Dispose();
            Core.Dispose();
        }

        /// <inheritdoc />
        protected override void PostStart() {
            if (MiscHelper.TryConvertStringToImGuiGlyphRanges(Core.GHSettings.FontCustomGlyphRange, out var glyphRanges)) {
                Core.Overlay.ReplaceFont(
                    Core.GHSettings.FontPathName,
                    Core.GHSettings.FontSize,
                    glyphRanges);
            }
            else {
                Core.Overlay.ReplaceFont(
                    Core.GHSettings.FontPathName,
                    Core.GHSettings.FontSize,
                    Core.GHSettings.FontLanguage);
            }

            PManager.InitializePlugins();
        }

        /// <inheritdoc />
        protected override Task Render() {
            CoroutineHandler.Tick(ImGui.GetIO().DeltaTime);
            CoroutineHandler.RaiseEvent(GameHelperEvents.PerFrameDataUpdate);
            CoroutineHandler.RaiseEvent(GameHelperEvents.OnRender);
            if (!Core.GHSettings.IsOverlayRunning) {
                this.Close();
            }

            return Task.CompletedTask;
        }

        private IEnumerator<Wait> UpdateOverlayBounds() {
            while (true) {
                yield return new Wait(GameHelperEvents.OnMoved);
                this.Position = new Point(Core.Process.WindowArea.Location.X, Core.Process.WindowArea.Location.Y);
                this.Size = new Point(Core.Process.WindowArea.Size.Width, Core.Process.WindowArea.Size.Height);
            }
        }
    }
}