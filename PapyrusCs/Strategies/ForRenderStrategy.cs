using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Maploader.Core;
using Maploader.Renderer;
using Maploader.Renderer.Texture;
using Maploader.World;

namespace PapyrusCs.Strategies
{
    public abstract class ForRenderStrategy : IRenderStrategy
    {
        protected abstract Func<IEnumerable<int>, ParallelOptions, Action<int>, ParallelLoopResult> OuterLoopStrategy
        {
            get;
        }

        public int XMin { get; set; }
        public int XMax { get; set; }
        public int ZMin { get; set; }
        public int ZMax { get; set; }
        public string OutputPath { get; set; }
        public Dictionary<string, Texture> TextureDictionary {get;set;}
        public string TexturePath { get; set; } 
        public int ChunkSize { get; set; } = 16;
        public int ChunksPerDimension { get; set; } = 2;
        public int TileSize { get; set; }
        public World World { get; set; }
        public int TotalChunkCount { get; set; }
        public int InitialZoomLevel { get; set; }
        public List<string> MissingTextures { get; } = new List<string>();
        public List<Exception> Exceptions { get; } = new List<Exception>();


        public RenderSettings RenderSettings { get; set; }

        public void RenderInitialLevel()
        {
            OuterLoopStrategy(BetterEnumerable.SteppedRange(XMin, XMax + 1, ChunksPerDimension), 
                new ParallelOptions() { MaxDegreeOfParallelism = RenderSettings.MaxNumberOfThreads},
                x =>
                {

                    try
                    {
                        var finder = new TextureFinder(TextureDictionary, TexturePath);
                        var chunkRenderer = new ChunkRenderer(finder, RenderSettings);
                        //Console.WriteLine($"Processing X-Line: {x}: {chunkNo}/{chunkCount}");
                        var s = Stopwatch.StartNew();

                        int chunksRendered = 0;
                        for (int z = ZMin; z < ZMax + 1; z += ChunksPerDimension)
                        {
                            using (var b = new Bitmap(TileSize, TileSize))
                            using (var g = Graphics.FromImage(b))
                            {
                                bool anydrawn = false;
                                for (int cx = 0; cx < ChunksPerDimension; cx++)
                                for (int cz = 0; cz < ChunksPerDimension; cz++)
                                {
                                    var chunk = World.GetChunk(x + cx, z + cz);
                                    if (chunk == null)
                                        continue;

                                    chunksRendered++;

                                    chunkRenderer.RenderChunk(chunk, g, cx * ChunkSize, cz * ChunkSize);
                                    anydrawn = true;
                                }

                                if (anydrawn)
                                {
                                    var fx = (x - XMin) / ChunksPerDimension;
                                    var fz = (z - ZMin) / ChunksPerDimension;

                                    SaveBitmap(InitialZoomLevel, fx, fz, b);
                                }
                            }
                        }
                        ChunksRendered?.Invoke(this, new ChunksRenderedEventArgs(chunksRendered));

                        foreach (var str in chunkRenderer.MissingTextures)
                        {
                            MissingTextures.Add(str);
                        }
                    }
                    catch (Exception ex)
                    {
                        Exceptions.Add(ex);
                    }
                });

        }

        private void SaveBitmap(int zoom, int x, int z, Bitmap b)
        {
            var path = Path.Combine(OutputPath, "map", $"{zoom}", $"{x}");
            var filepath = Path.Combine(path, $"{z}.png");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            b.Save(filepath);
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

        public void RenderZoomLevels()
        {
            var sourceZoomLevel = this.InitialZoomLevel;
            var sourceDiameter = this.InitialDiameter;

            while (sourceZoomLevel > 0)
            {
                //for (int x = 0; x < 2 * radius; x += 2)
                var destDiameter = sourceDiameter / 2;

                var sourceZoom = sourceZoomLevel;
                var destZoom = sourceZoomLevel-1;

                OuterLoopStrategy(BetterEnumerable.SteppedRange(0,sourceDiameter, 2), new ParallelOptions() { MaxDegreeOfParallelism = RenderSettings.MaxNumberOfThreads }, x =>
                {
                    Console.WriteLine($"Processing Line {x} at {sourceZoom}");

                    for (int z = 0; z < 2 * destDiameter; z += 2)
                    {
                        var b1 = LoadBitmap(sourceZoom, x, z);
                        var b2 = LoadBitmap(sourceZoom, x + 1, z);
                        var b3 = LoadBitmap(sourceZoom, x, z + 1);
                        var b4 = LoadBitmap(sourceZoom, x + 1, z + 1);

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
                                var path = Path.Combine(OutputPath, "map",$"{destZoom}",$"{x/2}");
                                if (!Directory.Exists(path))
                                    Directory.CreateDirectory(path);
                                var filepath = Path.Combine(path, $"{z / 2}.png");
                                bfinal.Save(filepath);
                            }
                        }
                    }
                });

                sourceDiameter = destDiameter;
                sourceZoomLevel = destZoom;
            }

        }

        public event EventHandler<ChunksRenderedEventArgs> ChunksRendered;

        public int InitialDiameter { get; set; }
    }
}