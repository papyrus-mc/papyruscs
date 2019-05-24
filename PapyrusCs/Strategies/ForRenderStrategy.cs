using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Maploader.Core;
using Maploader.Extensions;
using Maploader.Renderer;
using Maploader.Renderer.Texture;
using Maploader.World;
using PapyrusCs.Database;

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

        public RenderSettings RenderSettings { get; set; }

        public void RenderInitialLevel()
        {
            if (XMin.IsOdd())
                XMin--;
            if (ZMin.IsOdd())
                ZMin--;

            OuterLoopStrategy(BetterEnumerable.SteppedRange(XMin, XMax + 1, ChunksPerDimension),
                new ParallelOptions() { MaxDegreeOfParallelism = RenderSettings.MaxNumberOfThreads },
                x =>
                {
                    using (var db = DatabaseCreator())
                    {
                        int chunksRendered = 0;

                        try
                        {
                            var finder = new TextureFinder(TextureDictionary, TexturePath);
                            var chunkRenderer = new ChunkRenderer(finder, RenderSettings);

                            for (int z = ZMin; z < ZMax + 1; z += ChunksPerDimension)
                            {

                                Bitmap b = null;
                                Graphics g = null;
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

                                    if (false)
                                    {
                                        foreach (var s in data.SubChunks)
                                        {
                                            db.Checksums.Add(new Checksum() {LevelDbKey = s.Key, Crc32 = s.Crc32});
                                        }
                                        var savingTask = db.SaveChangesAsync();
                                        var result = savingTask.Result;
                                    }

                                    var chunk = World.GetChunk(x + cx, z + cz, data);

                                    if (b == null)
                                    {
                                        b = new Bitmap(TileSize, TileSize);
                                        g = Graphics.FromImage(b);
                                    }

                                    chunksRendered++;
                                    chunkRenderer.RenderChunk(b, chunk, g, cx * ChunkSize, cz * ChunkSize);
                                    anydrawn = true;

                                   
                                }

                                if (anydrawn)
                                {
                                    var fx = (x) / ChunksPerDimension;
                                    var fz = (z) / ChunksPerDimension;

                                    SaveBitmap(InitialZoomLevel, fx, fz, b);
                                    g.Dispose();
                                    b.Dispose();
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


    }
}