namespace GameOffsets.Objects.States
{
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct InGameStateOffset
    {
        [FieldOffset(0x030)] public IntPtr LocalData;
        [FieldOffset(0x420)] public IntPtr ServerData; // currently not using it.
        [FieldOffset(0x548)] public IntPtr UiRootPtr;
        [FieldOffset(0x710)] public CameraStructure CameraData;
        [FieldOffset(0xB08)] public IntPtr IngameUi;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct CameraStructure
    {
        [FieldOffset(0x00)] public IntPtr CodePtr;
        // [FieldOffset(0x8)] public int Width;
        // [FieldOffset(0xC)] public int Height;

        // First value is changing when we change the screen size (ratio)
        // 4 bytes before the matrix doesn't change
        // matrix is duplicated, meaning when first matrix finish, 2nd one start with exact same values.
        [FieldOffset(0x80)] public Matrix4x4 WorldToScreenMatrix;
        // [FieldOffset(0xF0)] public Vector3 Position;
        // [FieldOffset(0x1C4)] public float ZFar;
    }
}
