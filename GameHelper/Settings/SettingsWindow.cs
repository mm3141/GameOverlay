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
    using CoroutineEvents;
    using RemoteEnums;
    using RemoteObjects.Components;
    using GameOffsets.Natives;
    using ImGuiNET;
    using Plugin;
    using Utils;
    using GameOffsets.Objects.States.InGameState;

    /// <summary>
    ///     Creates the MainMenu on the UI.
    /// </summary>
    internal static class SettingsWindow
    {
        private static Vector4 color = new(1f, 1f, 0f, 1f);
        private static bool isOverlayRunningLocal = true;
        private static bool isSettingsWindowVisible = true;

        /// <summary>
        ///     Initializes the Main Menu.
        /// </summary>
        internal static void InitializeCoroutines()
        {
            HideOnStartCheck();
            CoroutineHandler.Start(SaveCoroutine());
            Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(
                RenderCoroutine(),
                "[Settings] Draw Core/Plugin settings",
                int.MaxValue));
        }

        private static void DrawManuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Enable Plugins"))
                {
                    foreach (var container in PManager.Plugins)
                    {
                        var isEnabled = container.Metadata.Enable;
                        if (ImGui.Checkbox($"{container.Name}", ref isEnabled))
                        {
                            container.Metadata.Enable = !container.Metadata.Enable;
                            if (container.Metadata.Enable)
                            {
                                container.Plugin.OnEnable(Core.Process.Address != IntPtr.Zero);
                            }
                            else
                            {
                                container.Plugin.OnDisable();
                            }
                        }
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem("Donate (捐)"))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = "https://www.paypal.com/paypalme/Ghelper",
                        UseShellExecute = true
                    });
                }

                ImGui.EndMenuBar();
            }
        }

        private static void DrawTabs()
        {
            if (ImGui.BeginTabBar("pluginsTabBar", ImGuiTabBarFlags.AutoSelectNewTabs | ImGuiTabBarFlags.Reorderable))
            {
                if (ImGui.BeginTabItem("Core"))
                {
                    DrawCoreSettings();
                    ImGui.EndTabItem();
                }

                foreach (var container in PManager.Plugins)
                {
                    if (container.Metadata.Enable && ImGui.BeginTabItem(container.Name))
                    {
                        container.Plugin.DrawSettings();
                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }
        }

        /// <summary>
        ///     Draws the currently selected settings on ImGui.
        /// </summary>
        private static void DrawCoreSettings()
        {
            ImGui.PushTextWrapPos(ImGui.GetContentRegionMax().X);
            ImGui.TextColored(color, "This is a free software, only use https://ownedcore.com to " +
                "download it. Do not buy from the fake sellers or websites.");
            ImGui.TextColored(color, "请不要花钱购买本软件，否则你就是个傻逼。这是一个免费软件。" +
                "不要从假卖家那里购买。前往 https://ownedcore.com 免费下载。");
            ImGui.NewLine();
            ImGui.TextColored(Vector4.One, "Developer of this software is not responsible for " +
                              "any loss that may happen due to the usage of this software. Use this " +
                              "software at your own risk.");
            ImGui.NewLine();
            ImGui.TextColored(Vector4.One, "All Settings (including plugins) are saved automatically " +
                  $"when you close the overlay or hide it via {Core.GHSettings.MainMenuHotKey} button.");
            ImGui.NewLine();
            ImGui.PopTextWrapPos();
            ImGui.DragInt("Nearby Monster Range", ref Core.GHSettings.NearbyMeaning, 1f, 1, 200);
            DrawNearbyMonsterRange();
            ImGuiHelper.ToolTip("If you are in the game and hovering over this, " +
                "it will draw your current nearby range on the screen.");
            ImGui.DragInt("Key Timeout", ref Core.GHSettings.KeyPressTimeout, 0.2f, 30, 300);
            ImGuiHelper.ToolTip("When GameOverlay press a key in the game, the key " +
                "has to go to the GGG server for it to work. This process takes " +
                "time equal to your latency x 2. During this time GameOverlay might " +
                "press that key again. Set the key timeout value to latency x 2 so " +
                "this doesn't happen. e.g. for 30ms latency, set it to 60ms.");
            ImGui.NewLine();
            ImGui.Text($"Current Game State: {Core.States.GameCurrentState}");
            ImGui.NewLine();
            ImGuiHelper.NonContinuousEnumComboBox("Settings Window Key", ref Core.GHSettings.MainMenuHotKey);
            ImGui.NewLine();
            ImGui.Checkbox("Performance Stats", ref Core.GHSettings.ShowPerfStats);
            ImGui.Spacing();
            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();
            ImGui.Checkbox("Hide when game is in background", ref Core.GHSettings.HidePerfStatsWhenBg);
            ImGui.Checkbox("Game UiExplorer (GE)", ref Core.GHSettings.ShowGameUiExplorer);
            ImGui.Checkbox("Data Visualization (DV)", ref Core.GHSettings.ShowDataVisualization);
            ImGui.Checkbox("Disable entity processing when in town or hideout",
                ref Core.GHSettings.DisableEntityProcessingInTownOrHideout);
            ImGui.Checkbox("Hide overlay settings upon start", ref Core.GHSettings.HideSettingWindowOnStart);
            ImGui.Checkbox("Close GameHelper when Game Exit", ref Core.GHSettings.CloseWhenGameExit);
            ImGui.NewLine();
            ChangeFontWidget();
            ImGui.NewLine();
            if (ImGui.Button("Test Disconnect POE"))
            {
                MiscHelper.KillTCPConnectionForProcess(Core.Process.Pid);
            }
#if DEBUG
            if (ImGui.Button("Reload Plugins"))
            {
                PManager.CleanUpAllPlugins();
                PManager.InitializePlugins();
            }
#endif
        }

        /// <summary>
        ///     Draws the ImGui widget for changing fonts.
        /// </summary>
        private static void ChangeFontWidget()
        {
            if (ImGui.CollapsingHeader("Change Fonts"))
            {
                ImGui.InputText("Pathname", ref Core.GHSettings.FontPathName, 300);
                ImGui.DragInt("Size", ref Core.GHSettings.FontSize, 0.1f, 13, 40);
                var languageChanged = ImGuiHelper.EnumComboBox("Language", ref Core.GHSettings.FontLanguage);
                var customLanguage = ImGui.InputText("Custom Glyph Ranges", ref Core.GHSettings.FontCustomGlyphRange, 100);
                ImGuiHelper.ToolTip("This is advance level feature. Do not modify this if you don't know what you are doing. " +
                    "Example usage:- If you have downloaded and pointed to the ArialUnicodeMS.ttf font, you can use " +
                    "0x0020, 0xFFFF, 0x00 text in this field to load all of the font texture in ImGui. Note the 0x00" +
                    " as the last item in the range.");
                if (languageChanged)
                {
                    Core.GHSettings.FontCustomGlyphRange = string.Empty;
                }

                if (customLanguage)
                {
                    Core.GHSettings.FontLanguage = FontGlyphRangeType.English;
                }

                if (ImGui.Button("Apply Changes"))
                {
                    if (MiscHelper.TryConvertStringToImGuiGlyphRanges(Core.GHSettings.FontCustomGlyphRange, out var glyphranges))
                    {
                        Core.Overlay.ReplaceFont(
                            Core.GHSettings.FontPathName,
                            Core.GHSettings.FontSize,
                            glyphranges);
                    }
                    else
                    {
                        Core.Overlay.ReplaceFont(
                            Core.GHSettings.FontPathName,
                            Core.GHSettings.FontSize,
                            Core.GHSettings.FontLanguage);
                    }
                }
            }
        }

        /// <summary>
        ///     Draws the closing confirmation popup on ImGui.
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
        ///     Hides the overlay on startup.
        /// </summary>
        private static void HideOnStartCheck()
        {
            if (Core.GHSettings.HideSettingWindowOnStart)
            {
                isSettingsWindowVisible = false;
            }
        }

        /// <summary>
        ///     Draws the Settings Window.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> RenderCoroutine()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnRender);
                if (NativeMethods.IsKeyPressedAndNotTimeout((int)Core.GHSettings.MainMenuHotKey))
                {
                    isSettingsWindowVisible = !isSettingsWindowVisible;
                    ImGui.GetIO().WantCaptureMouse = true;
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

                DrawManuBar();
                DrawTabs();
                ImGui.End();
            }
        }

        /// <summary>
        ///     Saves the GameHelper settings to disk.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> SaveCoroutine()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.TimeToSaveAllSettings);
                JsonHelper.SafeToFile(Core.GHSettings, State.CoreSettingFile);
            }
        }

        /// <summary>
        ///     Draws the nearby monster range on screen.
        /// </summary>
        private static void DrawNearbyMonsterRange()
        {
            var iGS = Core.States.InGameStateObject;
            if (ImGui.IsItemHovered() &&
                Core.States.GameCurrentState == GameStateTypes.InGameState &&
                iGS.CurrentAreaInstance.Player.GetComp<Render>(out var r))
            {
                foreach (var angle in Enumerable.Range(0, 360))
                {
                    Vector2 GetScreenCoord(int i)
                    {
                        var gridPoint = new Vector2(r.GridPosition.X, r.GridPosition.Y) +
                                        new Vector2(
                                            (float)(Math.Cos(Math.PI / 180 * i) * Core.GHSettings.NearbyMeaning),
                                            (float)(Math.Sin(Math.PI / 180 * i) * Core.GHSettings.NearbyMeaning));

                        var height = r.TerrainHeight;
                        try
                        {
                            height = Core.States.InGameStateObject.CurrentAreaInstance.GridHeightData[(int)gridPoint.Y][(int)gridPoint.X];
                        }
                        catch (Exception)
                        {
                        }

                        var screenCoord = Core.States.InGameStateObject.CurrentWorldInstance.WorldToScreen(
                            new StdTuple3D<float>
                            {
                                X = gridPoint.X * TileStructure.TileToWorldConversion / TileStructure.TileToGridConversion,
                                Y = gridPoint.Y * TileStructure.TileToWorldConversion / TileStructure.TileToGridConversion,
                                Z = height
                            });
                        return screenCoord;
                    }

                    var p1 = GetScreenCoord(angle);
                    var p2 = GetScreenCoord(angle + 1);
                    ImGui.GetBackgroundDrawList().AddLine(p1, p2, ImGuiHelper.Color(255, 0, 0, 255));
                }
            }
        }
    }
}
