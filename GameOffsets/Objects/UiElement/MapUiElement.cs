namespace GameOffsets.Objects.UiElement
{
    using System.Runtime.InteropServices;
    using Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct MapUiElementOffset
    {
        [FieldOffset(0x000)] public UiElementBaseOffset UiElementBase;
        [FieldOffset(0x2D8)] public StdTuple2D<float> Shift;
        [FieldOffset(0x2E0)] public StdTuple2D<float> DefaultShift;
        [FieldOffset(0x31C)] public float Zoom;
    }
}