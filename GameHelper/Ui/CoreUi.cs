// <copyright file="CoreUi.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Ui
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// Draws the Ui for the Core (GameHelper) features.
    /// </summary>
    public static class CoreUi
    {
        private static Vector2 devTreeWindowMin = new Vector2(800, 600);
        private static Vector2 devTreeWindowMax = new Vector2(2560, 1440);

        /// <summary>
        /// Initializes the Core Ui.
        /// </summary>
        internal static void InitializeCoroutines()
        {
            CoroutineHandler.Start(DrawCoreFeaturesUi());
        }

        /// <summary>
        /// Draws the window to display the perf stats.
        /// </summary>
        internal static void DrawPerfStats()
        {
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

        /// <summary>
        /// Draws the window to display the DevTree.
        /// </summary>
        internal static void DrawDevTree()
        {
            if (Core.GHSettings.ShowDevTree)
            {
                ImGui.SetNextWindowSizeConstraints(devTreeWindowMin, devTreeWindowMax);
                ImGui.Begin(
                    "Dev Tree",
                    ref Core.GHSettings.ShowDevTree,
                    ImGuiWindowFlags.AlwaysAutoResize);

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
                ImGui.End();
            }
        }

        /// <summary>
        /// Ui for the Core (GameHelper) features.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> DrawCoreFeaturesUi()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnRender);
                DrawPerfStats();
                DrawDevTree();
            }
        }
    }
}
