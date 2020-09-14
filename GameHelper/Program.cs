// <copyright file="Program.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper
{
    using System;
    using System.IO;
    using ClickableTransparentOverlay;
    using GameHelper.Plugin;
    using GameHelper.UI;

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

            if (!Core.GHSettings.ShowTerminal)
            {
                Overlay.TerminalWindow = false;
            }

            PManager.InitializePlugins();
            SettingsWindow.InitializeCoroutines();
            Core.InitializeCororutines();
            Overlay.RunInfiniteLoop(); // Overlay disposes itself before exit.
            Core.Dispose();
        }
    }
}