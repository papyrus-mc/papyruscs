using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Maploader.Core;
using Maploader.Extensions;
using Maploader.Renderer;
using Maploader.Renderer.Imaging;
using Maploader.Renderer.Texture;
using Maploader.World;
using PapyrusCs.Database;

namespace PapyrusCs.Strategies.For
{
    public abstract class ForRenderStrategy<TImage> : IRenderStrategy where TImage : class
    {
        protected abstract Func<IEnumerable<int>, ParallelOptions, Action<int>, ParallelLoopResult> OuterLoopStrategy { get; }

        private readonly IGraphicsApi<TImage> graphics;

        protected ForRenderStrategy(IGraphicsApi<TImage> graphics)
        {
            this.graphics = graphics;
        }

        public int XMin { get; set; }
        public int XMax { get; set; }
        public int ZMin { get; set; }
        public int ZMax { get; set; }
        public string OutputPath { get; set; }
        public Dictionary<string, Texture> TextureDictionary { get; set; }
        public string TexturePath { get; set; }
        public int ChunkSize { get; set; } = 16;
        public int ChunksPerDimension { get; set; } = 2;
        public int TileSize { get; set; }
        public World World { get; set; }
        public int TotalChunkCount { get; set; }
        public int InitialZoomLevel { get; set; }
        public ConcurrentBag<string> MissingTextures { get; } = new ConcurrentBag<string>();
        public List<Exception> Exceptions { get; } = new List<Exception>();
        public event EventHandler<ChunksRenderedEventArgs> ChunksRendered;
        public event EventHandler<ZoomRenderedEventArgs> ZoomLevelRenderd;

        public int InitialDiameter { get; set; }
        public Func<PapyrusContext> DatabaseCreator { get; set; }
        public HashSet<LevelDbWorldKey2> AllWorldKeys { get; set; }
        public ImmutableDictionary<LevelDbWorldKey2, KeyAndCrc> RenderedSubChunks { get; set; }
        public bool IsUpdate { get; set; }
        public string FileFormat { get; set; }
        public int FileQuality { get; set; }

        public RenderSettings RenderSettings { get; set; }

        public void RenderInitialLevel()
        {
            graphics.DefaultQuality = FileQuality;
            if (XMin.IsOdd())
                XMin--;
            if (ZMin.IsOdd())
                ZMin--;

            OuterLoopStrategy(BetterEnumerable.SteppedRange(XMin, XMax + 1, ChunksPerDimension),
                new ParallelOptions() {MaxDegreeOfParallelism = RenderSettings.MaxNumberOfThreads},
                x =>
                {

                    int chunksRendered = 0;

                    try
                    {
                        var finder = new TextureFinder<TImage>(TextureDictionary, TexturePath, graphics);
                        var chunkRenderer = new ChunkRenderer<TImage>(finder, graphics, RenderSettings);

                        for (int z = ZMin; z < ZMax + 1; z += ChunksPerDimension)
                        {

                            TImage b = null;
                            bool anydrawn = false;
                            for (int cx = 0; cx < ChunksPerDimension; cx++)
                            for (int cz = 0; cz < ChunksPerDimension; cz++)
                            {

                                if (AllWorldKeys != null)
                                {
                                    var key = new LevelDbWorldKey2(x + cx, z + cz);
                                    if (!AllWorldKeys.Contains(key))
                                        continue;
                                }

                                var data = World.GetOverworldChunkData(x + cx, z + cz);

                                if (data.Empty)
                                    continue;

                                var chunk = World.GetChunk(x + cx, z + cz, data);

                                if (b == null)
                                {
                                    b = graphics.CreateEmptyImage(TileSize, TileSize);
                                }

                                chunksRendered++;
                                chunkRenderer.RenderChunk(b, chunk, cx * ChunkSize, cz * ChunkSize);
                                anydrawn = true;


                            }

                            if (anydrawn)
                            {
                                var fx = (x) / ChunksPerDimension;
                                var fz = (z) / ChunksPerDimension;

                                SaveBitmap(InitialZoomLevel, fx, fz, b);
                            }


                            if (chunksRendered >= 32)
                            {
                                ChunksRendered?.Invoke(this, new ChunksRenderedEventArgs(chunksRendered));
                                chunksRendered = 0;
                            }
                        }

                        if (chunksRendered > 0)
                        {
                            ChunksRendered?.Invoke(this, new ChunksRenderedEventArgs(chunksRendered));
                        }

                        foreach (var str in chunkRenderer.MissingTextures)
                        {
                            MissingTextures.Add(str);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Exceptions.Add(ex);
                    }

                });
            Console.WriteLine("\nDone rendering initial level\n");
        }



        public void RenderZoomLevels()
        {
            var sourceZoomLevel = this.InitialZoomLevel;
            var sourceDiameter = this.InitialDiameter;

            var sourceLevelXmin = XMin / ChunksPerDimension;
            var sourceLevelXmax = XMax / ChunksPerDimension;
            var sourceLevelZmin = ZMin / ChunksPerDimension;
            var sourceLevelZmax = ZMax / ChunksPerDimension;

            graphics.DefaultQuality = FileQuality;

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

                        if (b1 !=null || b2 != null || b3 != null || b4 != null)
                        {
                            var bfinal = graphics.CreateEmptyImage(TileSize, TileSize);
                            {
                                var halfTileSize = TileSize / 2;

                                if (b1 != null)
                                {
                                    graphics.DrawImage(bfinal, b1, 0, 0, halfTileSize, halfTileSize);
                                }

                                if (b2 != null)
                                {
                                    graphics.DrawImage(bfinal, b2, halfTileSize, 0, halfTileSize, halfTileSize);
                                }

                                if (b3 != null)
                                {
                                    graphics.DrawImage(bfinal, b3, 0, halfTileSize, halfTileSize, halfTileSize);
                                }

                                if (b4 != null)
                                {
                                    graphics.DrawImage(bfinal, b4, halfTileSize, halfTileSize, halfTileSize, halfTileSize);
                                }

                                SaveBitmap(destZoom, x / 2, z / 2, bfinal);
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

        private void SaveBitmap(int zoom, int x, int z, TImage b)
        {
            var path = Path.Combine(OutputPath, "map", $"{zoom}", $"{x}");
            var filepath = Path.Combine(path, $"{z}.{FileFormat}");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            graphics.SaveImage(b, filepath);
        }

        private TImage LoadBitmap(int zoom, int x, int z)
        {
            var path = Path.Combine(OutputPath, "map", $"{zoom}", $"{x}");
            var filepath = Path.Combine(path, $"{z}.{FileFormat}");
            if (File.Exists(filepath))
            {
                return graphics.LoadImage(filepath);
            }

            return null;
        }


    }
}