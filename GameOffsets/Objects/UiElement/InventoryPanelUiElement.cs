namespace GameOffsets.Objects.UiElement
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct InventoryPanelUiElementOffset
    {
        [FieldOffset(0x000)] public UiElementBaseOffset UiElementBase;
        public static int InventoryListOffset = 0x350;
        public static int TotalInventories = 36;
    }
}
