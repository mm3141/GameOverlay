﻿// <copyright file="PManager.cs" company="None">
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
    using ClickableTransparentOverlay;
    using Coroutine;
    using GameHelper.Utils;

    /// <summary>
    /// Finds, loads and unloads the plugins.
    /// TODO: Hot Reload plugins on on plugin hash changes.
    /// TODO: Allow user to enable/disable this feature ^.
    /// </summary>
    internal static class PManager
    {
        private static ConcurrentBag<KeyValuePair<string, IPCore>> plugins =
            new ConcurrentBag<KeyValuePair<string, IPCore>>();

        /// <summary>
        /// Gets the loaded plugins.
        /// </summary>
        internal static Dictionary<string, PContainer> AllPlugins
        {
            get;
            private set;
        }

        = JsonHelper.CreateOrLoadJsonFile<Dictionary<string, PContainer>>(
            Settings.PluginsMetadataFile);

        /// <summary>
        /// Initlizes the plugin manager by loading all the plugins & their Metadata.
        /// </summary>
        internal static void InitializePlugins()
        {
            if (!Settings.PluginsDirectory.Exists)
            {
                Settings.PluginsDirectory.Create();
            }
            else
            {
                Parallel.ForEach(GetPluginsDirectories(), (pluginDirectory) =>
                {
                    var assembly = ReadPluginFiles(pluginDirectory);
                    if (assembly != null)
                    {
                        LoadPlugin(assembly, pluginDirectory.FullName);
                    }
                });
            }

            while (plugins.TryTake(out var tmp))
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

            CoroutineHandler.Start(DrawPluginUi());
        }

        private static List<DirectoryInfo> GetPluginsDirectories()
        {
            return Settings.PluginsDirectory.GetDirectories().Where(
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
                string pluginName = assembly.GetName().Name;
                plugins.Add(new KeyValuePair<string, IPCore>(pluginName, pluginCore));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading plugin {assembly.FullName} due to {e}");
            }
        }

        private static IEnumerator<Wait> DrawPluginUi()
        {
            while (true)
            {
                yield return new Wait(Overlay.OnRender);
                foreach (var pluginKeyValue in AllPlugins)
                {
                    pluginKeyValue.Value.Plugin.DrawUI();
                }
            }
        }
    }
}