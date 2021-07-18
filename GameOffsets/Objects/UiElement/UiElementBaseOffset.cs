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
        [FieldOffset(0x020)] public StdVector ChildrensPtr; // both points to same children UiElements.
        // [FieldOffset(0x038)] public StdVector ChildrensPtr1; // both points to same children UiElements.
        [FieldOffset(0x058)] public StdTuple2D<float> PositionModifier;
        [FieldOffset(0x060)] public StdWString Id;
        [FieldOffset(0x082)] public byte ScaleIndex;
        // Following Ptr is basically pointing to InGameState+0x458.
        // No idea what InGameState+0x458 is pointing to.
        [FieldOffset(0x088)] public IntPtr UnknownPtr;
        [FieldOffset(0x090)] public IntPtr ParentPtr; // UiElement.
        [FieldOffset(0x098)] public StdTuple2D<float> RelativePosition; // variable
        [FieldOffset(0x0B0)] public float LocalScaleMultiplier;
        // [FieldOffset(0x108)] public float Scale;
        [FieldOffset(0x110)] public uint Flags; // variable
        [FieldOffset(0x130)] public StdTuple2D<float> UnscaledSize; // variable
        // Tooltip????
    }

    public static class UiElementBaseFuncs
    {
        private const int SHOULD_MODIFY_BINARY_POS = 0x0A;
        private const int IS_VISIBLE_BINARY_POS = 0x0B;

        public const int SCALE_INDEX_1 = 0x04;
        public const int SCALE_INDEX_2 = 0x00;
        public const int SCALE_INDEX_3 = 0x02;

        public static Func<uint, bool> IsVisibleChecker = new Func<uint, bool>((param) =>
        {
            return Util.isBitSetUint(param, IS_VISIBLE_BINARY_POS);
        });

        public static Func<uint, bool> ShouldModifyPos = new Func<uint, bool>((param) =>
        {
            return Util.isBitSetUint(param, SHOULD_MODIFY_BINARY_POS);
        });

        public static Func<uint, bool> Unknown1 = new Func<uint, bool>((param) =>
        {
            return Util.isBitSetUint(param, 0x02);
        });

        public static Func<uint, bool> Unknown2 = new Func<uint, bool>((param) =>
        {
            return Util.isBitSetUint(param, 0x03);
        });

        public static Func<uint, bool> Unknown3 = new Func<uint, bool>((param) =>
        {
            return Util.isBitSetUint(param, 0x13);
        });
    }
}
