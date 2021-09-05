// <copyright file="OverlayKiller.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Ui
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using ImGuiNET;

    /// <summary>
    /// Kills the overlay.
    /// </summary>
    public static class OverlayKiller
    {
        private static Stopwatch sw = Stopwatch.StartNew();
        private static int timelimit = 20;
        private static Vector2 size = new Vector2(400);

        /// <summary>
        /// Initializes the co-routines.
        /// </summary>
        internal static void InitializeCoroutines()
        {
            CoroutineHandler.Start(OverlayKillerCoRoutine());
            CoroutineHandler.Start(OnAreaChange());
        }

        private static IEnumerator<Wait> OverlayKillerCoRoutine()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnRender);
                if (!Core.States.InGameStateObject.CurrentAreaInstance.AreaDetails.IsBattleRoyale)
                {
                    sw.Restart();
                    continue;
                }

                ImGui.SetNextWindowSize(size);
                ImGui.Begin("Player Vs Player (PVP) Detected");
                ImGui.TextWrapped("Please don't cheat in PvP mode. GameHelper was not " +
                    "created for PvP cheating. Overlay will close " +
                    $"in {timelimit - (int)sw.Elapsed.TotalSeconds} seconds.");
                ImGui.End();

                if (sw.Elapsed.TotalSeconds > timelimit)
                {
                    Core.Overlay.Close();
                }
            }
        }

        private static IEnumerator<Wait> OnAreaChange()
        {
            while (true)
            {
                yield return new Wait(RemoteEvents.AreaChanged);
                sw.Restart();
            }
        }
    }
}
