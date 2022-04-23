namespace GameOffsets.Objects.States
{
    using System.Runtime.InteropServices;
    using Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AreaLoadingStateOffset
    {
        [FieldOffset(0xC8)] public int IsLoading;

        [FieldOffset(0x354)] public uint TotalLoadingScreenTimeMs;

        //[FieldOffset(0x1CC)] public float TotalLoadingScreenTimeSec;
        //[FieldOffset(0x1D0)] public IntPtr UnknownPtr1;
        //[FieldOffset(0x1D8)] public IntPtr UnknownPtr2;
        //[FieldOffset(0x1E0)] public IntPtr UnknownPtr3;
        //[FieldOffset(0x1D8)] public IntPtr UnknownPtr4;
        [FieldOffset(0x390)] public StdWString CurrentAreaName; // TODO: use isloading offset diff
        //[FieldOffset(0x218)] public IntPtr LoadScreenImagePtr;
    }
}
