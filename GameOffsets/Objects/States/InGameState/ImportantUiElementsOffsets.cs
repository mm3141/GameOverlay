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
        [FieldOffset(0x540)] public InventoryAndStashPanelStruct InvAndStashPanels;
        [FieldOffset(0x648)] public IntPtr MapParentPtr;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct MapParentStruct
    {
        [FieldOffset(0x238)] public IntPtr LargeMapPtr; // MapUiElement struct
        [FieldOffset(0x240)] public IntPtr MiniMapPtr; // MapUiElement struct
    }

    // not a real struct but they are always side by side.
    // they are at UiRoot -> MainChild -> 35th, 36th Child respectively.
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct InventoryAndStashPanelStruct
    {
        [FieldOffset(0x00)] public IntPtr InventoryPanelPtr; // InventoryPanelUiElement struct.
        // [FieldOffset(0x08)] public IntPtr StashPanelPtr;
    }
}
