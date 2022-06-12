// <copyright file="Program.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace Launcher
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    public static class Program
    {
        private static int Main(string[] args)
        {
            if (!GameHelperFinder.TryFindGameHelperExe(out var gameHelperDir, out var gameHelperLoc))
            {
                Console.WriteLine($"GameHelper.exe is also not found in {gameHelperDir}");
                Console.ReadKey();
                return 1;
            }

            try
            {
                var isWaiting = false;
                if (args.Length == 0)
                {
                    Console.WriteLine("Are you waiting for a new GameHelper release? (yes/no)");
                    isWaiting = Console.ReadLine().ToLowerInvariant().StartsWith("y");
                }

                do
                {
                    if (AutoUpdate.UpgradeGameHelper(gameHelperDir))
                    {
                        // Returning because Launcher should auto-restart on exit.
                        return 0;
                    }
                    else if (isWaiting)
                    {
                        Console.WriteLine("Didn't find any new version, sleeping for 5 mins....");
                        Thread.Sleep(5 * 60 * 1000);
                    }
                }
                while (isWaiting);
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

            return 0;
        }
    }
}
