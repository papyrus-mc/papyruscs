using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Maploader.Core;
using Maploader.World;
using MoreLinq.Extensions;
using PapyrusCs.Database;

namespace PapyrusCs
{
    public static class TestCommands {
        private static Stopwatch _time = new Stopwatch();

        public static void TestDbRead(TestOptions opts)
        {
            var world = new World();
            try
            {
                Console.WriteLine("Testing DB READ. Opening world...");
                world.Open(opts.MinecraftWorld);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open world at '{opts.MinecraftWorld}'!. Did you specify the .../db folder?");
                Console.WriteLine("The reason was:");
                Console.WriteLine(ex.Message);
                {
                    return;
                }
            }

            int i = 0;
            int nextout = 2000;
            var keys = world.OverworldKeys.Select(x => new LevelDbWorldKey2(x)).Where(x => x.SubChunkId == 0).ToList();
            Console.WriteLine(keys.Count());

            _time = Stopwatch.StartNew();
            Parallel.ForEach(keys, new ParallelOptions() { MaxDegreeOfParallelism = opts.Threads }, key =>
              {
                  Interlocked.Increment(ref i);
                //var value = world.GetChunk(key.GetIntLe(0), key.GetIntLe(4));

                var k = key.Key;
                  for (int y = 0; y < 16; y++)
                  {
                      k[9] = (byte)y;
                      world.GetData(k);
                  }

                  if (i > nextout)
                  {
                      Interlocked.Add(ref nextout, 2000);
                      Console.WriteLine($"Reading key {i} {_time.Elapsed} {i / (_time.ElapsedMilliseconds / 1000.0)}");
                  }
              });

            Console.WriteLine($"Reading key {i}");

            Console.WriteLine(_time.Elapsed);
        }

        public static void TestDecode(TestOptions opts)
        {
            var world = new World();

            world.ChunkPool = new ChunkPool();
            try
            {
                Console.WriteLine("Testing Decode. Opening world...");
                world.Open(opts.MinecraftWorld);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open world at '{opts.MinecraftWorld}'!. Did you specify the .../db folder?");
                Console.WriteLine("The reason was:");
                Console.WriteLine(ex.Message);
                {
                    return;
                }
            }

            int i = 0;
            int nextout = 2000;
            var keys = world.OverworldKeys.Select(x => new LevelDbWorldKey2(x)).GroupBy(x => x.XZ).Select(x => x.Key).ToList();
            Console.WriteLine(keys.Count());

            _time = Stopwatch.StartNew();
            Parallel.ForEach(keys, new ParallelOptions() { MaxDegreeOfParallelism = opts.Threads }, key =>
            {
                Interlocked.Increment(ref i);
                //var value = world.GetChunk(key.GetIntLe(0), key.GetIntLe(4));

                var k = key;
                //var gcsk = new GroupedChunkSubKeys(key);
                //var cd = world.GetChunkData(gcsk);
                //var chunk = world.GetChunk(key.X, key.Z);
                //var chunk = world.GetChunk(gcsk.Subchunks.First().Value.X, gcsk.Subchunks.First().Value.Z);
                //var chunk = world.GetChunk(key.First().X, key.First().Z);
#if true
                var X = (int)((ulong)key >> 32);
                var Z = (int)((ulong)key & 0xffffffff);
                var cd = world.GetChunkData(X, Z);
                var c = world.GetChunk(cd.X, cd.Z, cd);
#else
                var X = (int) ((ulong) key >> 32);
                var Z = (int) ((ulong) key & 0xffffffff);
                var c = world.GetChunk(X,Z);
#endif

                if (i > nextout)
                {
                    Interlocked.Add(ref nextout, 2000);
                    Console.WriteLine($"Reading key {i} {_time.Elapsed} {i / (_time.ElapsedMilliseconds / 1000.0)}");
                }
            });

            Console.WriteLine($"Reading key {i}");

            Console.WriteLine(_time.Elapsed);
        }

