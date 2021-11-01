using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.FilesStructures
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BaseItemTypesDatOffsets
    {
        [FieldOffset(0x0018)] public StdWString BaseNamePtr;
    }
}