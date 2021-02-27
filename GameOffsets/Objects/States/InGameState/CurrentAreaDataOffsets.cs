namespace GameOffsets.Objects.States.InGameState
{
    using System;
    using System.Runtime.InteropServices;
    using GameOffsets.Native;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct CurrentAreaDataOffsets
    {
        [FieldOffset(0x90)] public byte MonsterLevel;
        [FieldOffset(0xF4)] public uint CurrentAreaHash;
        [FieldOffset(0x438)] public IntPtr LocalPlayerPtr;
        // Sleeping is decorations, disabled particles, effects.
        // Awake is objects like Chests, Monsters, Players, Npcs and etc.
        [FieldOffset(0x4C0)] public StdMap AwakeEntities;
        //[FieldOffset(0x4D0)] public StdMap SleepingEntities;
    }

    public static class InGameStateDataConstants
    {
        public static int NETWORK_BUBBLE_RADIUS = 100;
    }
    public static class EntityFilter
    {
        private static Func<byte, int, bool> isBitSet = (x, pos) => (x & (1 << pos)) != 0;

        public static Func<EntityNodeKey, bool> IgnoreSleepingEntities =
            new Func<EntityNodeKey, bool>((param) =>
            {
#if DEBUG
                if (param.type != 0xC0 && param.type != 0x88 && param.type != 0x00)
                {
                    Console.WriteLine($"[DEBUG] New Entity Type found: 0x{param.type:X}");
                }
#endif
                return !isBitSet(param.type, 7);
            });

        public static Func<EntityNodeKey, bool> IgnoreNothing =
            new Func<EntityNodeKey, bool>((param) => { return true; });
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
        public IntPtr Entity;
        // public int pad_0x30;

        public override string ToString()
        {
            return $"EntityPtr: {Entity.ToInt64():X}";
        }
    }
}