        public static void TestSmallFlow(TestOptions opts)
        {
            var world = new World();
            world.ChunkPool = new ChunkPool();
            try
            {
                Console.WriteLine("Testing SmallFlow. Opening world...");
                world.Open(opts.MinecraftWorld);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open world at '{opts.MinecraftWorld}'!. Did you specify the .../db folder?");
                Console.WriteLine("The reason was:");
                Console.WriteLine(ex.Message);
                {
                    return;
                }
            }

            int i = 0;
            int nextout = 2000;
            var keys = new HashSet<ulong>();
            foreach (var x in world.OverworldKeys)
            {
                var key = new LevelDbWorldKey2(x);
                if (!keys.Contains(key.XZ))
                    keys.Add(key.XZ);
            }
            //var keys = world.OverworldKeys.Select(x => new LevelDbWorldKey2(x)).GroupBy(x => x.XZ).Select(x => x.Key).ToList();
            Console.WriteLine(keys.Count());

            _time = Stopwatch.StartNew();

            //ObjectPool<ChunkData> op = new DefaultObjectPool<ChunkData>(new DefaultPooledObjectPolicy<ChunkData>());

            var tb = new TransformBlock<IEnumerable<ulong>, IReadOnlyCollection<ChunkData>>(key2 =>
            {
                var ret = new List<ChunkData>();
                foreach (var u in key2)
                {
                    var X = (int)((ulong)u >> 32);
                    var Z = (int)((ulong)u & 0xffffffff);
                    var cd = world.GetChunkData(X, Z);
                    ret.Add(cd);
                }

                return ret;
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, BoundedCapacity = 16 });

            var chunkCreator = new ActionBlock<IReadOnlyCollection<ChunkData>>(data =>
            {
                var sp = Stopwatch.StartNew();
                Chunk ck = null;
                foreach (var d in data)
                {
                    ck = world.GetChunk(d.X, d.Z, d);
                    world.ChunkPool.Return(ck);
                }

                Interlocked.Add(ref i, data.Count);
                if (i > nextout)
                {
                    Interlocked.Add(ref nextout, 2000);
                    Console.WriteLine($"Reading key {i} {_time.Elapsed} {i / (_time.ElapsedMilliseconds / 1000.0)}");
                    if (ck != null)
                    {
                        Console.WriteLine(ck.Blocks.Count());
                    }
                }

                Console.WriteLine(sp.Elapsed);
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 4, BoundedCapacity = 16 });

            tb.LinkTo(chunkCreator, new DataflowLinkOptions() { PropagateCompletion = true });

            int i2 = 0;
            foreach (var k in keys.Batch(256))
            {
                i2 += 256;
                if (!tb.Post(k))
                {
                    tb.SendAsync(k).Wait();
                }

                if (i2 > 500 * 100)
                {
                    break;
                }
            }

            tb.Complete();
            chunkCreator.Completion.Wait();

            Console.WriteLine($"Reading key {i}");
            Console.WriteLine(_time.Elapsed);
        }

        private static void TestSmallFlow2(TestOptions opts)
        {
            var world = new World();
            try
            {
                Console.WriteLine("Testing SmallFlow2. Opening world...");
                world.Open(opts.MinecraftWorld);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open world at '{opts.MinecraftWorld}'!. Did you specify the .../db folder?");
                Console.WriteLine("The reason was:");
                Console.WriteLine(ex.Message);
                {
                    return;
                }
            }

            int i = 0;
            int nextout = 2000;
            var keys = new HashSet<ulong>();
            foreach (var x in world.OverworldKeys)
            {
                var key = new LevelDbWorldKey2(x);
                if (!keys.Contains(key.XZ))
                    keys.Add(key.XZ);
            }
            Console.WriteLine(keys.Count());

            _time = Stopwatch.StartNew();
            var chunkdatalist = new List<ChunkData>();


            var tb = new TransformBlock<IEnumerable<ulong>, IReadOnlyCollection<ChunkData>>(key2 =>
            {
                var ret = new List<ChunkData>();
                foreach (var u in key2)
                {
                    var X = (int)((ulong)u >> 32);
                    var Z = (int)((ulong)u & 0xffffffff);
                    var cd = world.GetChunkData(X, Z);
                    ret.Add(cd);
                }

                return ret;
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, BoundedCapacity = 16 });

            var justStore = new ActionBlock<IReadOnlyCollection<ChunkData>>(datas => chunkdatalist.AddRange(datas), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });


            var chunkCreator = new ActionBlock<IReadOnlyCollection<ChunkData>>(data =>
            {
                var sp = Stopwatch.StartNew();
                Chunk ck = null;
                foreach (var d in data)
                {
                    ck = world.GetChunk(d.X, d.Z, d);
                }

                Interlocked.Add(ref i, data.Count);
                if (i > nextout)
                {
                    Interlocked.Add(ref nextout, 2000);
                    Console.WriteLine($"Reading key {i} {_time.Elapsed} {i / (_time.ElapsedMilliseconds / 1000.0)}");
                    if (ck != null)
                    {
                        Console.WriteLine(ck.Blocks.Count());
                    }
                }

                Console.WriteLine(sp.Elapsed);
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 4, BoundedCapacity = 16 });

            //tb.LinkTo(chunkCreator, new DataflowLinkOptions() { PropagateCompletion = true });
            tb.LinkTo(justStore, new DataflowLinkOptions() { PropagateCompletion = true });

            int i2 = 0;
            foreach (var k in keys.Batch(256))
            {

                i2 += 256;
                if (!tb.Post(k))
                {
                    tb.SendAsync(k).Wait();
                }

                if (i2 > 25 * 1000)
                {
                    //break;
                }
            }

            tb.Complete();
            justStore.Completion.Wait();
            Console.WriteLine(chunkdatalist.Count);


            _time = Stopwatch.StartNew();
            NotParallel.ForEach(chunkdatalist, d =>
            {
                Chunk ck = null;
                ck = world.GetChunk(d.X, d.Z, d);

                Interlocked.Add(ref i, 1);
                if (i > nextout)
                {
                    Interlocked.Add(ref nextout, 2000);
                    Console.WriteLine($"Reading key {i} {_time.Elapsed} {i / (_time.ElapsedMilliseconds / 1000.0)}");
                    if (ck != null)
                    {
                        Console.WriteLine(ck.Blocks.Count());
                    }
                }
            });



            Console.WriteLine($"Reading key {i}");
            Console.WriteLine(_time.Elapsed);
        }
    }
}

