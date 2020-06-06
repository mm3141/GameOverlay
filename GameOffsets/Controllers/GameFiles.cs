namespace GameOffsets.Controllers
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Native;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GameFilesStaticObject
    {
        [FieldOffset(0x08)] public StdList FilesList;
    }
}
