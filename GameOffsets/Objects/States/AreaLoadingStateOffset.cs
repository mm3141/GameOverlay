namespace GameOffsets.Objects.States
{
    using System.Runtime.InteropServices;
    using Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AreaLoadingStateOffset
    {
        [FieldOffset(0xC8)] public int IsLoading;

        [FieldOffset(0x368)] public uint TotalLoadingScreenTimeMs;
        [FieldOffset(0x3A8)] public StdWString CurrentAreaName; // use isloading/totalLoadingScreenTimeMs offset diff
    }
}
