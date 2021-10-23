namespace GameOffsets.Objects.States.InGameState
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ItemStruct
    {
        [FieldOffset(0x00)] public IntPtr VTablePtr;
        [FieldOffset(0x08)] public IntPtr EntityDetailsPtr;
        [FieldOffset(0x10)] public StdVector ComponentListPtr;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct EntityOffsets
    {
        [FieldOffset(0x00)] public ItemStruct ItemBase;
        [FieldOffset(0x30)] public StdVector UnknownListPtr;
        [FieldOffset(0x60)] public uint Id;
        [FieldOffset(0x64)] public byte IsValid; // 0x0C = Valid, 0x03 = Invalid
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

        public static Func<byte, bool> IsFriendly =
            new Func<byte, bool>((param) =>
            {
                return (param & 0x7F) == 0x01;
            });
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct EntityDetails
    {
        [FieldOffset(0x08)] public StdWString name;
        [FieldOffset(0x30)] public IntPtr ComponentLookUpPtr;
    };

    // DRY this with LoadedFilesOffset
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ComponentLookUpStruct
    {
        public IntPtr Unknown0;
        public IntPtr Unknown1;
        public StdVector Unknown2;
        public long PAD_00;
        public IntPtr ComponentArray; // ComponentArrayStructure
        public long Capacity; // actually, it's Capacity number - 1.
        public int Unknown3; // byte + padd
        public float Unknown4;
        public long Counter;
    };

    // DRY this with LoadedFilesOffset
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ComponentArrayStructure
    {
        public byte Flag0;
        public byte Flag1;
        public byte Flag2;
        public byte Flag3;
        public byte Flag4;
        public byte Flag5;
        public byte Flag6;
        public byte Flag7;

        public ComponentNameAndIndexStruct Pointer0;
        public ComponentNameAndIndexStruct Pointer1;
        public ComponentNameAndIndexStruct Pointer2;
        public ComponentNameAndIndexStruct Pointer3;
        public ComponentNameAndIndexStruct Pointer4;
        public ComponentNameAndIndexStruct Pointer5;
        public ComponentNameAndIndexStruct Pointer6;
        public ComponentNameAndIndexStruct Pointer7;
        public static byte InValidPointerFlagValue = 0xFF;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ComponentNameAndIndexStruct
    {
        public IntPtr NamePtr;
        public int Index;
        public int deadcode;
    }
}
