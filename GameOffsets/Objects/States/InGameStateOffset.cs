namespace GameOffsets.Objects.States
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct InGameStateOffset
    {
        [FieldOffset(0x18)] public IntPtr AreaInstanceData;
        [FieldOffset(0x78)] public IntPtr WorldData;
        [FieldOffset(0x1A0)] public IntPtr UiRootPtr;
        [FieldOffset(0x450)] public IntPtr IngameUi;
    }
}
