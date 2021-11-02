namespace GameOffsets.Objects.Components
{
    using System;
    using System.Runtime.InteropServices;

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

        /// <summary>
        ///     This is greater than zero if Vital is regenerating
        ///     For value = 0 or less than 0, Vital isn't regenerating
        /// </summary>
        public float Regeneration;

        public int Total;

        public int Current;

        /// <summary>
        ///     e.g. Clarity reserve flat Vital
        /// </summary>
        public int ReservedFlat;

        /// <summary>
        ///     e.g. Heralds reserve % Vital.
        ///     ReservedFlat does not change this value.
        ///     Note that it's an integer, this is due to 20.23% is stored as 2023
        /// </summary>
        public int ReservedPercent;

        /// <summary>
        ///     Returns current Vital in percentage (excluding the reserved vital) or returns zero in case the Vital
        ///     doesn't exists.
        /// </summary>
        /// <returns></returns>
        public int CurrentInPercent()
        {
            if (this.Total == 0)
            {
                return 0;
            }

            var vitalAfterReservedPercent = Math.Round((this.ReservedPercent / 100f) * this.Total);
            var vitalExcludingReserved = this.Total - this.ReservedFlat + vitalAfterReservedPercent;
            return (int)Math.Round(100 * this.Current / vitalExcludingReserved);
        }
    }
}