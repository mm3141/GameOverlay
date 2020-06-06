
namespace GameOffsets.RemoteMemoryObjects.Files
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Native;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct FileInfoPtr
    {
        // optimization 101: use this NamePtr to determine if it's Metadata or not.
        // skip non-metadata files for the purpose of finding preloads.
        [FieldOffset(0x00)] public IntPtr NamePtr;
        [FieldOffset(0x08)] public IntPtr Information;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct FileInfo
    {
        [FieldOffset(0x08)] public StdWString Name;
        [FieldOffset(0x28)] public IntPtr FileType; //0x01 = all files, 0x03 = TextureResource
        [FieldOffset(0x30)] public IntPtr UnknownPtr;
        [FieldOffset(0x38)] public int AreaChangeCount;
        [FieldOffset(0x3C)] public int CounterRelatedToAreaChange;
        [FieldOffset(0x40)] public int CounterWhichStopsWhenGameIsMinimized;
    }
}
