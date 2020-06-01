namespace GameOffsets.Native
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StdVector
    {
        public IntPtr First;
        public IntPtr Last;
        public IntPtr End;
    }
}
