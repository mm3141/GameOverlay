// <copyright file="Core.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.Controllers;

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
        /// Gets the GameState instance. For details read class description.
        /// </summary>
        public static GameState State { get; private set; } = new GameState();

        /// <summary>
        /// Gets the GameFiles instance. For details read class description.
        /// </summary>
        public static GameFiles Files { get; private set; } = new GameFiles();

        /// <summary>
        /// Initializes the <see cref="Core"/> class.
        /// </summary>
        public static void Initlize()
        {
            CoroutineHandler.Start(UpdateStateData());
            CoroutineHandler.Start(UpdateFilesData());
            CoroutineHandler.Start(GameClosedActions());
        }

        /// <summary>
        /// Co-routine to update the State address and related data.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<IWait> UpdateStateData()
        {
            while (true)
            {
                yield return new WaitEvent(Process.OnControllerReady);
                State.Address = Process.StaticAddresses["Game State"];
            }
        }

        /// <summary>
        /// Co-routine to set All controllers addresses to Zero,
        /// once the game closes.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<IWait> GameClosedActions()
        {
            while (true)
            {
                yield return new WaitEvent(Process.OnClose);
                State.Address = IntPtr.Zero;
                Files.Address = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Co-routine to update the Files loaded in game memory address and related data.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<IWait> UpdateFilesData()
        {
            while (true)
            {
                yield return new WaitEvent(Process.OnControllerReady);
                Files.Address = Process.StaticAddresses["File Root"];
            }
        }
    }
}
