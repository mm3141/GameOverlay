// <copyright file="Core.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper
{
    using System;
    using System.Collections.Generic;
    using ClickableTransparentOverlay;
    using Coroutine;
    using GameHelper.Controllers;
    using GameHelper.RemoteMemoryObjects;
    using GameHelper.Utils;

    /// <summary>
    /// Main Class to init all the controllers.
    /// NOTE: Upon application startup, this class automatically loads the settings
    /// from the file (or create a new one if it doesn't exists).
    /// </summary>
    public static class Core
    {
        /// <summary>
        /// Gets the GameStates instance. For details read class description.
        /// </summary>
        public static GameStates States
        {
            get;
            private set;
        }

        = new GameStates();

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
        /// Gets the AreaChangeCounter instance. For details read class description.
        /// </summary>
        public static AreaChangeCounter AreaChangeCounter
        {
            get;
            private set;
        }

        = new AreaChangeCounter(IntPtr.Zero);

        /// <summary>
        /// Gets the GameProcess instance. For details read class description.
        /// </summary>
        internal static GameProcess Process { get; private set; } = new GameProcess();

        /// <summary>
        /// Gets the GameHelper settings.
        /// </summary>
        internal static Settings GHSettings
        {
            get;
        }

        = JsonHelper.CreateOrLoadJsonFile<Settings>(Settings.CoreSettingFile);

        /// <summary>
        /// Initializes the <see cref="Core"/> class coroutines.
        /// </summary>
        internal static void InitializeCororutines()
        {
            CoroutineHandler.Start(UpdateOverlayBounds());
            CoroutineHandler.Start(UpdateStatesData());
            CoroutineHandler.Start(UpdateFilesData());
            CoroutineHandler.Start(UpdateAreaChangeData());
            CoroutineHandler.Start(GameClosedActions());
        }

        /// <summary>
        /// Cleans up all the resources taken by the application core.
        /// </summary>
        internal static void Dispose()
        {
            Process.Close(false);
        }

        /// <summary>
        /// Co-routine to update the address where the Game States are loaded in the game memory.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> UpdateStatesData()
        {
            while (true)
            {
                yield return new Wait(Process.OnControllerReady);
                States.Address = Process.StaticAddresses["Game States"];
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
                yield return new Wait(Process.OnControllerReady);
                CurrentAreaLoadedFiles.Address = Process.StaticAddresses["File Root"];
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
                yield return new Wait(Process.OnControllerReady);
                AreaChangeCounter.Address = Process.StaticAddresses["AreaChangeCounter"];
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
                yield return new Wait(Process.OnClose);
                States.Address = IntPtr.Zero;
                CurrentAreaLoadedFiles.Address = IntPtr.Zero;
                AreaChangeCounter.Address = IntPtr.Zero;
            }
        }

        private static IEnumerator<Wait> UpdateOverlayBounds()
        {
            while (true)
            {
                yield return new Wait(Process.OnMoved);
                Overlay.Position = new Veldrid.Point(Process.WindowArea.Location.X, Process.WindowArea.Location.Y);
                Overlay.Size = new Veldrid.Point(Process.WindowArea.Size.Width, Process.WindowArea.Size.Height);
            }
        }
    }
}
