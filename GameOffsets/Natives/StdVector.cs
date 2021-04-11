namespace GameOffsets.Natives
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StdVector
    {
        public IntPtr First;
        public IntPtr Last;
        public IntPtr End;

        /// <summary>
        /// Counts the number of elements in the StdVector.
        /// </summary>
        /// <param name="elementSize">Number of bytes in 1 element.</param>
        /// <returns></returns>
        public long TotalElements(int elementSize)
        {
            return (Last.ToInt64() - First.ToInt64()) / elementSize;
        }
    }
}
