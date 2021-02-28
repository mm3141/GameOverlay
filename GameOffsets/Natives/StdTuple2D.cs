namespace GameOffsets.Natives
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StdTuple2D<T>
    {
        public T X;
        public T Y;
    }
}
