namespace GameOffsets.Objects.Components
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BuffsOffsets
    {
        [FieldOffset(0x000)] public ComponentHeader Header;
        [FieldOffset(0x158)] public StdVector StatusEffectPtr;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct StatusEffectStruct
    {
        [FieldOffset(0x0008)] public IntPtr BuffDefinationPtr; //// BuffDefination.DAT file
        [FieldOffset(0x0018)] public float TotalTime;
        [FieldOffset(0x001C)] public float TimeLeft;
        [FieldOffset(0x003E)] public ushort Charges; // 2 bytes long but 1 is enough

        public override string ToString()
        {
            return $"BuffDefinationPtr: {this.BuffDefinationPtr.ToInt64():X}, MaxTime: {this.TotalTime}, " +
                $"TimeLeft: {this.TimeLeft}, Charges: {this.Charges}";
        }
    }
}
