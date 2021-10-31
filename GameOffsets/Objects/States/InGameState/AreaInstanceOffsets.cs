namespace GameOffsets.Objects.States.InGameState {
    using System;
    using System.Runtime.InteropServices;
    using Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AreaInstanceOffsets {
        [FieldOffset(0x80)] public IntPtr AreaDetailsPtr; // WorldAreaDatOffsets.cs
        [FieldOffset(0xB0)] public byte MonsterLevel;
        [FieldOffset(0x114)] public uint CurrentAreaHash;
        [FieldOffset(0x550)] public StdVector OverlayLeagueMechanic; //int, float.
        [FieldOffset(0x580)] public IntPtr ServerDataPtr;

        [FieldOffset(0x588)] public IntPtr LocalPlayerPtr;

        // Sleeping is decorations, disabled particles, effects.
        // Awake is objects like Chests, Monsters, Players, Npcs and etc.
        [FieldOffset(0x638)] public StdMap AwakeEntities;

        //[FieldOffset(0x5C0)] public StdMap SleepingEntities; // always after awake entities.
        [FieldOffset(0x7D8)] public TerrainStruct TerrainMetadata;
    }

    public static class AreaInstanceConstants {
        // should be few points less than the real value (2178)
        // real value manually calculating by checking when entity leave the bubble.
        // Updating it to 1630 to remove false positive.
        // 1630 render distance is around 150 grid distance. ( divide by 10.87 )
        // 1 full screen size is around 14xx so this is slightly more than 1 screen size.
        public const int NETWORK_BUBBLE_RADIUS = 1630;
    }

    public static class EntityFilter {
        public static Func<EntityNodeKey, bool> IgnoreSleepingEntities = param => {
            // from the game code
            //     if (0x3fffffff < *(uint *)(lVar1 + 0x60)) {}
            //     CMP    dword ptr [RSI + 0x60],0x40000000
            return param.id < 0x40000000;
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EntityNodeKey {
        public uint id;
        public int pad_0x24;

        public override string ToString() {
            return $"id: {id}";
        }

        public override bool Equals(object ob) {
            if (ob is EntityNodeKey c) {
                return id == c.id;
            }

            return false;
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }

        public static bool operator ==(EntityNodeKey left, EntityNodeKey right) {
            return left.Equals(right);
        }

        public static bool operator !=(EntityNodeKey left, EntityNodeKey right) {
            return !(left == right);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EntityNodeValue {
        public IntPtr EntityPtr;
        // public int pad_0x30;

        public override string ToString() {
            return $"EntityPtr: {EntityPtr.ToInt64():X}";
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct TerrainStruct {
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
        public ushort PAD_0x32;
        public ushort PAD_0x34;
        public byte RotationSelector;
        public byte PAD_0x37;
        public static int TileToGridConversion = 0x17; // 23
        public static float TileToWorldConversion = 250f; // 250
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SubTileStruct {
        public StdVector SubTileHeight; // tile has 23x23 subtiles.
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct TgtFileStruct {
        [FieldOffset(0x08)] public StdWString TgtPath;
        [FieldOffset(0x30)] public IntPtr TgtDetailPtr; // TgtDetailStruct
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct TgtDetailStruct {
        [FieldOffset(0x00)] public StdWString name;
    }
}