namespace GameOffsets.RemoteMemoryObjects.States
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct InGameStateOffset
    {
        //[FieldOffset(0x080)] public IntPtr GameUi;
        //[FieldOffset(0x0A0)] public IntPtr EntitiesLabels;
        [FieldOffset(0x500)] public IntPtr LocalData;
        //[FieldOffset(0x508)] public IntPtr ServerData;
    }
}
