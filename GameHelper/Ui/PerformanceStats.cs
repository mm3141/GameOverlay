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
                    ImGui.Text($"Total Event Coroutines: {CoroutineHandler.EventCount}");
                    ImGui.Text($"Total Tick Coroutines: {CoroutineHandler.TickingCount}");
                    var cAI = Core.States.InGameStateObject.CurrentAreaInstance;
                    ImGui.Text($"Total Entities: {cAI.AwakeEntities.Count}");
                    ImGui.Text($"Currently Active Entities: {cAI.NetworkBubbleEntityCount}");
                    ImGui.Text($"FPS: {ImGui.GetIO().Framerate}");
                    ImGui.NewLine();
                    for (int i = 0; i < Core.CoroutinesRegistrar.Count; i++)
                    {
                        var coroutine = Core.CoroutinesRegistrar[i];
                        if (coroutine.IsFinished)
                        {
                            Core.CoroutinesRegistrar.Remove(coroutine);
                        }

                        ImGui.Text($"{coroutine.Name}: " +
                            $"{(int)coroutine.LastMoveNextTime.TotalMilliseconds}(ms)");
                    }

                    ImGui.End();
                }
            }
        }
    }
}
