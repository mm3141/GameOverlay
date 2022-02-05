namespace GameOffsets.Objects
{
    using System;
    using System.Runtime.InteropServices;
    using Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GameStateStaticOffset
    {
        [FieldOffset(0x00)] public IntPtr GameState;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GameStateOffset
    {
        [FieldOffset(0x08)] public StdVector CurrentStatePtr; // Used in RemoteObject -> CurrentState
        [FieldOffset(0x48)] public IntPtr State0;
        [FieldOffset(0x58)] public IntPtr State1;
        [FieldOffset(0x68)] public IntPtr State2;
        [FieldOffset(0x78)] public IntPtr State3;
        [FieldOffset(0x88)] public IntPtr State4;
        [FieldOffset(0x98)] public IntPtr State5;
        [FieldOffset(0xA8)] public IntPtr State6;
        [FieldOffset(0xB8)] public IntPtr State7;
        [FieldOffset(0xC8)] public IntPtr State8;
        [FieldOffset(0xD8)] public IntPtr State9;
        [FieldOffset(0xE8)] public IntPtr State10;
        [FieldOffset(0xF8)] public IntPtr State11;
    }
}