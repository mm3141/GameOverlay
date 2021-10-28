namespace GameOffsets.Objects.FilesStructures
{
    using GameOffsets.Natives;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BaseItemTypesDatOffsets
    {
        [FieldOffset(0x0018)] public StdWString BaseNamePtr;
    }
}
