using System;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

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
        public BlockData(string id, Dictionary<string, Object> data)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Data = data;
            Version = 0;
        }

        [NotNull]
        public string Id { get; set; }

        public Dictionary<string, Object> Data { get; set; }

        public int Version { get; set; }

        public override string ToString()
        {
            string data = "[" + string.Join(",", Data.Select(pair => $"\"{pair.Key}\":{pair.Value}")) + "]";
            return string.Format($"{Id}:{data} ({Version})");
        }

        public void Reset()
        {
            Id = "minecraft:air";
            Data.Clear();
            Version = 0;
        }
    }
}