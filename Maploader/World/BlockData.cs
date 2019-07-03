using System;
using JetBrains.Annotations;

namespace Maploader.World
{
    public class BlockCoord
    {
        public BlockCoord(BlockData block, int x, int y, int z)
        {
            Block = block;
            X = x;
            Y = y;
            Z = z;
            XZ = x * 256 + z;
        }

        public BlockData Block { get; set; }
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public int XZ { get; }

        public override string ToString()
        {
            return $"{X} {Y} {Z} {Block}";
        }
    }

    public class BlockData
    {
        public BlockData(string id, long data)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Data = data;
            Version = 0;
        }

        [NotNull]
        public string Id { get; set; }

        public long Data { get; set; }

        public int Version { get; set; }

        public override string ToString()
        {
            return string.Format($"{Id}:{Data} ({Version})");
        }

        public void Reset()
        {
            Id = "minecraft:air";
            Data = 0;
            Version = 0;
        }
    }
}