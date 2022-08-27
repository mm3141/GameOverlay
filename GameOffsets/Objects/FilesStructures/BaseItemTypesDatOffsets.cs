namespace GameOffsets.Objects.FilesStructures
{
    using System.Runtime.InteropServices;
    using Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BaseItemTypesDatOffsets
    {
        [FieldOffset(0x0030)] public StdWString BaseNamePtr;
    }
}