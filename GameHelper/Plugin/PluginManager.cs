// <copyright file="PluginManager.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Finds, loads and unloads the plugins.
    /// TODO: Hot Reload plugins on on plugin hash changes.
    /// TODO: Allow user to enable/disable this feature ^.
    /// </summary>
    public static class PluginManager
    {
        private static readonly DirectoryInfo PluginsDirectory = new DirectoryInfo("Plugins");
        private static List<Plugin> AllPlugins = new List<Plugin>(20);

        /// <summary>
        /// Initlizes the plugin manager by loading all the plugins
        /// and keep a watch on updates and newly added plugins.
        /// </summary>
        public static void Initialize()
        {
            if (!PluginsDirectory.Exists)
            {
                PluginsDirectory.Create();
            }
            else
            {
                var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 16 };
                Parallel.ForEach(GetPluginsDirectories(), parallelOptions, (pluginDirectory) =>
                {
                    var assembly = ReadPluginFiles(pluginDirectory);
                    if (assembly != null)
                    {
                        LoadPlugin(assembly, pluginDirectory.FullName);
                    }
                });
            }
        }

        private static List<DirectoryInfo> GetPluginsDirectories()
        {
            return PluginsDirectory.GetDirectories().Where(
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
                    type => typeof(IPlugin).IsAssignableFrom(type) &&
                    type.IsSealed == true).ToList();
                if (iPluginClasses.Count != 1)
                {
                    Console.WriteLine($"Plugin (in {pluginRootDirectory}) {assembly} contains" +
                        $" {iPluginClasses.Count} sealed classes derived from IPlugin." +
                        $" It should have one sealed class derived from IPlugin.");
                    return;
                }

                var iSettingClasses = types.Where(
                    type => typeof(ISettings).IsAssignableFrom(type) &&
                    type.IsSealed == true).ToList();
                if (iSettingClasses.Count != 1)
                {
                    Console.WriteLine($"Plugin (in {pluginRootDirectory}) {assembly} contains" +
                        $" {iSettingClasses.Count} sealed classes derived from ISettings." +
                        $" It should have one sealed class derived from ISettings.");
                    return;
                }

                IPlugin core = Activator.CreateInstance(iPluginClasses[0]) as IPlugin;
                ISettings settings = Activator.CreateInstance(iSettingClasses[0]) as ISettings;
                AllPlugins.Add(new Plugin(core, settings, pluginRootDirectory));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading plugin {assembly.FullName} due to {e}");
            }
        }
    }
}
