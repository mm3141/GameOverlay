// <copyright file="Core.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.Controllers;
    using GameHelper.RemoteMemoryObjects;

    /// <summary>
    /// Main Class to init all the controllers.
    /// </summary>
    public static class Core
    {
        /// <summary>
        /// Gets the GameProcess instance. For details read class description.
        /// </summary>
        public static GameProcess Process { get; private set; } = new GameProcess();

        /// <summary>
        /// Gets the GameStates instance. For details read class description.
        /// </summary>
        public static GameStates States { get; private set; } = new GameStates();

        /// <summary>
        /// Gets the GameFiles instance. For details read class description.
        /// </summary>
        public static GameFiles Files { get; private set; } = new GameFiles();

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
        /// Initializes the <see cref="Core"/> class.
        /// </summary>
        public static void Initialize()
        {
            CoroutineHandler.Start(UpdateStatesData());
            CoroutineHandler.Start(UpdateFilesData());
            CoroutineHandler.Start(UpdateAreaChangeData());
            CoroutineHandler.Start(GameClosedActions());
        }

        /// <summary>
        /// Cleans up all the resources taken by the application core.
        /// </summary>
        public static void Dispose()
        {
            Process.Close(false);
        }

        /// <summary>
        /// Co-routine to update the address where the States are loaded in the game memory.
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
                Files.Address = Process.StaticAddresses["File Root"];
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
                Files.Address = IntPtr.Zero;
                AreaChangeCounter.Address = IntPtr.Zero;
            }
        }
    }
}
