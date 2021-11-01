using System;
using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState
{
    /// <summary>
    ///     All offsets over here are UiElements.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ImportantUiElementsOffsets
    {
        [FieldOffset(0x6B0)] public IntPtr MapParentPtr;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct MapParentStruct
    {
        [FieldOffset(0x278)] public IntPtr LargeMapPtr;
        [FieldOffset(0x280)] public IntPtr MiniMapPtr;
    }
}