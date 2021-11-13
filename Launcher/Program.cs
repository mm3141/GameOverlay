// <copyright file="Program.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace Launcher
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    public static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Preparing GameHelper...");
                var newName = MiscHelper.GenerateRandomString();
                TemporaryFileManager.Purge();
                //TODO: if functionality extends, should probably utilize an argument parser, but good for now
                LocationValidator.CheckGameHelperLocation(Debugger.IsAttached || args.Contains("-allowPath", StringComparer.InvariantCultureIgnoreCase));
                var gameHelperPath = GameHelperTransformer.TransformGameHelperExecutable(newName);
                Console.WriteLine($"Starting GameHelper at '{gameHelperPath}'...");
                Process.Start(gameHelperPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to launch GameHelper due to: {ex}");
                Console.ReadKey();
            }
        }
    }
}
