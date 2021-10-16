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
        private static string currentlySelectedPlugin = "Core";

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
                        "software at your own risk. This is a free software, do not pay anyone " +
                        "to get it.");
                    ImGui.NewLine();
                    ImGui.NewLine();
                    ImGui.TextWrapped($"!!!!IMPORTANT!!!! Please provide below the " +
                        $"Window Settings -> Display Settings -> Scale value. Restart the " +
                        $"Overlay after setting this value.\nExample Values:\n\t100%% -> 1\n\t" +
                        $"125%% -> 1.25\n\t150%% -> 1.50 etc\nThis is used to align the texture " +
                        $"drawn on the screen (e.g. Radar, UiElements border) with the InGame " +
                        $"UiElements and Map. It does not resize text or windows.");
                    ImGui.DragFloat("Window Scale", ref Core.GHSettings.WindowScale, 0.25f, 1f, 5f);
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
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 3);
                    UiHelper.NonContinuousEnumComboBox("Select Show/Hide Key", ref Core.GHSettings.MainMenuHotKey);
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 3);
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
                    ImGui.Checkbox("Game UiExplorer", ref Core.GHSettings.ShowGameUiExplorer);
                    ImGui.Checkbox("Data Visualization", ref Core.GHSettings.ShowDataVisualization);
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
                    "Game Overlay Settings Menu",
                    ref isOverlayRunningLocal);

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
