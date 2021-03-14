namespace GameOffsets.Objects.States
{
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct InGameStateOffset
    {
        [FieldOffset(0x30)] public IntPtr LocalData;
        [FieldOffset(0x0540)] public IntPtr UiRootPtr;
        // [FieldOffset(0x1088)] public StdTuple2D<int> GameWindowSize;

        // First value is changing when we change the screen size (ratio)
        // 4 bytes before the matrix doesn't change
        [FieldOffset(0x1100)] public Matrix4x4 WorldToScreenMatrix;
        // [FieldOffset(0x1170)] public Vector3 Position;
        // [FieldOffset(0x1244)] public float ZFar;
        [FieldOffset(0x1470)] public IntPtr IngameUi;
    }

    public static class ImportantUiElementsIndexes
    {
        // Careful when counting, every index starts from 0.
        public static int[] LargeMapIndex = new int[] { 1, 3, 1};
        public static int[] MiniMapIndex = new int[] { 1, 3, 2 };
    }
}
