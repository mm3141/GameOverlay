// <copyright file="SettingsWindow.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using ClickableTransparentOverlay;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.Plugin;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// Creates the MainMenu on the UI.
    /// </summary>
    internal static class SettingsWindow
    {
        private static bool isSettingsWindowVisible = true;
        private static string currentlySelectedPlugin = "Core";
        private static bool showImGuiStyleEditor = false;

        /// <summary>
        /// Initializes the Main Menu.
        /// </summary>
        internal static void InitializeCoroutines()
        {
            CoroutineHandler.Start(SaveGameHelperSettings());
            Core.CoroutinesRegistrar.Add(
                CoroutineHandler.Start(
                    DrawSettingsWindowRenderCoroutine(),
                    "[Settings] Draw Core/Plugin settings",
                    int.MaxValue));
        }

        /// <summary>
        /// Draws the (core/plugins) names as ImGui buttons in a single group.
        /// </summary>
        private static void DrawNames()
        {
            var totalWidthAvailable = ImGui.GetContentRegionAvail().X * 0.2f;
            var buttonSize = new Vector2(totalWidthAvailable, 0);
            ImGui.PushItemWidth(totalWidthAvailable);
            ImGui.BeginGroup();
            bool tmp = true;
            ImGui.Checkbox("##CoreEnableCheckBox", ref tmp);
            ImGui.SameLine();
            if (ImGui.Button("Core##ShowSettingsButton", buttonSize))
            {
                currentlySelectedPlugin = "Core";
            }

            foreach (var pKeyValue in PManager.AllPlugins.ToList())
            {
                var pluginContainer = pKeyValue.Value;
                tmp = pluginContainer.Enable;
                if (ImGui.Checkbox($"##{pKeyValue.Key}EnableCheckbox", ref tmp))
                {
                    pluginContainer.Enable = !pluginContainer.Enable;
                    if (pluginContainer.Enable)
                    {
                        pluginContainer.Plugin.OnEnable(Core.Process.Address != IntPtr.Zero);
                    }
                    else
                    {
                        pluginContainer.Plugin.OnDisable();
                    }

                    PManager.AllPlugins[pKeyValue.Key] = pluginContainer;
                }

                ImGui.SameLine();
                if (ImGui.Button($"{pKeyValue.Key}##ShowSettingsButton", buttonSize))
                {
                    currentlySelectedPlugin = pKeyValue.Key;
                }
            }

            ImGui.PopItemWidth();
            ImGui.EndGroup();
        }

        /// <summary>
        /// Draws the currently selected settings on ImGui.
        /// </summary>
        private static void DrawCurrentlySelectedSettings()
        {
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - (8f * ImGui.GetFontSize()));
            switch (currentlySelectedPlugin)
            {
                case "Core":
                    ImGui.BeginGroup();
                    ImGui.TextWrapped("Developer of this software is not responsible for " +
                        "any loss that may happen due to the usage of this software. Use this " +
                        "software at your own risk.");
                    ImGui.NewLine();
                    ImGui.NewLine();
                    ImGui.TextWrapped($"!!!!IMPORTANT!!!! Please provide below the " +
                        $"Window Settings -> Display Settings -> Scale value. Restart the " +
                        $"Overlay after setting this value.\nExample Values:\n\t100%% -> 1\n\t" +
                        $"125%% -> 1.25\n\t150%% -> 1.50 etc");
                    ImGui.DragFloat("Window Scale", ref Core.GHSettings.WindowScale, 0.25f, 1f, 5f);
                    ImGui.TextWrapped("When GameOverlay press a key in the game, the key " +
                        "has to go to the GGG server for it to work. This process takes " +
                        "time equal to your latency. During this time GameOverlay might " +
                        "press that key again. Set the following timeout value to " +
                        "latency x 2 so this doesn't happen. e.g. for 30ms latency, " +
                        "set it to 60ms.");
                    ImGui.DragInt("Key Timeout", ref Core.GHSettings.KeyPressTimeout, 0.2f, 30, 300);
                    ImGui.TextWrapped("NOTE: (Plugins/Core) Settings are saved automatically " +
                        "when you close the overlay or hide it via F12 button.");
                    ImGui.NewLine();
                    ImGui.Text($"Current Game State: {Core.States.GameCurrentState}");
                    ImGui.NewLine();
                    ImGui.Checkbox("Performance Stats", ref Core.GHSettings.ShowPerfStats);
                    ImGui.Spacing();
                    ImGui.SameLine();
                    ImGui.Spacing();
                    ImGui.SameLine();
                    ImGui.Checkbox("Hide when game is in background", ref Core.GHSettings.HidePerfStatsWhenBg);
                    ImGui.Checkbox("Game UiExplorer", ref Core.GHSettings.ShowGameUiExplorer);
                    ImGui.Checkbox("Data Visualization", ref Core.GHSettings.ShowDataVisualization);
                    ImGui.Checkbox("Show ImGui Editor", ref showImGuiStyleEditor);
                    ImGui.NewLine();
                    if (ImGui.Button("Test Disconnect POE"))
                    {
                        MiscHelper.KillTCPConnectionForProcess((uint)Core.Process.Information.Id);
                    }

                    if (showImGuiStyleEditor)
                    {
                        ImGui.ShowStyleEditor();
                    }

                    ImGui.EndGroup();
                    break;
                default:
                    if (PManager.AllPlugins.TryGetValue(currentlySelectedPlugin, out var pContainer))
                    {
                        ImGui.BeginGroup();
                        pContainer.Plugin.DrawSettings();
                        ImGui.EndGroup();
                    }

                    break;
            }

            ImGui.PopItemWidth();
        }

        /// <summary>
        /// Draws the Settings Window.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> DrawSettingsWindowRenderCoroutine()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnRender);
                if (NativeMethods.IsKeyPressedAndNotTimeout(Core.GHSettings.MainMenuHotKey))
                {
                    isSettingsWindowVisible = !isSettingsWindowVisible;
                    if (!isSettingsWindowVisible)
                    {
                        CoroutineHandler.RaiseEvent(GameHelperEvents.TimeToSaveAllSettings);
                    }
                }

                if (!isSettingsWindowVisible)
                {
                    continue;
                }

                ImGui.SetNextWindowSize(new Vector2(800, 600), ImGuiCond.Appearing);
                var isMainMenuExpanded = ImGui.Begin(
                    "Game Overlay Settings Menu",
                    ref Core.GHSettings.IsOverlayRunning);
                if (!Core.GHSettings.IsOverlayRunning)
                {
                    CoroutineHandler.RaiseEvent(GameHelperEvents.TimeToSaveAllSettings);
                }

                if (!isMainMenuExpanded)
                {
                    ImGui.End();
                    continue;
                }

                DrawNames();
                ImGui.SameLine();
                DrawCurrentlySelectedSettings();
                ImGui.End();
            }
        }

        /// <summary>
        /// Saves the GameHelper settings to disk.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> SaveGameHelperSettings()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.TimeToSaveAllSettings);
                JsonHelper.SafeToFile(Core.GHSettings, State.CoreSettingFile);
            }
        }
    }
}
