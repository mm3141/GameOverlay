// <copyright file="PreloadAlert.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PreloadAlert
{
    using System;
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
    public sealed class PreloadAlert : PCore<PreloadSettings>
    {
        private const ImGuiColorEditFlags ColorEditFlags = ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel;
        private readonly Dictionary<PreloadInfo, byte> preloadFound = new();
        private Vector4 color = new(1f);
        private string displayName = string.Empty;
        private bool logToDisk = false;
        private Dictionary<string, PreloadInfo> importantPreloads = new();
        private bool isPreloadAlertHovered;
        private ActiveCoroutine onPreloadUpdated;
        private string path = string.Empty;
        private string PreloadFileName => Path.Join(this.DllDirectory, "preloads.txt");
        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");

        /// <summary>
        ///     Clear all the important and found preloads and stops the co-routines.
        /// </summary>
        public override void OnDisable()
        {
            this.importantPreloads.Clear();
            this.preloadFound.Clear();
            this.onPreloadUpdated?.Cancel();
            this.onPreloadUpdated = null;
        }

        /// <summary>
        ///     Reads the settings and preloads from the disk and
        ///     starts this plugin co-routine.
        /// </summary>
        /// <param name="isGameOpened">value indicating whether game is opened or not.</param>
        public override void OnEnable(bool isGameOpened)
        {
            if (File.Exists(this.SettingPathname))
            {
                var content = File.ReadAllText(this.SettingPathname);
                this.Settings = JsonConvert.DeserializeObject<PreloadSettings>(content);
            }

            if (File.Exists(this.PreloadFileName))
            {
                var content = File.ReadAllText(this.PreloadFileName);
                this.importantPreloads = JsonConvert.DeserializeObject<
                    Dictionary<string, PreloadInfo>>(content);
            }

            this.onPreloadUpdated = CoroutineHandler.Start(this.OnPreloadsUpdated());
        }

        /// <summary>
        ///     Save this plugin settings to disk.
        ///     NOTE: it will always lock the preload window before storing.
        /// </summary>
        public override void SaveSettings()
        {
            var lockStatus = this.Settings.Locked;
            this.Settings.Locked = true;
            Directory.CreateDirectory(Path.GetDirectoryName(this.SettingPathname));
            var settingsData = JsonConvert.SerializeObject(this.Settings, Formatting.Indented);
            File.WriteAllText(this.SettingPathname, settingsData);
            this.Settings.Locked = lockStatus;
        }

        /// <summary>
        ///     Draws the settings for this plugin on settings window.
        /// </summary>
        public override void DrawSettings()
        {
            ImGui.Checkbox("Lock/Unlock preload window", ref this.Settings.Locked);
            ImGuiHelper.ToolTip("You can also lock it by double clicking the preload window. " +
                              "However, you can only unlock it from here.");
            ImGui.Checkbox("Hide when locked & not in the game", ref this.Settings.EnableHideUi);
            ImGui.Checkbox("Hide when no preload found", ref this.Settings.HideWindowWhenEmpty);
            ImGui.Checkbox("Hide when in town or hideout", ref this.Settings.HideWhenInTownOrHideout);
            ImGui.Separator();
            ImGui.TextWrapped("If you find something new and wants to add it in the preload " +
                              "you can use Core -> Data Visualization -> CurrentAreaLoadedFiles feature.");
            this.AddNewPreloadBox();
            this.DisplayAllImportantPreloads();
        }

        /// <summary>
        ///     Draws the Ui for this plugin.
        /// </summary>
        public override void DrawUI()
        {
            if (this.Settings.EnableHideUi && this.Settings.Locked &&
                (!Core.Process.Foreground || Core.States.GameCurrentState != GameStateTypes.InGameState))
            {
                return;
            }

            if (this.Settings.HideWindowWhenEmpty && this.preloadFound.Count == 0)
            {
                return;
            }

            var areaDetails = Core.States.InGameStateObject.CurrentWorldInstance.AreaDetails;
            if (this.Settings.HideWhenInTownOrHideout && (areaDetails.IsHideout || areaDetails.IsTown))
            {
                return;
            }

            const string windowName = "Preload Window";
            ImGui.PushStyleColor(ImGuiCol.WindowBg, this.isPreloadAlertHovered ? Vector4.Zero : this.Settings.BackgroundColor);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, this.isPreloadAlertHovered ? 0.0f : 0.5f);

            if (this.Settings.Locked)
            {
                ImGui.SetNextWindowPos(this.Settings.Pos);
                ImGui.SetNextWindowSize(this.Settings.Size);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
                ImGui.Begin(windowName, ImGuiHelper.TransparentWindowFlags);
                ImGui.PopStyleVar();
                this.isPreloadAlertHovered = ImGui.IsMouseHoveringRect(this.Settings.Pos, this.Settings.Pos + this.Settings.Size);
            }
            else
            {
                ImGui.Begin(windowName, ImGuiWindowFlags.NoSavedSettings);
                ImGui.TextColored(new Vector4(.86f, .71f, .36f, 1), "Edit Background Color: ");
                ImGui.SameLine();
                ImGui.ColorEdit4(
                    "Background Color##PreloadAlertBackground",
                    ref this.Settings.BackgroundColor,
                    ColorEditFlags);
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
                this.Settings.Pos = ImGui.GetWindowPos();
                this.Settings.Size = ImGui.GetWindowSize();
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    this.Settings.Locked = true;
                }
            }

            if (!this.isPreloadAlertHovered)
            {
                if (areaDetails.IsHideout)
                {
                    ImGui.Text("Preloads are not updated in hideout.");
                }
                else if (areaDetails.IsTown)
                {
                    ImGui.Text("Preloads are not updated in town.");
                }
                else if (this.Settings.Locked)
                {
                    foreach (var preloadInfo in this.preloadFound.Keys)
                    {
                        ImGui.TextColored(preloadInfo.Color, preloadInfo.DisplayName);
                    }
                }
            }

            ImGui.End();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
        }

        private void AddNewPreloadBox()
        {
            if (ImGui.CollapsingHeader("Add New Preload"))
            {
                ImGui.InputText("Path", ref this.path, 200);
                ImGui.InputText("Display Name", ref this.displayName, 50);
                ImGui.ColorEdit4("Color", ref this.color);
                ImGui.Checkbox("Log to file when preload found in area/zone.", ref this.logToDisk);
                if (ImGui.Button("Add & Save"))
                {
                    this.importantPreloads[this.path] = new(this.displayName, this.color, this.logToDisk);
                    this.path = string.Empty;
                    this.displayName = string.Empty;
                    this.logToDisk = false;
                    this.SaveAllPreloadsToDisk();
                }
            }
        }

        private void DisplayAllImportantPreloads()
        {
            if (ImGui.CollapsingHeader("All Important Preloads (click preload path to edit) (click checkbox to log preload on disk when found)"))
            {
                if (this.importantPreloads.Count == 0)
                {
                    ImGui.Text("No important preload found. Did you forget to copy preload.txt " +
                        "file in the preload alert plugin folder?");
                }

                var tmpShouldLog = false;
                foreach (var (key, preloadInfo) in this.importantPreloads)
                {
                    tmpShouldLog = preloadInfo.LogToDisk;
                    if (ImGui.SmallButton($"Delete##{key}"))
                    {
                        this.importantPreloads.Remove(key);
                        this.SaveAllPreloadsToDisk();
                    }

                    ImGui.SameLine();
                    ImGui.Spacing();
                    ImGui.SameLine();
                    ImGui.Spacing();
                    ImGui.SameLine();
                    if(ImGui.Checkbox($"##logToDisk{key}", ref tmpShouldLog))
                    {
                        this.importantPreloads[key] = new(preloadInfo.DisplayName, preloadInfo.Color, tmpShouldLog);
                        this.SaveAllPreloadsToDisk();
                    }

                    ImGui.SameLine();
                    ImGui.TextColored(preloadInfo.Color, $"{preloadInfo.DisplayName}");
                    ImGui.SameLine();
                    ImGui.Text(" - ");
                    ImGui.SameLine();
                    if (ImGui.Selectable($"{key}"))
                    {
                        this.path = key;
                        this.color = preloadInfo.Color;
                        this.displayName = preloadInfo.DisplayName;
                        this.logToDisk = preloadInfo.LogToDisk;
                    }
                }
            }
        }

        private IEnumerator<Wait> OnPreloadsUpdated()
        {
            var logFilePathname = Path.Join(this.DllDirectory, "preloads_found.log");
            List<string> writeToFile = new();
            while (true)
            {
                yield return new Wait(HybridEvents.PreloadsUpdated);
                this.preloadFound.Clear();
                var areaInfo = $"{Core.States.AreaLoading.CurrentAreaName}, {Core.States.InGameStateObject.CurrentAreaInstance.AreaHash}";
                foreach (var (key, preloadInfo) in this.importantPreloads)
                {
                    if (Core.CurrentAreaLoadedFiles.PathNames.TryGetValue(key, out _))
                    {
                        this.preloadFound[preloadInfo] = 1;
                        if (preloadInfo.LogToDisk)
                        {
                            writeToFile.Add($"{DateTime.Now}, {areaInfo}, {preloadInfo.DisplayName}");
                        }
                    }
                }

                if (writeToFile.Count > 0)
                {
                    File.AppendAllLines(logFilePathname, writeToFile);
                    writeToFile.Clear();
                }
            }
        }

        private void SaveAllPreloadsToDisk()
        {
            var preloadsData = JsonConvert.SerializeObject(this.importantPreloads, Formatting.Indented);
            File.WriteAllText(this.PreloadFileName, preloadsData);
        }

        private struct PreloadInfo
        {
            public string DisplayName;
            public Vector4 Color;
            public bool LogToDisk;

            public PreloadInfo(string name, Vector4 color, bool log)
            {
                this.DisplayName = name;
                this.Color = color;
                this.LogToDisk = log;
            }
        }
    }
}