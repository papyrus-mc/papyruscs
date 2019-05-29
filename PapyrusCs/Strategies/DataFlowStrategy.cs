  using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
  using System.Threading.Tasks;
  using System.Threading.Tasks.Dataflow;
  using Maploader.Core;
  using Maploader.Extensions;
  using Maploader.Renderer;
using Maploader.Renderer.Texture;
using Maploader.World;
using PapyrusCs.Database;

namespace PapyrusCs.Strategies
{
    public class DataFlowStrategy : IRenderStrategy
    {
        public int XMin { get; set; }
        public int XMax { get; set; }
        public int ZMin { get; set; }
        public int ZMax { get; set; }
        public string OutputPath { get; set; }
        public Dictionary<string, Texture> TextureDictionary { get; set; }
        public string TexturePath { get; set; }
        public int ChunkSize { get; set; }
        public int ChunksPerDimension { get; set; }
        public int TileSize { get; set; }
        public World World { get; set; }
        public int TotalChunkCount { get; set; }
        public int InitialZoomLevel { get; set; }
        public ConcurrentBag<string> MissingTextures { get; }
        public List<Exception> Exceptions { get; }
        public RenderSettings RenderSettings { get; set; }
        public int InitialDiameter { get; set; }
        public Func<PapyrusContext> DatabaseCreator { get; set; }
        public HashSet<LevelDbWorldKey2> AllWorldKeys { get; set; }
                              
