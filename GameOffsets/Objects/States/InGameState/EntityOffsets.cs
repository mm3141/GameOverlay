namespace GameOffsets.Objects.States.InGameState
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Natives;


    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct EntityOffsets
    {
        [FieldOffset(0x08)] public IntPtr EntityDetailsPtr;
        [FieldOffset(0x10)] public StdVector ComponentListPtr;
        [FieldOffset(0x30)] public StdVector UnknownListPtr;
        [FieldOffset(0x58)] public uint Id;
        [FieldOffset(0x5C)] public byte IsValid; // 0x0C = Valid, 0x03 = Invalid
        public static byte Valid = 0x0C;
    };

    public static class EntityHelper
    {
        // 0th bit set = invalid. test byte ptr [rcx+5C],01
        // pass IsValid byte to this function.
        public static Func<byte, bool> IsValidEntity =
            new Func<byte, bool>((param) =>
            {
                return !Util.isBitSetByte(param, 0);
            });
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct EntityDetails
    {
        [FieldOffset(0x08)] public StdWString name;
        [FieldOffset(0x30)] public IntPtr ComponentLookUpPtr;
    };

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ComponentLookUpStruct
    {
        [FieldOffset(0x30)] public StdList ComponentNameAndIndexPtr;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ComponentNameAndIndexStruct
    {
        public IntPtr NamePtr;
        public int Index;
    }
}
