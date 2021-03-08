namespace GameOffsets.Natives
{
    using System;

    public static class Util
    {
        public static Func<byte, int, bool> isBitSetByte = (x, pos) => (x & (1 << pos)) != 0;
        public static Func<uint, int, bool> isBitSetUint = (x, pos) => (x & (1 << pos)) != 0;
    }
}
