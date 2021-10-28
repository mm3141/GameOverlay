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
    using GameOffsets;
    using ImGuiNET;

    /// <summary>
    /// Creates the MainMenu on the UI.
    /// </summary>
    internal static class SettingsWindow
    {
        private static bool isOverlayRunningLocal = true;
        private static bool isSettingsWindowVisible = true;

        /// <summary>
        /// Initializes the Main Menu.
        /// </summary>
        internal static void InitializeCoroutines()
        {
            CoroutineHandler.Start(LoadCurrentlyConfiguredFont());
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
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Enable/Disable Plugins"))
                {
                    foreach (var pKeyValue in PManager.AllPlugins.ToList())
                    {
                        var pluginContainer = pKeyValue.Value;
                        var isEnabled = pluginContainer.Enable;
                        if (ImGui.Checkbox($"{pKeyValue.Key}", ref isEnabled))
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
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        /// <summary>
        /// Draws the currently selected settings on ImGui.
        /// </summary>
        private static void DrawCurrentlySelectedSettings()
        {
            if (ImGui.BeginTabBar("pluginsTabBar", ImGuiTabBarFlags.AutoSelectNewTabs | ImGuiTabBarFlags.Reorderable))
            {
                if (ImGui.BeginTabItem("Core"))
                {
                    DrawCoreSettings();
                    ImGui.EndTabItem();
                }

                foreach (var pKeyValue in PManager.AllPlugins.ToList())
                {
                    if (ImGui.BeginTabItem(pKeyValue.Key))
                    {
                        pKeyValue.Value.Plugin.DrawSettings();
                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }
        }

        /// <summary>
        /// Draws the core settings to ImGui.
        /// </summary>
        private static void DrawCoreSettings()
        {
            ImGui.TextWrapped("Developer of this software is not responsible for " +
                "any loss that may happen due to the usage of this software. Use this " +
                "software at your own risk. This is a free software, do not pay anyone " +
                "to get it.");
            ImGui.NewLine();
            ImGui.TextWrapped("When GameOverlay press a key in the game, the key " +
                "has to go to the GGG server for it to work. This process takes " +
                "time equal to your latency. During this time GameOverlay might " +
                "press that key again. Set the following timeout value to " +
                "latency x 2 so this doesn't happen. e.g. for 30ms latency, " +
                "set it to 60ms.");
            ImGui.DragInt("Key Timeout", ref Core.GHSettings.KeyPressTimeout, 0.2f, 30, 300);
            ImGui.TextWrapped("NOTE: (Plugins/Core) Settings are saved automatically " +
                $"when you close the overlay or hide it via {Core.GHSettings.MainMenuHotKey} button.");
            ImGui.NewLine();
            ImGui.Text($"Current Game State: {Core.States.GameCurrentState}");
            ImGui.NewLine();
            UiHelper.NonContinuousEnumComboBox("Select Show/Hide Key", ref Core.GHSettings.MainMenuHotKey);
            if (ImGui.DragInt("Select Font", ref Core.GHSettings.CurrentlySelectedFont, 0.1f, 0, Core.Overlay.Fonts.Length - 1))
            {
                SetCurrentlyConfiguredFont();
            }

            ImGui.Checkbox("Performance Stats", ref Core.GHSettings.ShowPerfStats);
            ImGui.Spacing();
            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();
            ImGui.Checkbox("Hide when game is in background", ref Core.GHSettings.HidePerfStatsWhenBg);
            ImGui.Checkbox("Game UiExplorer (GE)", ref Core.GHSettings.ShowGameUiExplorer);
            ImGui.Checkbox("Data Visualization (DV)", ref Core.GHSettings.ShowDataVisualization);
            ImGui.Checkbox("Do not save entities outside the network bubble", ref Core.GHSettings.RemoveAllInvalidEntities);
            ImGui.Checkbox("Disable entity processing when in town or hideout", ref Core.GHSettings.DisableEntityProcessingInTownOrHideout);
            ImGui.NewLine();
            if (ImGui.Button("Test Disconnect POE"))
            {
                MiscHelper.KillTCPConnectionForProcess(Core.Process.Pid);
            }

            ImGui.NewLine();
            if (ImGui.CollapsingHeader("Thank you for your support! Means a lot to me!"))
            {
                foreach (var c in GameProcessDetails.Contributors)
                {
                    ImGui.TextColored(new Vector4(212 / 255f, 175 / 255f, 55 / 255f, 255 / 255f), c);
                }
            }
        }

        /// <summary>
        /// Draws the closing confirmation popup on ImGui.
        /// </summary>
        private static void DrawConfirmationPopup()
        {
            ImGui.SetNextWindowPos(new Vector2(Core.Overlay.Size.X / 3f, Core.Overlay.Size.Y / 3f));
            if (ImGui.BeginPopup("GameHelperCloseConfirmation"))
            {
                ImGui.Text("Do you want to quit the GameHelper overlay?");
                ImGui.Separator();
                if (ImGui.Button("Yes", new Vector2(ImGui.GetContentRegionAvail().X / 2f, ImGui.GetTextLineHeight() * 2)))
                {
                    Core.GHSettings.IsOverlayRunning = false;
                    ImGui.CloseCurrentPopup();
                    isOverlayRunningLocal = true;
                }

                ImGui.SameLine();
                if (ImGui.Button("No", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetTextLineHeight() * 2)))
                {
                    ImGui.CloseCurrentPopup();
                    isOverlayRunningLocal = true;
                }

                ImGui.EndPopup();
            }
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
                if (NativeMethods.IsKeyPressedAndNotTimeout((int)Core.GHSettings.MainMenuHotKey))
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

                ImGui.SetNextWindowSizeConstraints(new Vector2(800, 600), Vector2.One * float.MaxValue);
                var isMainMenuExpanded = ImGui.Begin(
                    $"Game Overlay Settings [ {Core.GetVersion()} ]",
                    ref isOverlayRunningLocal,
                    ImGuiWindowFlags.MenuBar);

                if (!isOverlayRunningLocal)
                {
                    ImGui.OpenPopup("GameHelperCloseConfirmation");
                }

                DrawConfirmationPopup();
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

        private static IEnumerator<Wait> LoadCurrentlyConfiguredFont()
        {
            yield return new Wait(GameHelperEvents.OnRender);
            SetCurrentlyConfiguredFont();
        }

        private static void SetCurrentlyConfiguredFont()
        {
            unsafe
            {
                ImGui.GetIO().NativePtr->FontDefault = Core.Overlay.Fonts[Core.GHSettings.CurrentlySelectedFont];
            }
        }
    }
}
