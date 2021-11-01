namespace GameOffsets
{
    using System.Collections.Generic;

    public struct GameProcessDetails
    {
        /// <summary>
        ///     Name of the Game Process (Normally name of the executable file without .exe)
        ///     and the main window title. See task-manager to find the exact process name
        ///     and convert the game to the window mode to see the window title on the game.
        /// </summary>
        public static readonly Dictionary<string, string> ProcessName = new()
        {
            { "PathOfExile", "Path of Exile".ToLower() }, // also works on Garena
            { "PathOfExile_KG", "Path of Exile".ToLower() },
            { "PathOfExileSteam", "Path of Exile".ToLower() },
            { "PathOfExile_x64", "Path of Exile".ToLower() }
        };

        public static readonly List<string> Contributors = new()
        {
            "Dax***",
            "Scrippydoo",
            "Riyu",
            "Noneyatemp",
            "hienngocloveyou"
        };
    }
}