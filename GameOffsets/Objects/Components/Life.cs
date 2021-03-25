namespace GameOffsets.Objects.Components
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct LifeOffset
    {
        [FieldOffset(0x000)] public ComponentHeader Header;
        [FieldOffset(0x180)] public StdVector BuffsPtr;
        [FieldOffset(0x1B0)] public VitalStruct Mana;
        [FieldOffset(0x1E8)] public VitalStruct EnergyShield;
        [FieldOffset(0x248)] public VitalStruct Health;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BuffStruct
    {
        [FieldOffset(0x0008)] public long BuffInternalPtr;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BuffInternalStruct
    {
        //// BuffDefination.DAT file
        [FieldOffset(0x0008)] public long Name;
        [FieldOffset(0x0010)] public float MaxTime;
        [FieldOffset(0x0014)] public float CurrTimer;
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
