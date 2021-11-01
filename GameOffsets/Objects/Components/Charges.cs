using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ChargesOffsets
    {
        [FieldOffset(0x0000)] public ComponentHeader Header;
        [FieldOffset(0x0018)] public int current;
    }
}