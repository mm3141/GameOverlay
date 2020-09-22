namespace GameOffsets.RemoteMemoryObjects.States.InGameStateObjects
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct InGameStateDataOffsets
    {
        //[FieldOffset(0x68)] public IntPtr CurrentArea;
        [FieldOffset(0x80)] public byte MonsterLevel;
        [FieldOffset(0xE4)] public uint CurrentAreaHash;
        //[FieldOffset(0xF8)] public NativePtrArray MapStats;
        //[FieldOffset(0x218)] public IntPtr IngameStatePtr;
        //[FieldOffset(0x2F8)] public IntPtr IngameStatePtr2;
        //[FieldOffset(0x408)] public IntPtr LocalPlayer;
        //[FieldOffset(0x124)] public IntPtr LabDataPtr;
        //[FieldOffset(0x490)] public IntPtr EntityList;
        //[FieldOffset(0x498)] public IntPtr EntitiesCount;
        //[FieldOffset(0x610)] public TerrainData Terrain;
    }
}
