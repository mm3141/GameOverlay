using System.Runtime.InteropServices;

namespace GameOffsets.Objects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AreaChangeOffset
    {
        public int counter;
    }
}