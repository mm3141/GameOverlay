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
        [FieldOffset(0x1088)] public int GameWindowWidth;
        [FieldOffset(0x108C)] public int GameWindowHeight;
        [FieldOffset(0x1100)] public Matrix4x4 WindowToScreenMatrix;
        [FieldOffset(0x1170)] public Vector3 Position;
        [FieldOffset(0x1244)] public float ZFar;
        [FieldOffset(0x1470)] public IntPtr IngameUi;
    }
}
