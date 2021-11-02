namespace GameOffsets.Objects.Components
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ObjectMagicPropertiesOffsets
    {
        [FieldOffset(0x00)]
        public ComponentHeader Header;

        [FieldOffset(0x9C)]
        public int Rarity;
    }
}