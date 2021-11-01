using System;
using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BaseOffsets
    {
        [FieldOffset(0x0000)] public ComponentHeader Header;
        [FieldOffset(0x0010)] public IntPtr BaseInternalPtr; //BaseItemTypes.dat
    }
}