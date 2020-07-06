using System;
using System.Collections.Generic;
using System.Diagnostics;
using Maploader.Core;

namespace Maploader.World
{
    public class Chunk : IResettable
    {
        public int X { get; set; }
        public int Z { get; set; }

        public Chunk()
        {
            X = 0;
            Z = 0;
        }

        public Chunk(int x, int z)
        { 
            X = x;
            Z = z;
        }

        public override string ToString()
        {
            return $"Chunk {X},{Z}";
        }

        public void Reset()
        {
            foreach (var b in Blocks)
            {
                b.Value.Block.Reset();
            }
        }

        public Dictionary<UInt32, BlockCoord> Blocks { get; } = new Dictionary<UInt32,  BlockCoord>();

        public void SetBlockId(int x, int y, int z, ref BlockData data, bool noException = false)
        {

            if (data.Id == "minecraft:air")
                return;

            var coord = CreateKey(x, y, z);

            if (data.Data.Count < 0)
                Console.WriteLine();

            //if (data.Id == "minecraft:mob_spawner")
            //{
            //    var newcoord = CreateKey(x, 240, z);

            //    if (!Blocks.ContainsKey(newcoord))
            //    {
            //        var blockCoord2 = new BlockCoord(new BlockData("minecraft:coral_block", 1), x, 240, z);
            //        Blocks[newcoord] = blockCoord2;
            //        Console.WriteLine("Found Spaner");
            //    }
            //}
            //if (data.Id == "minecraft:chest")
            //{
            //    var newcoord = CreateKey(x, 240, z);

            //    if (!Blocks.ContainsKey(newcoord))
            //    {
            //        var blockCoord2 = new BlockCoord(new BlockData("minecraft:coral_block", 3), x, 240, z);
            //        Blocks[newcoord] = blockCoord2;
            //        Console.WriteLine("Found Chest");
            //    }
            //}

            if (!Blocks.ContainsKey(coord))
            {
                var blockCoord = new  BlockCoord(data, x, y, z);
                Blocks[coord] = blockCoord;
            }
            else
            {
                Blocks[coord].Block = data;
            }
        }

        public UInt32 CreateKey(int x, int y, int z)
        {
            return (UInt32) (((x & 0xFF) << 24) + ((z & 0xFF) << 16) + ((y & 0xFF) << 8));
        }
    }
}