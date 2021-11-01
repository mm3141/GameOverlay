using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.Components
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PlayerOffsets
    {
        [FieldOffset(0x000)] public ComponentHeader Header;
        [FieldOffset(0x160)] public StdWString Name;
    }
}