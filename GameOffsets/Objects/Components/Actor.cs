namespace GameOffsets.Objects.Components
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ActorOffset
    {
        [FieldOffset(0x234)] public int AnimationId;
    }
}