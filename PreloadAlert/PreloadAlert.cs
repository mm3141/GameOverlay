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
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;

    /// <summary>
    /// Displays important preload on the screen.
    /// </summary>
    public sealed class PreloadAlert : PCore<PreloadSettings>
    {
        private string path = string.Empty;
        private string displayName = string.Empty;
        private Vector4 color = new Vector4(1f);
        private Vector2 size = new Vector2(90f);
        private ActiveCoroutine onAreaChange;
        private Dictionary<string, PreloadInfo> importantPreloads
            = new Dictionary<string, PreloadInfo>();

        private Dictionary<PreloadInfo, byte> preloadFound
            = new Dictionary<PreloadInfo, byte>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PreloadAlert"/> class.
        /// </summary>
        public PreloadAlert()
        {
        }

        private string PreloadFileName => Path.Join(this.DllDirectory, "preloads.txt");

        private string SettingPathname => Path.Join(this.DllDirectory, "config", "settings.txt");

        /// <summary>
        /// Clear all the important and found preloads and stops the co-routines.
        /// </summary>
        public override void OnDisable()
        {
            this.importantPreloads.Clear();
            this.preloadFound.Clear();
            this.onAreaChange?.Cancel();
            this.onAreaChange = null;
        }

        /// <summary>
        /// Reads the settings and preloads from the disk and
        /// starts this plugin co-routione.
        /// </summary>
        public override void OnEnable()
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

            this.onAreaChange = CoroutineHandler.Start(this.OnAreaChanged());
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
            var flags = ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel;
            ImGui.TextWrapped("You can also lock it by double clicking it. " +
                "However, you can only unlock it from here.");
            ImGui.Checkbox("Lock/Unlock Preload Window: ", ref this.Settings.Locked);
            ImGui.Separator();
            this.size.X = ImGui.GetContentRegionAvail().X;
            ImGui.BeginChild("Add New Preload", this.size, true);
            ImGui.InputText("Path", ref this.path, 200);
            ImGui.InputText("Display Name", ref this.displayName, 50);
            ImGui.ColorEdit4("Color", ref this.color, flags);
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

                var preloadsData = JsonConvert.SerializeObject(
                    this.importantPreloads, Formatting.Indented);
                File.WriteAllText(this.PreloadFileName, preloadsData);
            }

            ImGui.EndChild();
            this.size.Y *= 2;
            ImGui.BeginChild("All Important Preloads", this.size, true);
            foreach (var kv in this.importantPreloads)
            {
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

            this.size.Y /= 2;
            ImGui.EndChild();
        }

        /// <summary>
        /// Draws the Ui for this plugin.
        /// </summary>
        public override void DrawUI()
        {
            string windowName = "Preload Window";
            ImGui.PushStyleColor(ImGuiCol.WindowBg, this.Settings.BackgroundColor);
            if (this.Settings.Locked)
            {
                ImGui.SetNextWindowPos(this.Settings.Pos);
                ImGui.SetNextWindowSize(this.Settings.Size);
                ImGui.Begin(windowName, UiHelper.TransparentWindowFlags);
            }
            else
            {
                ImGui.Begin(windowName, ImGuiWindowFlags.NoSavedSettings);
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
                ImGui.TextColored(new Vector4(.86f, .71f, .36f, 1), "Dummy Preload 9");
                ImGui.ColorEdit4("Background Color", ref this.Settings.BackgroundColor);
                this.Settings.Pos = ImGui.GetWindowPos();
                this.Settings.Size = ImGui.GetWindowSize();
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    this.Settings.Locked = true;
                }
            }

            foreach (var kv in this.preloadFound)
            {
                ImGui.TextColored(kv.Key.Color, kv.Key.DisplayName);
            }

            ImGui.End();
            ImGui.PopStyleColor();
        }

        private IEnumerator<Wait> OnAreaChanged()
        {
            while (true)
            {
                yield return new Wait(RemoteEvents.AreaChanged);
                this.preloadFound.Clear();
                CoroutineHandler.InvokeLater(new Wait(0), () =>
                {
                    foreach (var kv in this.importantPreloads)
                    {
                        if (Core.CurrentAreaLoadedFiles.PathNames.TryGetValue(kv.Key, out var value))
                        {
                            this.preloadFound[kv.Value] = 1;
                        }
                    }
                });
            }
        }

        private struct PreloadInfo
        {
            public string DisplayName;
            public Vector4 Color;
        }
    }
}
