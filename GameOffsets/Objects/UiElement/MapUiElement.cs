namespace GameOffsets.Objects.UiElement
{
    using System.Runtime.InteropServices;
    using Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct MapUiElementOffset
    {
        [FieldOffset(0x000)] public UiElementBaseOffset UiElementBase;
        [FieldOffset(0x270)] public StdTuple2D<float> Shift;
        [FieldOffset(0x278)] public StdTuple2D<float> DefaultShift; //new v2=(0, -20f)
        [FieldOffset(0x2B4)] public float Zoom;
    }
}
