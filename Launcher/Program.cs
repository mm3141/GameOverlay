// <copyright file="Program.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace Launcher
{
    using System;
    using System.Diagnostics;

    public static class Program
    {
        private static void Main()
        {
            if (!GameHelperFinder.TryFindGameHelperExe(out var gameHelperDir, out var gameHelperLoc))
            {
                Console.WriteLine($"GameHelper.exe is also not found in {gameHelperDir}");
                Console.ReadKey();
                return;
            }

            try
            {
                AutoUpdate.UpgradeGameHelper(gameHelperDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to upgrade GameHelper due to: {ex}");
                Console.ReadKey();
            }

            try
            {
                Console.WriteLine("Preparing GameHelper...");
                var newName = MiscHelper.GenerateRandomString();
                TemporaryFileManager.Purge();
                //TODO: if functionality extends, should probably utilize an argument parser, but good for now
                if (!LocationValidator.IsGameHelperLocationGood(out var message))
                {
                    Console.WriteLine(message);
                    Console.Write("Press any key to ignore this warning.");
                    Console.ReadLine();
                }

                var gameHelperPath = GameHelperTransformer.TransformGameHelperExecutable(gameHelperDir, gameHelperLoc, newName);
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
