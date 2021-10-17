namespace GameOffsets
{
    using System.Collections.Generic;

    public struct GameProcessDetails
    {
        /// <summary>
        /// Name of the Game Process (Normally name of the executable file without .exe)
        /// See task-manager to find the exact process name.
        /// </summary>
        public static string ProcessName = "PathOfExile";

        /// <summary>
        /// Name of the Game Title Window
        /// </summary>
        public static string WindowTitle = "Path of Exile".ToLower();

        public static List<string> Contributors = new List<string>()
        {
            "Dax***",
            "Scrippydoo",
            "Riyu",
            "Noneyatemp",
            "hienngocloveyou",
        };
    }
}
