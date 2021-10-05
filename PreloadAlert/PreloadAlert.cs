// <copyright file="PreloadAlert.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PreloadAlert
{
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
    /// Displays important preload on the screen.
    /// </summary>
    public sealed class PreloadAlert : PCore<PreloadSettings>
    {
        private readonly ImGuiColorEditFlags colorEditflags
            = ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel;

        private readonly Dictionary<PreloadInfo, byte> preloadFound
            = new Dictionary<PreloadInfo, byte>();

        private string path = string.Empty;
        private string displayName = string.Empty;
        private Vector4 color = new Vector4(1f);

        private ActiveCoroutine onPreloadUpdated;
        private Dictionary<string, PreloadInfo> importantPreloads
            = new Dictionary<string, PreloadInfo>();

        private bool isPreloadAlertHovered = false;

        private string PreloadFileName => Path.Join(this.DllDirectory, "preloads.txt");

        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");

        /// <summary>
        /// Clear all the important and found preloads and stops the co-routines.
        /// </summary>
        public override void OnDisable()
        {
            this.importantPreloads.Clear();
            this.preloadFound.Clear();
            this.onPreloadUpdated?.Cancel();
            this.onPreloadUpdated = null;
        }

        /// <summary>
        /// Reads the settings and preloads from the disk and
        /// starts this plugin co-routione.
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
        /// Save this plugin settings to disk.
        /// NOTE: it will always lock the preload window before storing.
        /// </summary>
        public override void SaveSettings()
        {
            var lockstatus = this.Settings.Locked;
            this.Settings.Locked = true;
            Directory.CreateDirectory(Path.GetDirectoryName(this.SettingPathname));
            var settingsData = JsonConvert.SerializeObject(this.Settings, Formatting.Indented);
            File.WriteAllText(this.SettingPathname, settingsData);
            this.Settings.Locked = lockstatus;
        }

        /// <summary>
        /// Draws the settings for this plugin on settings window.
        /// </summary>
        public override void DrawSettings()
        {
            ImGui.TextWrapped("You can also lock it by double clicking the preload window. " +
                "However, you can only unlock it from here.");
            ImGui.Checkbox("Lock/Unlock preload window", ref this.Settings.Locked);
            ImGui.Checkbox("Hide when locked & not in the game", ref this.Settings.EnableHideUi);
            ImGui.Checkbox("Hide when no preload found", ref this.Settings.HideWindowWhenEmpty);
            ImGui.Separator();
            ImGui.TextWrapped("If you find something new and wants to add it in the preload " +
                "you can use Core -> Data Visulization -> CurrentAreaLoadedFiles feature.");
            this.AddNewPreloadBox();
            this.DisplayAllImportantPreloads();
        }

        /// <summary>
        /// Draws the Ui for this plugin.
        /// </summary>
        public override void DrawUI()
        {
            if (this.Settings.EnableHideUi &&
                this.Settings.Locked &&
                (!Core.Process.Foreground ||
                Core.States.GameCurrentState != GameStateTypes.InGameState))
            {
                return;
            }

            if (this.Settings.HideWindowWhenEmpty && this.preloadFound.Count == 0)
            {
                return;
            }

            var areaDetails = Core.States.InGameStateObject.CurrentAreaInstance.AreaDetails;
            string windowName = "Preload Window";
            ImGui.PushStyleColor(ImGuiCol.WindowBg, this.isPreloadAlertHovered ? Vector4.Zero : this.Settings.BackgroundColor);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, this.isPreloadAlertHovered ? 0.0f : 0.5f);

            if (this.Settings.Locked)
            {
                ImGui.SetNextWindowPos(this.Settings.Pos);
                ImGui.SetNextWindowSize(this.Settings.Size);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
                ImGui.Begin(windowName, UiHelper.TransparentWindowFlags);
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
                    this.colorEditflags);
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
                    ImGui.Text($"Preloads are not updated in hideout.");
                }
                else if (areaDetails.IsTown)
                {
                    ImGui.Text($"Preloads are not updated in town.");
                }
                else if (this.Settings.Locked)
                {
                    foreach (var kv in this.preloadFound)
                    {
                        ImGui.TextColored(kv.Key.Color, kv.Key.DisplayName);
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
                ImGui.ColorEdit4("Color", ref this.color, this.colorEditflags);
                ImGui.SameLine();
                if (ImGui.Button("Add & Save"))
                {
                    this.importantPreloads[this.path] = new PreloadInfo()
                    {
                        Color = this.color,
                        DisplayName = this.displayName,
                    };

                    this.path = string.Empty;
                    this.displayName = string.Empty;
                    this.SaveAllPreloadsToDisk();
                }
            }
        }

        private void DisplayAllImportantPreloads()
        {
            if (ImGui.CollapsingHeader("All Important Preloads (click path to edit)"))
            {
                ImGui.Text("Click on the display name to edit that preload.");
                foreach (var kv in this.importantPreloads)
                {
                    if (ImGui.SmallButton($"Delete##{kv.Key}"))
                    {
                        this.importantPreloads.Remove(kv.Key);
                        this.SaveAllPreloadsToDisk();
                    }

                    ImGui.SameLine();
                    ImGui.TextColored(kv.Value.Color, $"{kv.Value.DisplayName}");
                    ImGui.SameLine();
                    ImGui.Text(" - ");
                    ImGui.SameLine();
                    if (ImGui.Selectable($"{kv.Key}"))
                    {
                        this.path = kv.Key;
                        this.color = kv.Value.Color;
                        this.displayName = kv.Value.DisplayName;
                    }
                }
            }
        }

        private IEnumerator<Wait> OnPreloadsUpdated()
        {
            while (true)
            {
                yield return new Wait(HybridEvents.PreloadsUpdated);
                this.preloadFound.Clear();
                foreach (var kv in this.importantPreloads)
                {
                    if (Core.CurrentAreaLoadedFiles.PathNames.TryGetValue(kv.Key, out var _))
                    {
                        this.preloadFound[kv.Value] = 1;
                    }
                }
            }
        }

        private void SaveAllPreloadsToDisk()
        {
            var preloadsData = JsonConvert.SerializeObject(
                this.importantPreloads, Formatting.Indented);
            File.WriteAllText(this.PreloadFileName, preloadsData);
        }

        private struct PreloadInfo
        {
            public string DisplayName;
            public Vector4 Color;
        }
    }
}
