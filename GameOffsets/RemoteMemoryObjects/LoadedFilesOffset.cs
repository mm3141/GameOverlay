namespace GameOffsets.RemoteMemoryObjects
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Native;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct LoadedFilesRootObjectOffset
    {
        [FieldOffset(0x00)] public float IsValid;
        [FieldOffset(0x04)] public int intUseless;
        [FieldOffset(0x08)] public StdList FilesList; //FileInfoPtr
        [FieldOffset(0x18)] public StdVector FilesVectorUseless;
        [FieldOffset(0x30)] public long long1Useless;
        [FieldOffset(0x38)] public long long2Useless;
        public static int TotalCount = 256;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct FileInfoPtr
    {
        // optimization 101: use this NamePtr to determine if it's Metadata or not.
        // skip non-metadata files for the purpose of finding preloads.
        [FieldOffset(0x00)] public IntPtr NamePtr;
        [FieldOffset(0x08)] public IntPtr Information; //FileInfo
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct FileInfo
    {
        [FieldOffset(0x08)] public StdWString Name;
        [FieldOffset(0x28)] public int FileType; // Possible values 0x01, 0x03 (0x03 is useless)
        [FieldOffset(0x30)] public IntPtr UnknownPtr;
        [FieldOffset(0x38)] public int AreaChangeCount;
        [FieldOffset(0x3C)] public int CounterRelatedToAreaChange;
        [FieldOffset(0x40)] public int CounterWhichStopsWhenGameIsMinimized;
    }
}
