// <copyright file="PerformanceStats.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Ui
{
    using System.Collections.Generic;
    using System.Numerics;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// Visualize the co-routines stats.
    /// </summary>
    public static class PerformanceStats
    {
        /// <summary>
        /// Initializes the co-routines.
        /// </summary>
        internal static void InitializeCoroutines()
        {
            CoroutineHandler.Start(PerformanceStatRenderCoRoutine());
        }

        /// <summary>
        /// Draws the window to display the perf stats.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> PerformanceStatRenderCoRoutine()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnRender);
                if (Core.GHSettings.ShowPerfStats)
                {
                    ImGui.SetNextWindowPos(Vector2.Zero);
                    ImGui.Begin("Perf Stats Window", UiHelper.TransparentWindowFlags);
                    ImGui.Text($"Performance Related Stats");
                    for (int i = 0; i < Core.CoroutinesRegistrar.Count; i++)
                    {
                        var coroutine = Core.CoroutinesRegistrar[i];
                        if (coroutine.IsFinished)
                        {
                            Core.CoroutinesRegistrar.Remove(coroutine);
                        }

                        ImGui.Text($"{coroutine.Name}: " +
                            $"{coroutine.AverageMoveNextTime.Milliseconds}(ms)");
                    }

                    ImGui.End();
                }
            }
        }
    }
}
