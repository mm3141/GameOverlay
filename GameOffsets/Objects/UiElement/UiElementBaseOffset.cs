namespace GameOffsets.Objects.UiElement
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct UiElementBaseOffset
    {
        [FieldOffset(0x000)] public IntPtr Vtable;
        [FieldOffset(0x018)] public IntPtr Self;
        [FieldOffset(0x020)] public StdVector ChildrensPtr0; // both points to same children UiElements.
        [FieldOffset(0x038)] public StdVector ChildrensPtr1; // both points to same children UiElements.
        [FieldOffset(0x060)] public StdWString Id;
        // Following Ptr is basically pointing to InGameState+0x458.
        // No idea what InGameState+0x458 is pointing to.
        [FieldOffset(0x088)] public IntPtr UnknownPtr;
        [FieldOffset(0x090)] public IntPtr ParentPtr; // UiElement.
        [FieldOffset(0x098)] public StdTuple2D<float> RelativePosition;
        [FieldOffset(0x108)] public float Scale;
        [FieldOffset(0x110)] public int RelativeIsVisible;
        [FieldOffset(0x130)] public StdTuple2D<float> Size;
        // Tooltip????
    }
}
