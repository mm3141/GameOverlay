namespace GameOffsets.Objects.UiElement
{
    using System.Runtime.InteropServices;
    using Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct MapUiElementOffset
    {
        [FieldOffset(0x000)] public UiElementBaseOffset UiElementBase;
        [FieldOffset(0x2E8)] public StdTuple2D<float> Shift;
        [FieldOffset(0x2F0)] public StdTuple2D<float> DefaultShift;
        [FieldOffset(0x32C)] public float Zoom;
    }
}
