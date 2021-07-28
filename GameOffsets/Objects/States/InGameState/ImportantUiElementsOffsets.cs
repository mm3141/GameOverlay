namespace GameOffsets.Objects.States.InGameState
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// All offsets over here are UiElements.
    /// </summary>

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ImportantUiElementsOffsets
    {
        [FieldOffset(0x648)] public IntPtr MapParentPtr;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct MapParentStruct
    {
        [FieldOffset(0x238)] public IntPtr LargeMapPtr;
        [FieldOffset(0x240)] public IntPtr MiniMapPtr;
    }
}
