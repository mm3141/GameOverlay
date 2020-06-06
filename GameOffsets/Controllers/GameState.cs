namespace GameOffsets.Controllers
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Native;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GameStateStaticObject
    {
        [FieldOffset(0x00)] public IntPtr GameState;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GameStateObject
    {
        [FieldOffset(0x48)] public StdMap States;
    }
}