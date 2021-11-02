namespace GameOffsets.Natives
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Not sure the name of this structure. POE game uses it in a lot of different places
    ///     it might be a stdbucket, std unordered_set or std_unordered_multiset.
    ///     TODO: Create c++ HelloWorld program,
    ///     Create these structures (name it var),
    ///     Fill 1 value,
    ///     Use cheat engine on that HelloWorld Program.
    ///     HINT: use cout << &var << endl; to print memory address.
    ///     NOTE: A reader function that uses this datastructure exists
    ///     in SafeMemoryHandle class. If you modify this datastructure
    ///     modify that function too.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StdBucket
    {
        public long PAD_00;
        public IntPtr Data; // ComponentArrayStructure
        public long Capacity; // actually, it's Capacity number - 1.
        public int Unknown3; // byte + padd
        public float Unknown4;
        public int PAD_20; // it's actually Counter (i.e. amount of items in the bucket) but unstable, do not use this.
        public int PAD_24;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StdBucketNode<TValue>
        where TValue : struct
    {
        public byte Flag0;
        public byte Flag1;
        public byte Flag2;
        public byte Flag3;
        public byte Flag4;
        public byte Flag5;
        public byte Flag6;
        public byte Flag7;

        public TValue Pointer0;
        public TValue Pointer1;
        public TValue Pointer2;
        public TValue Pointer3;
        public TValue Pointer4;
        public TValue Pointer5;
        public TValue Pointer6;
        public TValue Pointer7;

        public static byte InValidPointerFlagValue = 0xFF;
    }
}