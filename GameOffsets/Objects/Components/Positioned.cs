using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PositionedOffsets
    {
        [FieldOffset(0x000)] public ComponentHeader Header;

        [FieldOffset(0x159)] public byte Reaction;
        // [FieldOffset(0x1E8)] public StdTuple2D<int> GridPosition;
        // [FieldOffset(0x214)] public StdTuple2D<float> WorldPosition;
    }
}