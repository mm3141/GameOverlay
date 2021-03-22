// <copyright file="Program.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Class executed when the application starts.
    /// TODO: Disable when Game is minimized.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// function executed when the application starts.
        /// </summary>
        private static async Task Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, exceptionArgs) =>
            {
                var errorText = "Program exited with message:\n " + exceptionArgs.ExceptionObject;
                File.AppendAllText("Error.log", $"{DateTime.Now:g} {errorText}\r\n{new string('-', 30)}\r\n");
                Environment.Exit(1);
            };

            using (Core.Overlay = new GameOverlay())
            {
                await Core.Overlay.Run();
            }
        }
    }
}