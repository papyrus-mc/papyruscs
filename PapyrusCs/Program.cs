using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Maploader.Renderer;
using Maploader.Renderer.Texture;
using Maploader.World;

namespace MapCreatorCore
{
    public static class BetterEnumerable
    {
        public static IEnumerable<int> SteppedRange(int fromInclusive, int toExclusive, int step)
        {
            for (var i = fromInclusive; i < toExclusive; i += step)
            {
                yield return i;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            var world = new World();
            //world.Open(@"C:\Users\deepblue1\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\RhIAAFEzQQA=\db");
            world.Open(@"C:\Users\r\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\RhIAAFEzQQA=\db");

            var keys = world.ChunkKeys.ToList();

            int chunkCount = keys.Count;
            Console.WriteLine($"Total Chunk count {keys.Count}");

            var xmin = keys.Min(x => x.X);
            var xmax = keys.Max(x => x.X);
            var zmin = keys.Min(x => x.Y);
            var zmax = keys.Max(x => x.Y);



            //xmin = -2;
            //xmax = 2;
            //zmin = -2;
            //zmax = 2;

            var sw = Stopwatch.StartNew();


            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Textures\terrain_texture.json"));
            var ts = new TerrainTextureJsonParser(json, "");
            var textures = ts.Textures;

            const int chunkSize = 256;
            int chunksPerDimension = 2;
            int tileSize = chunkSize * chunksPerDimension;

            var maxDiameter = Math.Max(Math.Abs(xmax - xmin + 1), Math.Abs(zmax - zmin + 1));
            Console.WriteLine($"Max Diameter is {maxDiameter}");

            maxDiameter = (maxDiameter+(chunksPerDimension-1)) / chunksPerDimension;
            Console.WriteLine($"For {chunksPerDimension} per Tile, new Max Diameter is {maxDiameter}");

            var zoom = Math.Ceiling(Math.Log(maxDiameter) / Math.Log(2));
            int extendedDia = (int) Math.Pow(2, zoom);

            Console.WriteLine($"Calculated {zoom} Zoom Levels");
            Console.WriteLine($"Extended Diameter is {extendedDia}");

            List<Exception> exes = new List<Exception>();

            int chunkNo = 0;
            var missingTextures = new ConcurrentBag<string>();

            if (true)
            {
                //Initial Renderung
                Parallel.ForEach(BetterEnumerable.SteppedRange(xmin, xmax + 1, chunksPerDimension), new ParallelOptions() {MaxDegreeOfParallelism = 8}, x =>
                {
                    try
                    {
                        var finder = new TextureFinder(textures, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures"));
                        var chunkRenderer = new ChunkRenderer(finder);

                        Console.WriteLine($"Processing X-Line: {x}: {chunkNo}/{chunkCount}");
                        var s = Stopwatch.StartNew();
                        int chunkCounter = 0;
                        for (int z = zmin; z < zmax + 1; z += chunksPerDimension)
                        {
                            using (var b = new Bitmap(tileSize, tileSize))
                            using (var g = Graphics.FromImage(b))
                            {
                                bool anydrawn = false;
                                for (int cx = 0; cx < chunksPerDimension; cx++)
                                for (int cz = 0; cz < chunksPerDimension; cz++)
                                {

                                    var chunk = world.GetChunk(x + cx, z + cz);
                                    if (chunk == null)
                                        continue;

                                    Interlocked.Increment(ref chunkNo);

                                    chunkRenderer.RenderChunk(chunk, g, cx * chunkSize, cz * chunkSize);
                                    anydrawn = true;
                                }

                                if (anydrawn)
                                {
                                    var fx = (x - xmin) / chunksPerDimension;
                                    var fz = (z - zmin) / chunksPerDimension;

                                    var path = $"map\\{zoom}\\{fx}";
                                    var filepath = Path.Combine(path, $"{fz}.png");

                                    if (!Directory.Exists(path))
                                        Directory.CreateDirectory(path);
                                    b.Save(filepath);
                                    chunkCounter+=chunksPerDimension*chunksPerDimension;
                                }
                            }
                        }
                        Console.WriteLine($"Processed X-Line: {x}: {chunkNo}/{chunkCount} @ {chunkCounter/(s.ElapsedMilliseconds/1000.0)}");


                        foreach (var str in chunkRenderer.MissingTextures)
                        {
                            missingTextures.Add(str);
                        }
                    }
                    catch (Exception ex)
                    {
                        exes.Add(ex);
                    }
                });
            }
            File.WriteAllLines("missingtextures.txt", missingTextures.Distinct());


            // Creating Zoomlevels
            while (zoom >= 0)
            {
                //for (int x = 0; x < 2 * radius; x += 2)
                var radiusLocal = extendedDia / 2;
                var zoomLocal = zoom;
                Parallel.For(0, extendedDia, hx =>
                {
                    var x = 2 * hx;
                    Console.WriteLine($"Processing {x} at {zoomLocal-1}");
                    for (int z = 0; z < 2 * radiusLocal; z += 2)
                    {

                        var b1 = LoadBitmap(zoomLocal, x, z);
                        var b2 = LoadBitmap(zoomLocal, x + 1, z);
                        var b3 = LoadBitmap(zoomLocal, x, z + 1);
                        var b4 = LoadBitmap(zoomLocal, x + 1, z + 1);

                        bool didDraw = false;
                        using (var bfinal = new Bitmap(tileSize, tileSize))
                        using (var gfinal = Graphics.FromImage(bfinal))
                        {
                            var halfTileSize = tileSize / 2;

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
                                var path = $"map\\{zoomLocal - 1}\\{x / 2}";
                                if (!Directory.Exists(path))
                                    Directory.CreateDirectory(path);
                                var filepath = Path.Combine(path, $"{z / 2}.png");
                                bfinal.Save(filepath);
                            }
                        }
                    }
                });

                extendedDia /= 2;
                zoom--;
            }


            world.Close();

            Console.WriteLine("Time {0}", sw.Elapsed);
        }

        private static Bitmap LoadBitmap(double zoom, int x, int z)
        {
            var path = $"map\\{zoom}\\{x}";
            var filepath = Path.Combine(path, $"{z}.png");
            if (File.Exists(filepath))
            {
                return new Bitmap(filepath);
            }

            return null;
        }
    }
}

