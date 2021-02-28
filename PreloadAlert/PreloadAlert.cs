namespace PreloadAlert
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using Coroutine;
    using GameHelper;
    using GameHelper.Plugin;
    using GameHelper.RemoteObjects;
    using ImGuiNET;
    using Newtonsoft.Json;

    /// <summary>
    /// TODO: Save all preloads to disk.
    /// File Name: AreaName + hash
    /// If file exists, logs on preload window and don't overwrite.
    /// Sort before dumping
    /// Maybe do in a different thread.
    /// make a Diary UI too.
    /// </summary>
    public sealed class PreloadAlert : PCore<PreloadSettings>
    {
        private const ImGuiWindowFlags overlayEnabled = ImGuiWindowFlags.NoInputs |
            ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoTitleBar;

        private string ERRORMSG = string.Empty;

        private string SettingPathname => Path.Join(DllDirectory, "config", "settings.txt");

        private string PreloadPathname => Path.Join(DllDirectory, "preloads.txt");

        private Dictionary<string, PreloadInfo> importantPreloadsInArea
            = new Dictionary<string, PreloadInfo>();

        private Dictionary<string, PreloadInfo> preloadsFromFile
            = new Dictionary<string, PreloadInfo>();

        private HashSet<string> allPreloadsOfArea = new HashSet<string>();
        private string DebugDirectory;
        private string DebugFileName;

        private bool isLastScan =>
            Core.CurrentAreaLoadedFiles.CurrentPreloadScan == LoadedFiles.MaximumPreloadScans;

        public PreloadAlert()
        {
            CoroutineHandler.Start(OnAreaChanged());
        }

        public override void OnDisable()
        {
            this.Cleanup(true);
        }

        public override void OnEnable()
        {
            this.DebugDirectory = Path.Join(DllDirectory, "Debug");
            Directory.CreateDirectory(this.DebugDirectory);
            this.LoadSettings();
            this.LoadImportantPreloadsFromFile();
        }
        
        public override void SaveSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingPathname));
            var settingsData = JsonConvert.SerializeObject(this.Settings, Formatting.Indented);
            File.WriteAllText(this.SettingPathname, settingsData);
        }

        public override void DrawSettings()
        {
            if (!String.IsNullOrEmpty(ERRORMSG))
            {
                ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), ERRORMSG);
            }

            int totalTime = ((LoadedFiles.MaximumPreloadScans - 1) * LoadedFiles.WaitBetweenScans);
            ImGui.TextWrapped("Disabling/Enabling the plugin " +
                "(via the checkbox beside the plugin name) will reload the important " +
                "preloads from the text file. It will not re-scan the game memory, for " +
                "that you need to change Area/Zone.");
            ImGui.NewLine();
            ImGui.TextWrapped($"Preloads are re-scanned/updated for {totalTime} seconds after " +
                "you change the Area/Zone. Any preloads found during that time will be " +
                "displayed on the UI.");
            ImGui.NewLine();
            ImGui.Checkbox("Lock Preload Window Position/Size", ref this.Settings.Locked);
            ImGui.ColorEdit4("Background", ref this.Settings.BackgroundColor);
            ImGui.Checkbox("Find new preload mode", ref this.Settings.DebugMode);
            if (this.Settings.DebugMode)
            {
                ImGui.TextWrapped($"Please wait for all {LoadedFiles.MaximumPreloadScans} " +
                    $"scans to happen before moving. Please use diary to record all important " +
                    $"things in the Area/Zone.");
            }
        }

        public override void DrawUI()
        {
            //CacheGameData();
            BeginImGuiWindow();
            //if (!String.IsNullOrEmpty(ERRORMSG))
            //{
            //    ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), ERRORMSG);
            //    EndImGuiWindow();
            //    return;
            //}
            //ImGui.TextWrapped($"Status: Scanned {Core.CurrentAreaLoadedFiles.CurrentPreloadScan}" +
            //    $" times out of {LoadedFiles.MaximumPreloadScans}.");
            //if (this.Settings.DebugMode)
            //{
            //    ImGui.TextWrapped($"Please wait for all {LoadedFiles.MaximumPreloadScans} " +
            //        $"iterations before moving.");
            //    ImGui.TextWrapped($"File Name: {this.DebugFileName}");
            //}

            //ImGui.Separator();
            //foreach (var keyValuePair in importantPreloadsInArea)
            //{
            //    ImGui.TextColored(keyValuePair.Value.Color, keyValuePair.Value.DisplayName);
            //}

            EndImGuiWindow();
        }

        private void BeginImGuiWindow()
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, this.Settings.BackgroundColor);
            string title = $"Preload Alert";
            if (this.Settings.Locked)
            {
                ImGui.SetNextWindowPos(this.Settings.Pos);
                ImGui.SetNextWindowSize(this.Settings.Size);
                ImGui.Begin(title, overlayEnabled);
            }
            else
            {
                ImGui.Begin(title, ImGuiWindowFlags.NoSavedSettings);
                this.Settings.Pos = ImGui.GetWindowPos();
                this.Settings.Size = ImGui.GetWindowSize();
            }
        }

        private void EndImGuiWindow()
        {
            ImGui.End();
            ImGui.PopStyleColor();
        }

        private void writeToFile()
        {
            if (this.isLastScan)
            {
                if (!File.Exists(this.DebugFileName))
                {
                    var dataToWrite = this.allPreloadsOfArea.ToList();
                    dataToWrite.Sort();
                    File.WriteAllLines(this.DebugFileName, dataToWrite);
                }
            }
        }

        private void CacheForFileWriting(string preload)
        {
            if (this.isLastScan)
            {
                this.allPreloadsOfArea.Add(preload);
            }
        }

        private void CacheGameData()
        {
            var files = Core.CurrentAreaLoadedFiles.Data;
            if (!files.IsEmpty)
            {
                if (this.Settings.DebugMode)
                {

                }

                while (!files.IsEmpty)
                {
                    files.TryTake(out string file);

                    if (this.Settings.DebugMode)
                    {
                        this.CacheForFileWriting(file);
                    }

                    if (importantPreloadsInArea.ContainsKey(file))
                    {
                        continue;
                    }

                    foreach (var item in preloadsFromFile)
                    {
                        if (file.StartsWith(item.Key))
                        {
                            importantPreloadsInArea.Add(file, item.Value);
                            break;
                        }
                    }
                }

                if (this.Settings.DebugMode)
                {
                    this.DebugFileName = Path.Join(
                        this.DebugDirectory,
                        $"{Core.States.AreaLoading.CurrentAreaName.Trim()}_" +
                        $"{Core.States.InGameStateObject.CurrentAreaInstance.AreaHash}.txt");
                    this.writeToFile();
                }
            }
        }

        private void LoadSettings()
        {
            if (File.Exists(SettingPathname))
            {
                var content = File.ReadAllText(SettingPathname);
                this.Settings = JsonConvert.DeserializeObject<PreloadSettings>(content);
            }
        }

        private void LoadImportantPreloadsFromFile()
        {
            if (!File.Exists(Path.Join(this.PreloadPathname)))
            {
                this.ERRORMSG = $"Missing {this.PreloadPathname} file.";
            }
            else
            {
                this.ERRORMSG = string.Empty;
                foreach (var line in File.ReadAllText(this.PreloadPathname).Split("\r\n"))
                {
                    if (String.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    {
                        continue;
                    }

                    var data = line.Split(';');
                    if (data.Length < 2)
                    {
                        preloadsFromFile.Clear();
                        this.ERRORMSG = $"Invalid preload line found: {line}. Please fix it.";
                        break;
                    }

                    var key = data[0].Trim();
                    var displayName = data[1].Trim();
                    var color = Vector4.One;
                    if (data.Length == 3)
                    {
                        color = this.ConvertToVector4FromStringColor(data[2].Trim().ToLower());
                    }

                    if (String.IsNullOrWhiteSpace(key) ||
                        String.IsNullOrWhiteSpace(displayName))
                    {
                        preloadsFromFile.Clear();
                        this.ERRORMSG = $"Invalid preload line found: {line}. Please fix it.";
                        break;
                    }

                    var preloadInfo = new PreloadInfo()
                    {
                        DisplayName = displayName,
                        Color = color
                    };

                    preloadsFromFile.Add(key, preloadInfo);
                }
            }
        }

        private Vector4 ConvertToVector4FromStringColor(string argb)
        {
            Vector4 color = Vector4.One;
            color.W = uint.Parse($"{argb[0]}{argb[1]}", NumberStyles.HexNumber) / 255f;
            color.X = uint.Parse($"{argb[2]}{argb[3]}", NumberStyles.HexNumber) / 255f;
            color.Y = uint.Parse($"{argb[4]}{argb[5]}", NumberStyles.HexNumber) / 255f;
            color.Z = uint.Parse($"{argb[6]}{argb[7]}", NumberStyles.HexNumber) / 255f;
            return color;
        }

        private void Cleanup(bool clearPreloadsFromFile)
        {
            this.allPreloadsOfArea.Clear();
            this.importantPreloadsInArea.Clear();
            if (clearPreloadsFromFile)
            {
                this.preloadsFromFile.Clear();
            }

        }

        private IEnumerator<Wait> OnAreaChanged()
        {
            while (true)
            {
                // TODO: fix this with when game exit.
                // TODO: doesn't work when game is moved to character select or login screen.
                yield return new Wait(Core.States.AreaLoading.AreaChanged);
                this.Cleanup(false);
            }
        }

        public struct PreloadInfo
        {
            public string DisplayName;
            public Vector4 Color;
        }
    }
}
