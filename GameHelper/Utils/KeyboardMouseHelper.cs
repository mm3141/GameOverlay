// <copyright file="KeyboardMouseHelper.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Utils
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Util class to send keyboard/mouse keys to the game.
    /// </summary>
    public static class KeyboardMouseHelper
    {
        /*private static void KeyDown(ConsoleKey key)
        {
            if (Core.Process.Address != IntPtr.Zero)
            {
                SendMessage(Core.Process.Information.MainWindowHandle, 0x100, (int)key, 0);
            }
        }*/

        /// <summary>
        /// Releases the key.
        /// </summary>
        /// <param name="key">key to release.</param>
        public static void KeyUp(ConsoleKey key)
        {
            if (Core.Process.Address != IntPtr.Zero)
            {
                SendMessage(Core.Process.Information.MainWindowHandle, 0x101, (int)key, 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
    }
}
