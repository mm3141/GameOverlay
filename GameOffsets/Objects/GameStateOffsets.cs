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
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct StateHeaderStruct
    {
        [FieldOffset(0x00)] public IntPtr VirtualFunctionPtr;
        [FieldOffset(0x0B)] public byte GameStateTypeEnum;
    }
}