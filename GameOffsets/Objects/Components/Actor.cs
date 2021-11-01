using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ActorOffset
    {
        [FieldOffset(0x234)] public int AnimationId;
    }
}