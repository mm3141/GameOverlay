namespace GameOffsets.Objects.Components
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct TargetableOffsets
    {
        [FieldOffset(0x00)] public ComponentHeader Header;
        [FieldOffset(0x48)] public bool isTargetable;
    }
}