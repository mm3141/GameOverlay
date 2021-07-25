namespace GameOffsets.Objects.UiElement
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct InventoryItemOffset
    {
        [FieldOffset(0x000)] public UiElementBaseOffset UiElementBase;
        [FieldOffset(0x390)] public ItemDetailsStruct ItemDetails;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ItemDetailsStruct // TODO: Validate the struct via Game function analysis.
    {
        [FieldOffset(0x00)] public IntPtr Item;
        [FieldOffset(0x08)] public StdTuple2D<int> Position;
        [FieldOffset(0x10)] public StdTuple2D<int> Size;
    }
}
