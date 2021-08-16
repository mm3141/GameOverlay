// <copyright file="MiscHelper.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Utils
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Util class to send keyboard/mouse keys to the game.
    /// </summary>
    public static class MiscHelper
    {
        private enum TcpTableClass
        {
            TcpTableBasicListener,
            TcpTableBasicConnections,
            TcpTableBasicAll,
            TcpTableOwnerPidListener,
            TcpTableOwnerPidConnections,
            TcpTableOwnerPidAll,
            TcpTableOwnerModuleListener,
            TcpTableOwnerModuleConnections,
            TcpTableOwnerModuleAll,
        }

        /*private static void KeyDown(ConsoleKey key)
        {
            if (Core.Process.Address != IntPtr.Zero)
            {
                SendMessage(Core.Process.Information.MainWindowHandle, 0x100, (int)key, 0);
            }
        }*/

        /// <summary>
        /// Releases the key in the game.
        /// </summary>
        /// <param name="key">key to release.</param>
        public static void KeyUp(ConsoleKey key)
        {
            if (Core.Process.Address != IntPtr.Zero)
            {
                SendMessage(Core.Process.Information.MainWindowHandle, 0x101, (int)key, 0);
            }
        }

        /// <summary>
        /// Kills the IPV4 TCP Connection for the process.
        /// </summary>
        /// <param name="processId">process Id whos tcp connection to kill.</param>
        public static void KillTCPConnectionForProcess(uint processId)
        {
            MibTcprowOwnerPid[] table;
            var afInet = 2;
            var buffSize = 0;
            var ret = GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, afInet, TcpTableClass.TcpTableOwnerPidAll);
            var buffTable = Marshal.AllocHGlobal(buffSize);
            try
            {
                ret = GetExtendedTcpTable(buffTable, ref buffSize, true, afInet, TcpTableClass.TcpTableOwnerPidAll);
                if (ret != 0)
                {
                    return;
                }

                var tab = (MibTcptableOwnerPid)Marshal.PtrToStructure(buffTable, typeof(MibTcptableOwnerPid));
                var rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf(tab.DwNumEntries));
                table = new MibTcprowOwnerPid[tab.DwNumEntries];
                for (var i = 0; i < tab.DwNumEntries; i++)
                {
                    var tcpRow = (MibTcprowOwnerPid)Marshal.PtrToStructure(rowPtr, typeof(MibTcprowOwnerPid));
                    table[i] = tcpRow;
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(tcpRow));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffTable);
            }

            // Kill Path Connection
            var pathConnection = table.FirstOrDefault(t => t.OwningPid == processId);
            pathConnection.State = 12;
            var ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(pathConnection));
            Marshal.StructureToPtr(pathConnection, ptr, false);
            SetTcpEntry(ptr);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TcpTableClass tblClass, uint reserved = 0);

        [DllImport("iphlpapi.dll")]
        private static extern int SetTcpEntry(IntPtr pTcprow);

        [StructLayout(LayoutKind.Sequential)]
        private struct MibTcprowOwnerPid
        {
            public uint State;
            public uint LocalAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] LocalPort;
            public uint RemoteAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] RemotePort;
            public uint OwningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MibTcptableOwnerPid
        {
            public uint DwNumEntries;
            private readonly MibTcprowOwnerPid table;
        }
    }
}
