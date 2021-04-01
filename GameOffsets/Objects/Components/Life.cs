namespace GameOffsets.Objects.Components
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct LifeOffset
    {
        [FieldOffset(0x000)] public ComponentHeader Header;
        [FieldOffset(0x180)] public StdVector StatusEffectPtr;
        [FieldOffset(0x1B0)] public VitalStruct Mana;
        [FieldOffset(0x1E8)] public VitalStruct EnergyShield;
        [FieldOffset(0x248)] public VitalStruct Health;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StatusEffectKeyValueStruct
    {
        public ushort key; // just a counter (keeps on incrementing when new buff is added).
        public byte PAD_02;
        public byte PAD_03;
        public int PAD_04;
        public IntPtr Value;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct StatusEffectStruct
    {
        // [FieldOffset(0x0000)] public IntPtr UselessPtr;
        [FieldOffset(0x0008)] public IntPtr BuffDefinationPtr; //// BuffDefination.DAT file
        [FieldOffset(0x0010)] public float TotalTime;
        [FieldOffset(0x0014)] public float TimeLeft;
        [FieldOffset(0x0030)] public ushort Charges; // 2 bytes long but 1 is enough
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VitalStruct
    {
        public IntPtr PtrToLifeComponent;
        //// This is greater than zero if Vital is regenerating
        //// For value = 0 or less than 0, Vital isn't regenerating
        public float Regeneration;
        public int Total;
        //// e.g. ICICLE MINE reserve flat Vital
        public int ReservedFlat;
        public int Current;
        //// e.g. HERALD reserve % Vital.
        //// ReservedFlat does not change this value.
        public int ReservedPercent;
    }
}
