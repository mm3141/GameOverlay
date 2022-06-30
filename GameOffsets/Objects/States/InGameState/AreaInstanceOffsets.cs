namespace GameOffsets.Objects.States.InGameState
{
    using System;
    using System.Runtime.InteropServices;
    using Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AreaInstanceOffsets
    {
        [FieldOffset(0xA0)] public byte MonsterLevel;
        [FieldOffset(0x104)] public uint CurrentAreaHash;

        // Env which are activated. Keys can be found in Environments.dat file.
        [FieldOffset(0x740)] public StdVector Environments; // EnvironmentStruct
        [FieldOffset(0x770)] public IntPtr ServerDataPtr;

        [FieldOffset(0x778)] public IntPtr LocalPlayerPtr;

        [FieldOffset(0x828)] public StdMap AwakeEntities;
        // [FieldOffset(0x648)] public StdMap SleepingEntities; // always after awake entities.

        [FieldOffset(0x9C0)] public TerrainStruct TerrainMetadata;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EnvironmentStruct
    {
        public ushort Key;
        public ushort Value0;
        public float Value1;
    }

    public static class AreaInstanceConstants
    {
        // should be few points less than the real value (200)
        // real value manually calculating by checking when entity leave the bubble.
        // BTW real value is different for different entity types.
        // Updating it to 150 to remove false positive.
        public const int NETWORK_BUBBLE_RADIUS = 150;
    }

    public static class EntityFilter
    {
        public static Func<EntityNodeKey, bool> IgnoreVisualsAndDecorations = param =>
        {
            // from the game code
            //     if (0x3fffffff < *(uint *)(lVar1 + 0x60)) {}
            //     CMP    dword ptr [RSI + 0x60],0x40000000
            return param.id < 0x40000000;
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EntityNodeKey
    {
        public uint id;
        public int pad_0x24;

        public override string ToString()
        {
            return $"id: {this.id}";
        }

        public override bool Equals(object ob)
        {
            if (ob is EntityNodeKey c)
            {
                return this.id == c.id;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }

        public static bool operator ==(EntityNodeKey left, EntityNodeKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityNodeKey left, EntityNodeKey right)
        {
            return !(left == right);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EntityNodeValue
    {
        public IntPtr EntityPtr;
        // public int pad_0x30;

        public override string ToString()
        {
            return $"EntityPtr: {this.EntityPtr.ToInt64():X}";
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct TerrainStruct
    {
        //[FieldOffset(0x08)] public IntPtr Unknown0;
        [FieldOffset(0x18)] public StdTuple2D<long> TotalTiles;

        [FieldOffset(0x28)] public StdVector TileDetailsPtr; // TileStructure

        //[FieldOffset(0x40)] public StdTuple2D<long> TotalTilesPlusOne;
        //[FieldOffset(0x50)] public StdVector Unknown1;
        //[FieldOffset(0x68)] public StdVector Unknown2;
        [FieldOffset(0x80)] public long TileHeightMultiplier;

        //[FieldOffset(0x8C)] public StdTuple2D<int> TotalTilesAgain;
        [FieldOffset(0xD8)] public StdVector GridWalkableData;
        [FieldOffset(0xF0)] public StdVector GridLandscapeData;
        [FieldOffset(0x108)] public int BytesPerRow; // for walkable/landscape data.
        public static float TileHeightFinalMultiplier = 7.8125f;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x38)]
    public struct TileStructure // size 0x38
    {
        public IntPtr SubTileDetailsPtr; // SubTileStruct
        public IntPtr TgtFilePtr; // TgtFileStruct
        public StdVector EntitiesList;
        public IntPtr PAD_0x28;
        public short TileHeight;
        public byte PAD_0x32;
        public byte PAD_0x33;
        public byte tileIdX;
        public byte tileIdY;
        public byte RotationSelector;
        public byte PAD_0x37;
        public static int TileToGridConversion = 0x17; // 23
        public static float TileToWorldConversion = 250f; // 250
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SubTileStruct
    {
        public StdVector SubTileHeight; // tile has 23x23 subtiles.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TgtFileStruct
    {
        public IntPtr Vtable;
        public StdWString TgtPath;
        //public int Unknown0;
        //public int Unknown1;
        //public IntPtr TgtDetailPtr; // TgtDetailStruct
        //public int Unknown2;
        //public int Unknown3;
        //public int Unknown4;
    }

    //[StructLayout(LayoutKind.Explicit, Pack = 1)]
    //public struct TgtDetailStruct
    //{
    //    [FieldOffset(0x00)] public StdWString name;
    //}
}
