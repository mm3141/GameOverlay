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
        private static Dictionary<string, MovingAverage> movingAverage
            = new Dictionary<string, MovingAverage>();

        private static bool isPerformanceWindowHovered = false;

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
                    if (Core.GHSettings.HidePerfStatsWhenBg && !Core.Process.Foreground)
                    {
                        continue;
                    }

                    ImGui.SetNextWindowPos(Vector2.Zero);
                    if (isPerformanceWindowHovered)
                    {
                        ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);
                        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
                    }

                    ImGui.Begin("Perf Stats Window", UiHelper.TransparentWindowFlags);

                    if (isPerformanceWindowHovered)
                    {
                        ImGui.PopStyleVar();
                        ImGui.PopStyleColor();
                    }

                    isPerformanceWindowHovered = ImGui.IsMouseHoveringRect(Vector2.Zero, ImGui.GetWindowSize());
                    if (isPerformanceWindowHovered)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, Vector4.Zero);
                    }

                    ImGui.Text($"Performance Related Stats");
                    ImGui.Text($"Total Event Coroutines: {CoroutineHandler.EventCount}");
                    ImGui.Text($"Total Tick Coroutines: {CoroutineHandler.TickingCount}");
                    var cAI = Core.States.InGameStateObject.CurrentAreaInstance;
                    ImGui.Text($"Total Entities: {cAI.AwakeEntities.Count}");
                    ImGui.Text($"Currently Active Entities: {cAI.NetworkBubbleEntityCount}");
                    var fps = ImGui.GetIO().Framerate;
                    ImGui.Text($"FPS: {fps}");
                    ImGui.NewLine();
                    ImGui.Text($"==Average of last {(int)(1440 / fps)} seconds==");
                    for (int i = 0; i < Core.CoroutinesRegistrar.Count; i++)
                    {
                        var coroutine = Core.CoroutinesRegistrar[i];
                        if (coroutine.IsFinished)
                        {
                            Core.CoroutinesRegistrar.Remove(coroutine);
                        }

                        if (movingAverage.TryGetValue(coroutine.Name, out var value))
                        {
                            value.ComputeAverage(
                                coroutine.LastMoveNextTime.TotalMilliseconds,
                                coroutine.MoveNextCount);
                            ImGui.Text($"{coroutine.Name}: {value.Average:0.00}(ms)");
                        }
                        else
                        {
                            movingAverage[coroutine.Name] = new MovingAverage();
                        }
                    }

                    if (isPerformanceWindowHovered)
                    {
                        ImGui.PopStyleColor();
                    }

                    ImGui.End();
                }
            }
        }

        private class MovingAverage
        {
            private Queue<double> samples = new Queue<double>();
            private int windowSize = 144 * 10; // 10 seconds moving average @ 144 FPS.
            private double sampleAccumulator;
            private int lastIterationNumber = 0;

            public double Average { get; private set; }

            /// <summary>
            /// Computes a new windowed average each time a new sample arrives.
            /// </summary>
            /// <param name="newSample">new sample to add into the moving average.</param>
            /// <param name="iterationNumber">iteration number who's sample you are adding.</param>
            public void ComputeAverage(double newSample, int iterationNumber)
            {
                if (iterationNumber <= this.lastIterationNumber)
                {
                    return;
                }

                this.lastIterationNumber = iterationNumber;
                this.sampleAccumulator += newSample;
                this.samples.Enqueue(newSample);

                if (this.samples.Count > this.windowSize)
                {
                    this.sampleAccumulator -= this.samples.Dequeue();
                }

                this.Average = this.sampleAccumulator / this.samples.Count;
            }
        }
    }
}
