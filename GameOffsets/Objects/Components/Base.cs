namespace GameOffsets.Objects.Components
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BaseOffsets
    {
        [FieldOffset(0x0000)] public ComponentHeader Header;
        [FieldOffset(0x0010)] public IntPtr BaseInternalPtr; //BaseItemTypes.dat
    }
}