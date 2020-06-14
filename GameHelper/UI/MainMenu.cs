// <copyright file="MainMenu.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.UI
{
    using System.Collections.Generic;
    using System.Numerics;
    using ClickableTransparentOverlay;
    using Coroutine;
    using ImGuiNET;

    /// <summary>
    /// Creates the MainMenu on the UI.
    /// </summary>
    public static class MainMenu
    {
        private static bool isMainMenuVisible = true;
        private static CoreSettings coreSettings = null;

        /// <summary>
        /// Gets the event raised when rendering the Main Menu.
        /// Use this if user wants to add stuff in main menu under their section.
        /// </summary>
        public static Event OnRenderMainMenuSection { get; private set; } = new Event();

        /// <summary>
        /// Initializes the Main Menu.
        /// </summary>
        /// <param name="settings">CoreSettings instance to associate with the MainMenu.</param>
        public static void InitializeCoroutines(CoreSettings settings)
        {
            coreSettings = settings;
            CoroutineHandler.Start(DrawMainMenu());
        }

        private static IEnumerator<Wait> DrawMainMenu()
        {
            while (true)
            {
                yield return new Wait(Overlay.OnRender);
                if (NativeMethods.IsKeyPressed(coreSettings.MainMenuHotKey))
                {
                    isMainMenuVisible = !isMainMenuVisible;
                    if (!isMainMenuVisible)
                    {
                        coreSettings.SafeToFile();
                    }
                }

                if (!isMainMenuVisible)
                {
                    continue;
                }

                bool isOverlayRunning = true;
                ImGui.SetNextWindowSizeConstraints(new Vector2(800, 600), new Vector2(1024, 1024));
                var isMainMenuExpanded = ImGui.Begin(
                    "Game Overlay Menu",
                    ref isOverlayRunning,
                    ImGuiWindowFlags.NoSavedSettings);
                Overlay.Close = !isOverlayRunning;
                if (!isMainMenuExpanded)
                {
                    ImGui.End();
                    continue;
                }

                if (ImGui.Checkbox("Hide terminal on startup", ref coreSettings.HideTerminal))
                {
                    Overlay.TerminalWindow = !coreSettings.HideTerminal;
                }

                ImGui.Checkbox("Close Game Helper When Game Closes", ref coreSettings.CloseOnGameExit);
                ImGui.End();
            }
        }
    }
}
