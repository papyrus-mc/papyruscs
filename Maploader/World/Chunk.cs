using System;
using System.Collections.Generic;

namespace Maploader.World
{
    public class Chunk
    {
        public int X { get; }
        public int Z { get; }

        public Chunk(int x, int z)
        {
            X = x;
            Z = z;
        }

        public override string ToString()
        {
            return $"Chunk {X},{Z}";
        }

        public Dictionary<UInt32, BlockCoord> Blocks { get; } = new Dictionary<UInt32, BlockCoord>();


        public void SetBlockId(int x, int y, int z, BlockData data, bool noException = false)
        {

            if (data.Id == "minecraft:air")
                return;
            var coord = CreateKey(x, y, z);
            if (!Blocks.ContainsKey(coord))
            {
                var blockCoord = new BlockCoord(data, x, y, z);
                Blocks[coord] = blockCoord;
            }
            else
            {
                if (!noException)
                {
                    throw new ArgumentException($"Key {x},{y},{z} already exists");
                }
            }
        }

        public UInt32 CreateKey(int x, int y, int z)
        {
            return (UInt32) (((x & 0xFF) << 24) + ((z & 0xFF) << 16) + ((y & 0xFF) << 8));
        }
    }
}