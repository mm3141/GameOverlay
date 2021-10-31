namespace GameOffsets.Objects.Components {
    using System;
    using System.Runtime.InteropServices;
    using Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BuffsOffsets {
        [FieldOffset(0x000)] public ComponentHeader Header;
        [FieldOffset(0x158)] public StdVector StatusEffectPtr;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct StatusEffectStruct {
        [FieldOffset(0x0008)] public IntPtr BuffDefinationPtr; //// BuffDefination.DAT file
        [FieldOffset(0x0018)] public float TotalTime;

        [FieldOffset(0x001C)] public float TimeLeft;

        // [FieldOffset(0x0020)] public float unknown0; // always set to 1.
        [FieldOffset(0x0028)] public uint SourceEntityId;

        //[FieldOffset(0x0030)] public long Unknown1;
        //[FieldOffset(0x0038)] public int unknown2;
        [FieldOffset(0x003E)] public ushort Charges; // 2 bytes long but 1 is enough

        public override string ToString() {
            var maxTime = float.IsInfinity(TotalTime) ? "Inf" : TotalTime.ToString();
            var timeLeft = float.IsInfinity(TimeLeft) ? "Inf" : TimeLeft.ToString();
            return $"BuffDefinationPtr: {BuffDefinationPtr.ToInt64():X}, MaxTime: {maxTime}, " +
                   $"TimeLeft: {timeLeft}, Charges: {Charges}, " +
                   $"Source Entity Id: {SourceEntityId}";
        }
    }
}