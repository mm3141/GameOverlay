// <copyright file="DataVisualization.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Ui
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// Visualize the <see cref="RemoteObjects"/> and other miscellaneous data.
    /// </summary>
    public static class DataVisualization
    {
        /// <summary>
        /// Initializes the co-routines.
        /// </summary>
        internal static void InitializeCoroutines()
        {
            CoroutineHandler.Start(DataVisualizationRenderCoRoutine());
        }

        /// <summary>
        /// Draws the window for Data Visualization.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> DataVisualizationRenderCoRoutine()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnRender);
                if (!Core.GHSettings.ShowDataVisualization)
                {
                    continue;
                }

                if (ImGui.Begin("Data Visualization", ref Core.GHSettings.ShowDataVisualization))
                {
                    if (ImGui.CollapsingHeader("Settings"))
                    {
                        var fields = Core.GHSettings.GetType().GetFields().ToList();
                        for (int i = 0; i < fields.Count; i++)
                        {
                            FieldInfo field = fields[i];
                            ImGui.Text($"{field.Name}: {field.GetValue(Core.GHSettings)}");
                        }
                    }

                    if (ImGui.CollapsingHeader("Game Process"))
                    {
                        if (Core.Process.Address != IntPtr.Zero)
                        {
                            UiHelper.IntPtrToImGui("Base Address", Core.Process.Address);
                            ImGui.Text($"Process: {Core.Process.Information}");
                            ImGui.Text($"WindowArea: {Core.Process.WindowArea}");
                            ImGui.Text($"Foreground: {Core.Process.Foreground}");
                            if (ImGui.TreeNode("Static Addresses"))
                            {
                                foreach (var saddr in Core.Process.StaticAddresses)
                                {
                                    UiHelper.IntPtrToImGui(saddr.Key, saddr.Value);
                                }

                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.Text($"Game not found.");
                        }
                    }

                    Core.RemoteObjectsToImGuiCollapsingHeader();
                }

                ImGui.End();
            }
        }
    }
}