        public void RenderInitialLevel()
        {
            var keysByXZ = AllWorldKeys.GroupBy(x => x.XZ);

            Console.Write("Grouping subchunks... ");
            List<ChunkKeyStack> chunkKeys = new List<ChunkKeyStack>();
            foreach (var chunkGroup in keysByXZ)
            {
                chunkKeys.Add(new ChunkKeyStack(chunkGroup));
            }

            Console.WriteLine(chunkKeys.Count);

            var getOptions = new ExecutionDataflowBlockOptions()
                {BoundedCapacity = 64, EnsureOrdered = false, MaxDegreeOfParallelism = 1};
            var chunkCreatorOptions = new ExecutionDataflowBlockOptions()
                {BoundedCapacity = 16, EnsureOrdered = false, MaxDegreeOfParallelism = 8 };
            var bitmapOptions = new ExecutionDataflowBlockOptions()
                { BoundedCapacity = 16, EnsureOrdered = false, MaxDegreeOfParallelism = 16 };
            var saveOptions = new ExecutionDataflowBlockOptions()
                {BoundedCapacity = 16, EnsureOrdered = false, MaxDegreeOfParallelism = 1};

            var groupedToTiles = chunkKeys.GroupBy(x => x.Subchunks.First().Value.GetXZGroup(ChunksPerDimension)).ToList();
            Console.WriteLine($"Grouped by {ChunksPerDimension} to {groupedToTiles.Count} tiles");
            var average = groupedToTiles.Average(x => x.Count());
            Console.WriteLine($"Average of {average} chunks per tile");

            var gdbCount = 0;
            var ccbCount = 0;
            var bitmapCount = 0;

            var getDataBlock = new TransformBlock<IEnumerable<ChunkKeyStack>, IEnumerable<ChunkData>>(
                stacks =>
                {
                    var ret = new List<ChunkData>();
                    foreach (var chunkKeyStack in stacks)
                    {
                        ret.Add(World.GetChunkData(chunkKeyStack));
                    }

                    gdbCount++;
                    return ret;
                    
                }, getOptions);

            var createChunkBlock = new TransformBlock<IEnumerable<ChunkData>, IEnumerable<Chunk>>(datas =>
            {
                var chunks = new List<Chunk>();
                foreach (var cd in datas)
                {
                    chunks.Add(World.GetChunk(cd.X, cd.Z, cd));
                }

                ccbCount++;
                return chunks;
            }, chunkCreatorOptions);



            var chunkRenderedCounter = 0;
            ThreadLocal<RendererCombi> RenderCombi = new ThreadLocal<RendererCombi>(() =>
                new RendererCombi(TextureDictionary, TexturePath, RenderSettings));

            var bitmapBlock = new TransformBlock<IEnumerable<Chunk>,BitmapInfo>(chunks =>
            {
                var b = new Bitmap(TileSize, TileSize);
                using (var g = Graphics.FromImage(b))
                {
                    var chunkList = chunks.ToList();
                    var first = chunkList.First();
                    var chunkRenderer = RenderCombi.Value.ChunkRenderer;

                    foreach (var chunk in chunkList)
                    {
                        var x = chunk.X % ChunksPerDimension;
                        var z = chunk.Z % ChunksPerDimension;
                        if (x < 0) x += ChunksPerDimension;
                        if (z < 0) z += ChunksPerDimension;
                        chunkRenderer.RenderChunk(b, chunk, g, x * ChunkSize, z * ChunkSize);
                        chunkRenderedCounter++;
                    }

                    var dx = first.X - first.X % ChunksPerDimension;
                    var dz = first.Z - first.Z % ChunksPerDimension;

                    var fx = (dx / ChunksPerDimension);
                    var fz = (dz / ChunksPerDimension);

                    bitmapCount++;

                    if (chunkRenderedCounter >= 100)
                    {
                        ChunksRendered?.Invoke(this, new ChunksRenderedEventArgs(chunkRenderedCounter));
                        chunkRenderedCounter = 0;
                    }
                    return new BitmapInfo() { B = b, X = fx, Z = fz };
                }
            }, bitmapOptions);

            var outputCount = 0;
            var outputBlock = new ActionBlock<BitmapInfo>(info =>
            {
                SaveBitmap(InitialZoomLevel, info.X, info.Z, info.B);
                outputCount++;
                info.B.Dispose();
            }, saveOptions);

            getDataBlock.LinkTo(createChunkBlock, new DataflowLinkOptions() { PropagateCompletion = true, });
            createChunkBlock.LinkTo(bitmapBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            bitmapBlock.LinkTo(outputBlock, new DataflowLinkOptions() {PropagateCompletion = true});

            int sacCount = 0;
            foreach (var groupedToTile in groupedToTiles)
            {
                getDataBlock?.SendAsync(groupedToTile).Wait();
                sacCount++;
                if (sacCount % 100 == 0)
                {
                    Console.WriteLine($"{getDataBlock.OutputCount} {createChunkBlock.OutputCount} {bitmapBlock.OutputCount} {outputBlock.InputCount}");
                }
            }

            Console.WriteLine("Post complete");
            
            getDataBlock.Complete();
            while (!outputBlock.Completion.Wait(1000))
            {
                Console.WriteLine($"\n{gdbCount} {ccbCount} {bitmapCount} {outputCount} {outputBlock.InputCount}");
            }
        }

        private void SaveBitmap(int zoom, int x, int z, Bitmap b)
        {
            var path = Path.Combine(OutputPath, "map", $"{zoom}", $"{x}");
            var filepath = Path.Combine(path, $"{z}.png");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            b.Save(filepath);
        }

        protected Func<IEnumerable<int>, ParallelOptions, Action<int>, ParallelLoopResult> OuterLoopStrategy => Parallel.ForEach;

        public void RenderZoomLevels()
        {
            var sourceZoomLevel = this.InitialZoomLevel;
            var sourceDiameter = this.InitialDiameter;

            var sourceLevelXmin = XMin / ChunksPerDimension;
            var sourceLevelXmax = XMax / ChunksPerDimension;
            var sourceLevelZmin = ZMin / ChunksPerDimension;
            var sourceLevelZmax = ZMax / ChunksPerDimension;


            while (sourceZoomLevel > 0)
            {
                var destDiameter = sourceDiameter / 2;
                var sourceZoom = sourceZoomLevel;
                var destZoom = sourceZoomLevel - 1;
                var linesRendered = 0;


                if (sourceLevelXmin.IsOdd()) // always start at an even coordinate
                    sourceLevelXmin--;

                if (sourceLevelXmax.IsOdd())
                    sourceLevelXmax++;

                if (sourceLevelZmin.IsOdd()) // always start at an even coordinate
                    sourceLevelZmin--;

                if (sourceLevelZmax.IsOdd())
                    sourceLevelZmax++;


                Console.WriteLine($"\nRendering Level {destZoom} with source coordinates X {sourceLevelXmin} to {sourceLevelXmax}, Z {sourceLevelZmin} to {sourceLevelZmax}");

                OuterLoopStrategy(BetterEnumerable.SteppedRange(sourceLevelXmin, sourceLevelXmax, 2),
                    new ParallelOptions() { MaxDegreeOfParallelism = RenderSettings.MaxNumberOfThreads },
                    x =>
                    {
                        for (int z = sourceLevelZmin; z < sourceLevelZmax; z += 2)
                        {
                            var b1 = LoadBitmap(sourceZoom, x, z);
                            var b2 = LoadBitmap(sourceZoom, x + 1, z);
                            var b3 = LoadBitmap(sourceZoom, x, z + 1);
                            var b4 = LoadBitmap(sourceZoom, x + 1, z + 1);

                            if (b1 != null || b2 != null || b3 != null || b4 != null)
                            {
                                bool didDraw = false;
                                using (var bfinal = new Bitmap(TileSize, TileSize))
                                using (var gfinal = Graphics.FromImage(bfinal))
                                {
                                    var halfTileSize = TileSize / 2;

                                    if (b1 != null)
                                    {
                                        gfinal.DrawImage(b1, 0, 0, halfTileSize, halfTileSize);
                                        didDraw = true;
                                    }

                                    if (b2 != null)
                                    {
                                        gfinal.DrawImage(b2, halfTileSize, 0, halfTileSize, halfTileSize);
                                        didDraw = true;

                                    }

                                    if (b3 != null)
                                    {
                                        gfinal.DrawImage(b3, 0, halfTileSize, halfTileSize, halfTileSize);
                                        didDraw = true;

                                    }

                                    if (b4 != null)
                                    {
                                        gfinal.DrawImage(b4, halfTileSize, halfTileSize, halfTileSize, halfTileSize);
                                        didDraw = true;

                                    }

                                    if (didDraw)
                                    {
                                        SaveBitmap(destZoom, x / 2, z / 2, bfinal);
                                    }
                                }
                            }
                        }
                        Interlocked.Add(ref linesRendered, 2);

                        ZoomLevelRenderd?.Invoke(this, new ZoomRenderedEventArgs(linesRendered, sourceDiameter, destZoom));


                    });

                sourceLevelZmin /= 2;
                sourceLevelZmax /= 2;
                sourceLevelXmin /= 2;
                sourceLevelXmax /= 2;

                sourceDiameter = destDiameter;
                sourceZoomLevel = destZoom;
            }

        }

        private Bitmap LoadBitmap(int zoom, int x, int z)
        {
            var path = Path.Combine(OutputPath, "map", $"{zoom}", $"{x}");
            var filepath = Path.Combine(path, $"{z}.png");
            if (File.Exists(filepath))
            {
                return new Bitmap(filepath);
            }

            return null;
        }

        public event EventHandler<ChunksRenderedEventArgs> ChunksRendered;
        public event EventHandler<ZoomRenderedEventArgs> ZoomLevelRenderd;
    }
}