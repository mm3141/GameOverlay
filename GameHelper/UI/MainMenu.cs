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
        private static int mainMenuHotKey = 0x7B; // F12
        private static bool foo1 = false;
        private static bool foo2 = false;
        private static int foo3 = 0;

        /// <summary>
        /// Gets the event raised when rendering the Main Menu.
        /// Use this if user wants to add stuff in main menu under their section.
        /// </summary>
        public static Event OnRenderMainMenuSection { get; private set; } = new Event();

        /// <summary>
        /// Initializes the Main Menu.
        /// </summary>
        public static void InitializeCoroutine()
        {
            CoroutineHandler.Start(RenderMainMenu());
        }

        private static IEnumerator<IWait> RenderMainMenu()
        {
            while (true)
            {
                yield return new WaitEvent(Overlay.OnRender);
                if (NativeMethods.isKeyPressed(mainMenuHotKey))
                {
                    isMainMenuVisible = !isMainMenuVisible;
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

                ImGui.Checkbox("Hide terminal on startup", ref foo1);
                ImGui.Checkbox("Close Game Helper When Game Closes", ref foo2);
                if (ImGui.InputInt($"Re-Read game files for {foo3} seconds", ref foo3, 5, 10))
                {
                    if (foo3 < 0)
                    {
                        foo3 = 0;
                    }
                    else if (foo3 > 30)
                    {
                        foo3 = 30;
                    }
                }

                ImGui.End();
            }
        }
    }
}
