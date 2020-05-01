using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Maploader.Core;
using Maploader.World;
using PapyrusCs.Database;

namespace PapyrusCs
{
    public partial class Program
    {
      private static int RunFindOptions(FindOptions opts)
        {
            var world = new World();

            int result = 0;

            if (!string.IsNullOrWhiteSpace(opts.BlockId))
            {
                result = FindBlockId(opts, world);
            }
            else
            {
                result = FindVillages(opts, world);
            }

            Console.WriteLine(_time.Elapsed);

            return result;
        }

        private class Village
        {
            public string Name { get; set; }
            public int X0 { get; set; }
            public int X1 { get; set; }
            public int Y0 { get; set; }
            public int Y1 { get; set; }
            public int Z0 { get; set; }
            public int Z1 { get; set; }

            public int XSize => Math.Abs(X0 - X1);
            public int YSize => Math.Abs(Y0 - Y1);
            public int ZSize => Math.Abs(Z0 - Z1);

            public string Center => $"{X0 + (XSize / 2)} {Y0 + (YSize / 2)} {Z0 + (ZSize / 2)}";
        }

        private static int FindVillages(FindOptions opts, World world)
        {
            try
            {
                Console.WriteLine("Find Villages Decode. Opening world...");
                world.Open(opts.MinecraftWorld);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open world at '{opts.MinecraftWorld}'!. Did you specify the .../db folder?");
                Console.WriteLine("The reason was:");
                Console.WriteLine(ex.Message);
                {
                    return -1;
                }
            }

            int i = 0;
            int nextout = 2000;
            foreach (var k in world.Keys)
            {
                if (i > nextout)
                {
                    Console.WriteLine("Key {0}", i);
                    nextout += 50000;
                }

                i++;

                if (k.Length > 8 && k[8] == 47)
                {
                    continue;
                }

                var village = new [] {'V', 'I', 'L', 'L', 'A','G','E'}.Select(x => (byte)x).ToArray();
                if (k.Locate(village).Length == 0) 
                    continue;

                var data = world.GetData(k);

                var golem = new[] { 'M', 'T', 'i','c','k'}.Select(x => (byte)x).ToArray();

                var searchResults = data.Locate(golem);
                if (searchResults != null && searchResults.Length > 0)
                {

                    var s = new MemoryStream(data);
                    var nbt = new fNbt.NbtReader(s, false);

                    var v = new Village();

                    var result = nbt.ReadToFollowing();
                    if (nbt.IsCompound)
                    {
                        result = nbt.ReadToFollowing();
                        while (result)
                        {
                            if (nbt.HasName && nbt.HasValue)
                            {
                                var n = nbt.TagName;
                                var val = nbt.ReadValue();
                                //Console.WriteLine($"\t\t{n}: {val}");

                                switch (n)
                                {
                                    case "X0":
                                        v.X0 = Convert.ToInt32(val);
                                        break;
                                    case "X1":
                                        v.X1 = Convert.ToInt32(val);
                                        break;
                                    case "Y0":
                                        v.Y0 = Convert.ToInt32(val);
                                        break;
                                    case "Y1":
                                        v.Y1 = Convert.ToInt32(val);
                                        break;
                                    case "Z0":
                                        v.Z0 = Convert.ToInt32(val);
                                        break;
                                    case "Z1":
                                        v.Z1 = Convert.ToInt32(val);
                                        break;
                                    default:
                                        break;
                                }

                            }

                            result = nbt.ReadToFollowing();

                        }

                        if (v.Y0 > 150)
                        {
                            Console.WriteLine("Found golem in {0}", string.Join(" ", k.Select(x => $"{x:X2}")));
                            Console.WriteLine($"Center us {v.Center}");
                            return 0;

                        }


                    }
                    //var nbt2 = new fNbt.NbtFile();
                    //nbt2.LoadFromBuffer(data, 0, data.Length, NbtCompression.None);


                }

            }

            return 0;
        }

        private static int FindBlockId(FindOptions opts, World world)
        {
            world.ChunkPool = new ChunkPool();
            try
            {
                Console.WriteLine("Find Blocks. Opening world...");
                world.Open(opts.MinecraftWorld);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open world at '{opts.MinecraftWorld}'!. Did you specify the .../db folder?");
                Console.WriteLine("The reason was:");
                Console.WriteLine(ex.Message);
                {
                    return -1;
                }
            }

            int i = 0;
            int nextout = 2000;
            var keys = world.OverworldKeys.Select(x => new LevelDbWorldKey2(x)).GroupBy(x => x.XZ).Select(x => x.Key).ToList();
            Console.WriteLine(keys.Count());

            _time = Stopwatch.StartNew();
            foreach (var key in keys)
            {
                i++;

                var X = (int) ((ulong) key >> 32);
                var Z = (int) ((ulong) key & 0xffffffff);
                var cd = world.GetChunkData(X, Z);
                var c = world.GetChunk(cd.X, cd.Z, cd);

                var bells = c.Blocks.Where(x => x.Value.Block.Id == opts.BlockId);
                foreach (var b in bells)
                {
                    Console.WriteLine($"Chunk {X} {Z} {c.X} {c.Z} -- Block {b.Value.X + c.X * 16} {b.Value.Z + c.Z * 16} {b.Value.Y} {b.Value.Block.Id}");
                }

                if (i > nextout)
                {
                    nextout += 2000;
                    Console.WriteLine($"Reading key {i} {_time.Elapsed} {i / (_time.ElapsedMilliseconds / 1000.0)}");
                }
            }

            Console.WriteLine($"Reading key {i}");
            return 0;
        }
    }
}
