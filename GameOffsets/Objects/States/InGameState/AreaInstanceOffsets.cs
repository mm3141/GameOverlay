namespace GameOffsets.Objects.States.InGameState
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AreaInstanceOffsets
    {
        [FieldOffset(0x80)] public IntPtr AreaDetailsPtr; // WorldAreaDatOffsets.cs
        [FieldOffset(0xA0)] public byte MonsterLevel;
        [FieldOffset(0x104)] public uint CurrentAreaHash;
        [FieldOffset(0x500)] public IntPtr LocalPlayerPtr;
        // Sleeping is decorations, disabled particles, effects.
        // Awake is objects like Chests, Monsters, Players, Npcs and etc.
        [FieldOffset(0x5B0)] public StdMap AwakeEntities;
        //[FieldOffset(0x5C0)] public StdMap SleepingEntities; // always after awake entities.
        [FieldOffset(0x750)] public TerrainStruct TerrainMetadata;
    }

    public static class AreaInstanceConstants
    {
        // should be few points less than the real value (2178)
        // real value manually calculating by checking when entity leave the bubble.
        // Updating it to 1630 to remove false positive.
        // 1630 render distance is around 150 grid distance. ( divide by 10.87 )
        // 1 full screen size is around 14xx so this is slightly more than 1 screen size.
        public static int NETWORK_BUBBLE_RADIUS = 1630;
    }

    public static class EntityFilter
    {
        public static Func<EntityNodeKey, bool> IgnoreSleepingEntities =
            new Func<EntityNodeKey, bool>((param) =>
            {
                // from the game code
                //     if (0x3fffffff < *(uint *)(lVar1 + 0x60)) {}
                //     CMP    dword ptr [RSI + 0x60],0x40000000
                return param.id < 0x40000000;
            });
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EntityNodeKey
    {
        public uint id;
        public int pad_0x24;

        public override string ToString()
        {
            return $"id: {id}";
        }

        public override bool Equals(object ob)
        {
            if (ob is EntityNodeKey c)
            {
                return this.id == c.id;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EntityNodeValue
    {
        public IntPtr EntityPtr;
        // public int pad_0x30;

        public override string ToString()
        {
            return $"EntityPtr: {EntityPtr.ToInt64():X}";
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct TerrainStruct
    {
        [FieldOffset(0xD8)] public StdVector WalkableData;
        [FieldOffset(0xF0)] public StdVector LandscapeData;
        [FieldOffset(0x108)] public int BytesPerRow;

        public override string ToString()
        {
            return
                $"Walkable Data: {this.WalkableData.TotalElements(1)}, " +
                $"Landscape Data: {this.LandscapeData.TotalElements(1)}, " +
                $"Bytes Per Row: {this.BytesPerRow}";
        }
    }
}
