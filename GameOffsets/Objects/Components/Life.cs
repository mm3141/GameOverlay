namespace GameOffsets.Objects.Components
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct LifeOffset
    {
        [FieldOffset(0x000)] public ComponentHeader Header;
        [FieldOffset(0x190)] public VitalStruct Mana;
        [FieldOffset(0x1C8)] public VitalStruct EnergyShield;
        [FieldOffset(0x228)] public VitalStruct Health;
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
        ///     Final Reserved amount of Vital after all the calculations.
        /// </summary>
        public int ReservedTotal => (int)Math.Ceiling(this.ReservedPercent / 10000f * this.Total) + this.ReservedFlat;

        /// <summary>
        ///     Final un-reserved amount of Vital after all the calculations.
        /// </summary>
        public int Unreserved => this.Total - this.ReservedTotal;

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

            return (int)Math.Round(100d * this.Current / this.Unreserved);
        }
    }
}