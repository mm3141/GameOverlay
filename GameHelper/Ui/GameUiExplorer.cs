// <copyright file="GameUiExplorer.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Ui
{
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.RemoteObjects.UiElement;
    using ImGuiNET;

    /// <summary>
    /// Explore (visualize) the Game Ui Elements.
    /// </summary>
    public static class GameUiExplorer
    {
        /// <summary>
        /// Initializes the co-routines.
        /// </summary>
        internal static void InitializeCoroutines()
        {
            CoroutineHandler.Start(GameUiExplorerRenderCoRoutine());
        }

        /// <summary>
        /// Adds the UiElementBase to GameUiExplorer.
        /// </summary>
        /// <param name="element">UiElementBase to investigate.</param>
        internal static void AddUiElement(UiElementBase element)
        {
        }

        /// <summary>
        /// Draws the window to display the Game UiExplorer.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> GameUiExplorerRenderCoRoutine()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnRender);
                if (Core.GHSettings.ShowGameUiExplorer)
                {
                    if (ImGui.Begin("Game UiExplorer", ref Core.GHSettings.ShowGameUiExplorer))
                    {
                    }

                    ImGui.End();
                }
            }
        }
    }
}
