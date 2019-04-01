using System;
using System.Collections.Generic;

namespace Maploader.Source
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

        public Dictionary<Coordinate, BlockData> Blocks { get; } = new Dictionary<Coordinate, BlockData>();


        public void SetBlockId(int x, int y, int z, BlockData data, bool noException = false)
        {
            if (data.Id == "minecraft:air")
                return;
            Coordinate coord = new Coordinate(x, y, z);
            if (!Blocks.ContainsKey(coord))
            {
                Blocks[coord] = data;
            }
            else
            {
                if (!noException)
                {
                    throw new ArgumentException($"Key {x},{y},{z} already exists");
                }
            }
        }
    }
}