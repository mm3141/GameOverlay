namespace GameOffsets.Objects.States.InGameState
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Natives;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AreaInstanceOffsets
    {
        [FieldOffset(0x78)] public IntPtr AreaDetailsPtr; // WorldAreaDatOffsets.cs
        [FieldOffset(0x90)] public byte MonsterLevel;
        [FieldOffset(0xF4)] public uint CurrentAreaHash;
        [FieldOffset(0x438)] public IntPtr LocalPlayerPtr;
        // Sleeping is decorations, disabled particles, effects.
        // Awake is objects like Chests, Monsters, Players, Npcs and etc.
        [FieldOffset(0x4C0)] public StdMap AwakeEntities;
        //[FieldOffset(0x4D0)] public StdMap SleepingEntities;
        [FieldOffset(0x640)] public TerrainStruct TerrainMetadata;
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
                return !Util.isBitSetByte(param.type, 7);
            });
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EntityNodeKey
    {
        public ushort id;
        public byte pad_0x22;
        public byte type; // 0x00 object/entity, 0xC0 object/entity-effects
        public int pad_0x24;

        public override string ToString()
        {
            return $"id: {id}, type: {type}";
        }

        public override bool Equals(object ob)
        {
            if (ob is EntityNodeKey)
            {
                EntityNodeKey c = (EntityNodeKey)ob;
                return this.id == c.id && this.type == c.type;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode() ^ this.type.GetHashCode();
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
