namespace GameOffsets.Objects.Components
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct LifeOffset
    {
        [FieldOffset(0x000)] public ComponentHeader Header;
        [FieldOffset(0x1A8)] public VitalStruct Mana;
        [FieldOffset(0x1E0)] public VitalStruct EnergyShield;
        [FieldOffset(0x240)] public VitalStruct Health;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VitalStruct
    {
        public IntPtr PtrToLifeComponent;
        //// This is greater than zero if Vital is regenerating
        //// For value = 0 or less than 0, Vital isn't regenerating
        public float Regeneration;
        public int Total;
        public int Current;
        //// e.g. ICICLE MINE reserve flat Vital
        public int ReservedFlat;
        //// e.g. HERALD reserve % Vital.
        //// ReservedFlat does not change this value.
        public int ReservedPercent;
    }
}
