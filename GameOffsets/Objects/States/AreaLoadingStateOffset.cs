namespace GameOffsets.Objects.States
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AreaLoadingStateOffset
    {
        [FieldOffset(0x0D8)] public int IsLoading;
        [FieldOffset(0x1C4)] public uint TotalLoadingScreenTimeMs;
        //[FieldOffset(0x1CC)] public float TotalLoadingScreenTimeSec;
        //[FieldOffset(0x1D0)] public IntPtr UnknownPtr1;
        //[FieldOffset(0x1D8)] public IntPtr UnknownPtr2;
        //[FieldOffset(0x1E0)] public IntPtr UnknownPtr3;
        //[FieldOffset(0x1D8)] public IntPtr UnknownPtr4;
        [FieldOffset(0x1F0)] public StdWString CurrentAreaName;
        //[FieldOffset(0x218)] public IntPtr LoadScreenImagePtr;
    }
}
