// <copyright file="PManager.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Plugin
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.Settings;
    using GameHelper.Utils;

    /// <summary>
    /// Finds, loads and unloads the plugins.
    /// </summary>
    internal static class PManager
    {
        private static readonly ConcurrentBag<KeyValuePair<string, IPCore>> Plugins = new();

        /// <summary>
        /// Gets the loaded plugins.
        /// </summary>
        internal static Dictionary<string, PContainer> AllPlugins
        {
            get;
            private set;
        }

        = JsonHelper.CreateOrLoadJsonFile<Dictionary<string, PContainer>>(
            State.PluginsMetadataFile);

        /// <summary>
        /// Initlizes the plugin manager by loading all the plugins and their Metadata.
        /// </summary>
        internal static void InitializePlugins()
        {
            State.PluginsDirectory.Create(); // doesn't do anything if already exists.
            Parallel.ForEach(GetPluginsDirectories(), LoadPlugin);
            CombinePluginAndMetadata();
            Parallel.ForEach(AllPlugins, EnablePluginIfRequired);
            CoroutineHandler.Start(SavePluginSettings());
            CoroutineHandler.Start(SavePluginMetadata());
            Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(
                DrawPluginUiRenderCoroutine(), "[PManager] Draw Plugins UI"));
        }
#if DEBUG
        /// <summary>
        /// Cleans up the already loaded plugins.
        /// </summary>
        internal static void CleanUpAllPlugins()
        {
            foreach (var plugin in AllPlugins)
            {
                plugin.Value.Plugin.OnDisable();
            }

            AllPlugins.Clear();
        }
#endif

        private static List<DirectoryInfo> GetPluginsDirectories()
        {
            return State.PluginsDirectory.GetDirectories().Where(
                x => (x.Attributes & FileAttributes.Hidden) == 0).ToList();
        }

        private static Assembly ReadPluginFiles(DirectoryInfo pluginDirectory)
        {
            try
            {
                var dllFile = pluginDirectory.GetFiles(
                    $"{pluginDirectory.Name}*.dll",
                    SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (dllFile == null)
                {
                    Console.WriteLine($"Couldn't find plugin dll with name {pluginDirectory.Name}" +
                        $" in directory {pluginDirectory.FullName}." +
                        $" Please make sure DLL & the plugin got same name.");
                }

                var pdbPath = dllFile.FullName.Replace(".dll", ".pdb");
                var dllData = File.ReadAllBytes(dllFile.FullName);
                if (File.Exists(pdbPath))
                {
                    return Assembly.Load(dllData, File.ReadAllBytes(pdbPath));
                }

                return Assembly.Load(dllData);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load plugin {pluginDirectory.FullName} due to {e}");
                return null;
            }
        }

        private static void LoadPlugin(DirectoryInfo pluginDirectory)
        {
            var assembly = ReadPluginFiles(pluginDirectory);
            if (assembly != null)
            {
                var relativePluginDir = pluginDirectory.FullName.Replace(
                    State.PluginsDirectory.FullName, State.PluginsDirectory.Name);
                LoadPlugin(assembly, relativePluginDir);
            }
        }

        private static void LoadPlugin(Assembly assembly, string pluginRootDirectory)
        {
            try
            {
                var types = assembly.GetTypes();
                if (types.Length <= 0)
                {
                    Console.WriteLine($"Plugin (in {pluginRootDirectory}) {assembly} doesn't " +
                        $"contain any types (i.e. classes/stuctures).");
                    return;
                }

                var iPluginClasses = types.Where(
                    type => typeof(IPCore).IsAssignableFrom(type) &&
                    type.IsSealed == true).ToList();
                if (iPluginClasses.Count != 1)
                {
                    Console.WriteLine($"Plugin (in {pluginRootDirectory}) {assembly} contains" +
                        $" {iPluginClasses.Count} sealed classes derived from CoreBase<TSettings>." +
                        $" It should have one sealed class derived from IPlugin.");
                    return;
                }

                IPCore pluginCore = Activator.CreateInstance(iPluginClasses[0]) as IPCore;
                pluginCore.SetPluginDllLocation(pluginRootDirectory);
                string pluginName = assembly.GetName().Name;
                Plugins.Add(new KeyValuePair<string, IPCore>(pluginName, pluginCore));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading plugin {assembly.FullName} due to {e}");
            }
        }

        private static void CombinePluginAndMetadata()
        {
            while (Plugins.TryTake(out var tmp))
            {
                if (AllPlugins.ContainsKey(tmp.Key))
                {
                    var pC = AllPlugins[tmp.Key];
                    pC.Plugin = tmp.Value;
                    AllPlugins[tmp.Key] = pC;
                }
                else
                {
                    var pC = new PContainer() { Enable = true, Plugin = tmp.Value };
                    AllPlugins.Add(tmp.Key, pC);
                }
            }

            // Removing any plugins Metadata which are deleted by the users.
            foreach (var item in AllPlugins.ToList())
            {
                if (item.Value.Plugin == null)
                {
                    AllPlugins.Remove(item.Key);
                }
            }

            JsonHelper.SafeToFile(AllPlugins, State.PluginsMetadataFile);
        }

        private static void EnablePluginIfRequired(KeyValuePair<string, PContainer> kv)
        {
            if (kv.Value.Enable)
            {
                kv.Value.Plugin.OnEnable(Core.Process.Address != IntPtr.Zero);
            }
        }

        private static IEnumerator<Wait> SavePluginMetadata()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.TimeToSaveAllSettings);
                JsonHelper.SafeToFile(AllPlugins, State.PluginsMetadataFile);
            }
        }

        private static IEnumerator<Wait> SavePluginSettings()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.TimeToSaveAllSettings);
                foreach (var keyvalue in AllPlugins)
                {
                    keyvalue.Value.Plugin.SaveSettings();
                }
            }
        }

        private static IEnumerator<Wait> DrawPluginUiRenderCoroutine()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnRender);
                foreach (var pluginKeyValue in AllPlugins)
                {
                    if (pluginKeyValue.Value.Enable)
                    {
                        pluginKeyValue.Value.Plugin.DrawUI();
                    }
                }
            }
        }
    }
}
