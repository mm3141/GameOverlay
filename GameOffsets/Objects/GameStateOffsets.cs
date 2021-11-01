using System;
using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GameStateStaticOffset
    {
        [FieldOffset(0x00)] public IntPtr GameState;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GameStateOffset
    {
        [FieldOffset(0x08)] public StdVector CurrentStatePtr; // Used in RemoteObject -> CurrentState

        //[FieldOffset(0x20)] public StdVector CurrentStatePtr; // same as CurrentStatePtr.
        [FieldOffset(0x50)] public StdList States;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct StateInternalStructure
    {
        [FieldOffset(0x00)] public byte StateEnumToName;
        [FieldOffset(0x08)] public IntPtr StatePtr;
    }
}