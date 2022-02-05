// <copyright file="LocationValidator.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace Launcher
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     A helper class to validate location.
    /// </summary>
    public static class LocationValidator
    {
        private static readonly Regex BadNameRegex = new(
            @"poe|path\s*of\s*exile|overlay|helper|hud|desktop",
            RegexOptions.IgnoreCase);

        /// <summary>
        ///     Checks the GameHelper tool path name to make sure it doesn't have any bad information.
        /// </summary>
        /// <param name="allowBadPath">throws an exception in case of the failure if true.</param>
        public static void CheckGameHelperLocation(bool allowBadPath)
        {
            var currentProcessPath = Assembly.GetExecutingAssembly().Location;
            var directoryPath = Path.GetDirectoryName(currentProcessPath);
            var pathMatch = BadNameRegex.Match(directoryPath);
            if (pathMatch.Success)
            {
                var message = $"You have downloaded GameHelper in a bad folder/pathname. " +
                    $"Your folder/pathname contains \"{pathMatch.Value}\". " +
                    $"This is bad for your account. Please rename/move GameHelper to a better folder/pathname. " +
                    $"If you don't really care about your account, feel free to launch GameHelper.exe directly " +
                    $"rather than launching from the Launcher.exe.";
                if (allowBadPath)
                {
                    Console.WriteLine(message);
                }
                else
                {
                    throw new Exception(message);
                }
            }
        }
    }
}
