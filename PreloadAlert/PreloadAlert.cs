// <copyright file="PreloadAlert.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PreloadAlert {
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using Coroutine;
    using GameHelper;
    using GameHelper.CoroutineEvents;
    using GameHelper.Plugin;
    using GameHelper.RemoteEnums;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;

    /// <summary>
    ///     Displays important preload on the screen.
    /// </summary>
    public sealed class PreloadAlert : PCore<PreloadSettings> {
        private readonly ImGuiColorEditFlags colorEditflags
            = ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel;

        private readonly Dictionary<PreloadInfo, byte> preloadFound = new();
        private Vector4 color = new(1f);
        private string displayName = string.Empty;
        private Dictionary<string, PreloadInfo> importantPreloads = new();

        private bool isPreloadAlertHovered;

        private ActiveCoroutine onPreloadUpdated;

        private string path = string.Empty;

        private string PreloadFileName => Path.Join(DllDirectory, "preloads.txt");

        private string SettingPathname => Path.Join(DllDirectory, "config", "settings.txt");

        /// <summary>
        ///     Clear all the important and found preloads and stops the co-routines.
        /// </summary>
        public override void OnDisable() {
            importantPreloads.Clear();
            preloadFound.Clear();
            onPreloadUpdated?.Cancel();
            onPreloadUpdated = null;
        }

        /// <summary>
        ///     Reads the settings and preloads from the disk and
        ///     starts this plugin co-routione.
        /// </summary>
        /// <param name="isGameOpened">value indicating whether game is opened or not.</param>
        public override void OnEnable(bool isGameOpened) {
            if (File.Exists(SettingPathname)) {
                var content = File.ReadAllText(SettingPathname);
                Settings = JsonConvert.DeserializeObject<PreloadSettings>(content);
            }

            if (File.Exists(PreloadFileName)) {
                var content = File.ReadAllText(PreloadFileName);
                importantPreloads = JsonConvert.DeserializeObject<
                    Dictionary<string, PreloadInfo>>(content);
            }

            onPreloadUpdated = CoroutineHandler.Start(OnPreloadsUpdated());
        }

        /// <summary>
        ///     Save this plugin settings to disk.
        ///     NOTE: it will always lock the preload window before storing.
        /// </summary>
        public override void SaveSettings() {
            var lockStatus = Settings.Locked;
            Settings.Locked = true;
            Directory.CreateDirectory(Path.GetDirectoryName(SettingPathname));
            var settingsData = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(SettingPathname, settingsData);
            Settings.Locked = lockStatus;
        }

        /// <summary>
        ///     Draws the settings for this plugin on settings window.
        /// </summary>
        public override void DrawSettings() {
            ImGui.TextWrapped("You can also lock it by double clicking the preload window. " +
                              "However, you can only unlock it from here.");
            ImGui.Checkbox("Lock/Unlock preload window", ref Settings.Locked);
            ImGui.Checkbox("Hide when locked & not in the game", ref Settings.EnableHideUi);
            ImGui.Checkbox("Hide when no preload found", ref Settings.HideWindowWhenEmpty);
            ImGui.Checkbox("Hide when in town or hideout", ref Settings.HideWhenInTownOrHideout);
            ImGui.Separator();
            ImGui.TextWrapped("If you find something new and wants to add it in the preload " +
                              "you can use Core -> Data Visulization -> CurrentAreaLoadedFiles feature.");
            AddNewPreloadBox();
            DisplayAllImportantPreloads();
        }

        /// <summary>
        ///     Draws the Ui for this plugin.
        /// </summary>
        public override void DrawUI() {
            if (Settings.EnableHideUi &&
                Settings.Locked &&
                (!Core.Process.Foreground ||
                 Core.States.GameCurrentState != GameStateTypes.InGameState)) {
                return;
            }

            if (Settings.HideWindowWhenEmpty && preloadFound.Count == 0) {
                return;
            }

            var areaDetails = Core.States.InGameStateObject.CurrentAreaInstance.AreaDetails;
            if (Settings.HideWhenInTownOrHideout && (areaDetails.IsHideout || areaDetails.IsTown)) {
                return;
            }

            var windowName = "Preload Window";
            ImGui.PushStyleColor(ImGuiCol.WindowBg, isPreloadAlertHovered ? Vector4.Zero : Settings.BackgroundColor);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, isPreloadAlertHovered ? 0.0f : 0.5f);

            if (Settings.Locked) {
                ImGui.SetNextWindowPos(Settings.Pos);
                ImGui.SetNextWindowSize(Settings.Size);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
                ImGui.Begin(windowName, UiHelper.TransparentWindowFlags);
                ImGui.PopStyleVar();
                isPreloadAlertHovered = ImGui.IsMouseHoveringRect(Settings.Pos, Settings.Pos + Settings.Size);
            }
            else {
                ImGui.Begin(windowName, ImGuiWindowFlags.NoSavedSettings);
                ImGui.TextColored(new Vector4(.86f, .71f, .36f, 1), "Edit Background Color: ");
                ImGui.SameLine();
                ImGui.ColorEdit4(
                    "Background Color##PreloadAlertBackground",
                    ref Settings.BackgroundColor,
                    colorEditflags);
                ImGui.TextColored(new Vector4(1, 1, 1, 1), "Dummy Preload 1");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(0, 1, 1, 1), "Dummy Preload 2");
                ImGui.TextColored(new Vector4(1, 0, 1, 1), "Dummy Preload 3");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(1, 1, 0, 1), "Dummy Preload 4");
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Dummy Preload 5");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(0, 1, 0, 1), "Dummy Preload 6");
                ImGui.TextColored(new Vector4(0, 0, 1, 1), "Dummy Preload 7");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(0, 0, 0, 1), "Dummy Preload 8");
                Settings.Pos = ImGui.GetWindowPos();
                Settings.Size = ImGui.GetWindowSize();
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) {
                    Settings.Locked = true;
                }
            }

            if (!isPreloadAlertHovered) {
                if (areaDetails.IsHideout) {
                    ImGui.Text("Preloads are not updated in hideout.");
                }
                else if (areaDetails.IsTown) {
                    ImGui.Text("Preloads are not updated in town.");
                }
                else if (Settings.Locked) {
                    foreach (var kv in preloadFound) {
                        ImGui.TextColored(kv.Key.Color, kv.Key.DisplayName);
                    }
                }
            }

            ImGui.End();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
        }

        private void AddNewPreloadBox() {
            if (ImGui.CollapsingHeader("Add New Preload")) {
                ImGui.InputText("Path", ref path, 200);
                ImGui.InputText("Display Name", ref displayName, 50);
                ImGui.ColorEdit4("Color", ref color, colorEditflags);
                ImGui.SameLine();
                if (ImGui.Button("Add & Save")) {
                    importantPreloads[path] = new PreloadInfo {
                        Color = color,
                        DisplayName = displayName
                    };

                    path = string.Empty;
                    displayName = string.Empty;
                    SaveAllPreloadsToDisk();
                }
            }
        }

        private void DisplayAllImportantPreloads() {
            if (ImGui.CollapsingHeader("All Important Preloads (click path to edit)")) {
                ImGui.Text("Click on the display name to edit that preload.");
                foreach (var kv in importantPreloads) {
                    if (ImGui.SmallButton($"Delete##{kv.Key}")) {
                        importantPreloads.Remove(kv.Key);
                        SaveAllPreloadsToDisk();
                    }

                    ImGui.SameLine();
                    ImGui.TextColored(kv.Value.Color, $"{kv.Value.DisplayName}");
                    ImGui.SameLine();
                    ImGui.Text(" - ");
                    ImGui.SameLine();
                    if (ImGui.Selectable($"{kv.Key}")) {
                        path = kv.Key;
                        color = kv.Value.Color;
                        displayName = kv.Value.DisplayName;
                    }
                }
            }
        }

        private IEnumerator<Wait> OnPreloadsUpdated() {
            while (true) {
                yield return new Wait(HybridEvents.PreloadsUpdated);
                preloadFound.Clear();
                foreach (var (key, preloadInfo) in importantPreloads) {
                    if (Core.CurrentAreaLoadedFiles.PathNames.TryGetValue(key, out _)) {
                        preloadFound[preloadInfo] = 1;
                    }
                }
            }
        }

        private void SaveAllPreloadsToDisk() {
            var preloadsData = JsonConvert.SerializeObject(
                importantPreloads, Formatting.Indented);
            File.WriteAllText(PreloadFileName, preloadsData);
        }

        private struct PreloadInfo {
            public string DisplayName;
            public Vector4 Color;
        }
    }
}