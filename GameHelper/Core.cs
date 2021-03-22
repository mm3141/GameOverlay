// <copyright file="Core.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.RemoteObjects;
    using GameHelper.Settings;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// Main Class that depends on the GameProcess Events
    /// and updates the RemoteObjects. It also manages the
    /// GameHelper settings.
    /// </summary>
    public static class Core
    {
        /// <summary>
        /// Gets the GameHelper Overlay.
        /// </summary>
        public static GameOverlay Overlay
        {
            get;
            internal set;
        }

        = null;

        /// <summary>
        /// Gets the list of active coroutines.
        /// </summary>
        public static List<ActiveCoroutine> CoroutinesRegistrar
        {
            get;
            private set;
        }

        = new List<ActiveCoroutine>();

        /// <summary>
        /// Gets the GameStates instance. For details read class description.
        /// </summary>
        public static GameStates States
        {
            get;
            private set;
        }

        = new GameStates(IntPtr.Zero);

        /// <summary>
        /// Gets the files loaded for the current area.
        /// </summary>
        public static LoadedFiles CurrentAreaLoadedFiles
        {
            get;
            private set;
        }

        = new LoadedFiles(IntPtr.Zero);

        /// <summary>
        /// Gets the GameProcess instance. For details read class description.
        /// </summary>
        public static GameProcess Process
        {
            get; private set;
        }

        = new GameProcess();

        /// <summary>
        /// Gets the AreaChangeCounter instance. For details read class description.
        /// </summary>
        internal static AreaChangeCounter AreaChangeCounter
        {
            get;
            private set;
        }

        = new AreaChangeCounter(IntPtr.Zero);

        /// <summary>
        /// Gets the values associated with the Game Window Scale.
        /// </summary>
        internal static GameWindowScale GameScale
        {
            get;
            private set;
        }

        = new GameWindowScale(IntPtr.Zero);

        /// <summary>
        /// Gets the GameHelper settings.
        /// </summary>
        internal static State GHSettings
        {
            get;
        }

        = JsonHelper.CreateOrLoadJsonFile<State>(State.CoreSettingFile);

        /// <summary>
        /// Initializes the <see cref="Core"/> class coroutines.
        /// </summary>
        internal static void InitializeCororutines()
        {
            CoroutineHandler.Start(GameClosedActions());
            CoroutineHandler.Start(UpdateStatesData(), priority: int.MaxValue - 3);
            CoroutineHandler.Start(UpdateFilesData(), priority: int.MaxValue - 2);
            CoroutineHandler.Start(UpdateAreaChangeData(), priority: int.MaxValue - 1);
            CoroutineHandler.Start(UpdateScaleData(), priority: int.MaxValue);
        }

        /// <summary>
        /// Cleans up all the resources taken by the application core.
        /// </summary>
        internal static void Dispose()
        {
            Process.Close(false);
        }

        /// <summary>
        /// Converts the RemoteObjects to ImGui Widgets.
        /// </summary>
        internal static void RemoteObjectsToImGuiCollapsingHeader()
        {
            var propertyFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            foreach (var property in UiHelper.GetToImGuiMethods(typeof(Core), propertyFlags, null))
            {
                if (ImGui.CollapsingHeader(property.Name))
                {
                    property.ToImGui.Invoke(property.Value, null);
                }
            }
        }

        /// <summary>
        /// Co-routine to update the address where the
        /// Game Window Values are loaded in the game memory.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> UpdateScaleData()
        {
            while (true)
            {
                yield return new Wait(Process.OnStaticAddressFound);
                GameScale.Address = Process.StaticAddresses["GameWindowScaleValues"];
            }
        }

        /// <summary>
        /// Co-routine to update the address where AreaChange object is loaded in the game memory.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> UpdateAreaChangeData()
        {
            while (true)
            {
                yield return new Wait(Process.OnStaticAddressFound);
                AreaChangeCounter.Address = Process.StaticAddresses["AreaChangeCounter"];
            }
        }

        /// <summary>
        /// Co-routine to update the address where the Files are loaded in the game memory.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> UpdateFilesData()
        {
            while (true)
            {
                yield return new Wait(Process.OnStaticAddressFound);
                CurrentAreaLoadedFiles.Address = Process.StaticAddresses["File Root"];
            }
        }

        /// <summary>
        /// Co-routine to update the address where the Game States are loaded in the game memory.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> UpdateStatesData()
        {
            while (true)
            {
                yield return new Wait(Process.OnStaticAddressFound);
                States.Address = Process.StaticAddresses["Game States"];
            }
        }

        /// <summary>
        /// Co-routine to set All controllers addresses to Zero,
        /// once the game closes.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> GameClosedActions()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnClose);
                States.Address = IntPtr.Zero;
                CurrentAreaLoadedFiles.Address = IntPtr.Zero;
                AreaChangeCounter.Address = IntPtr.Zero;
                GameScale.Address = IntPtr.Zero;
            }
        }
    }
}
