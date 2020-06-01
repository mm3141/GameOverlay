// <copyright file="Program.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper
{
    using System;
    using System.IO;
    using System.Threading;
    using Coroutine;

    /// <summary>
    /// Class executed when the application starts.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// function executed when the application starts.
        /// </summary>
        private static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, exceptionArgs) =>
            {
                var errorText = "Program exited with message:\n " + exceptionArgs.ExceptionObject;
                File.AppendAllText("Error.log", $"{DateTime.Now:g} {errorText}\r\n{new string('-', 30)}\r\n");
                Environment.Exit(1);
            };

            var lastTime = DateTime.Now;
            var currTime = DateTime.Now;
            while (true)
            {
                currTime = DateTime.Now;
                CoroutineHandler.Tick((currTime - lastTime).TotalSeconds);
                lastTime = currTime;
                Thread.Sleep(1);
            }
        }
    }
}
