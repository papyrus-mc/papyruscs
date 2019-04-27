using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Maploader.Core;
using Maploader.Renderer;
using Maploader.Renderer.Texture;
using Maploader.World;

namespace PapyrusCs
{
    class Program
    {
        public enum Strategy
        {
            SingleFor,
            ParallelFor,
            TplDataFlow
        }

        public class Options
        {

            [Option('w', "world", Required = true, HelpText = "Sets the path the Minecraft Bedrock Edition Map")]
            public string MinecraftWorld { get; set; }

            [Option('o', "outputpath", Required = false, HelpText = "Sets the output path for the generated map tiles", Default = ".")]
            public string OutputPath { get; set; }

            [Option('s', "strategy", Required = false, HelpText = "Sets the strategy")]
            public Strategy Strategy { get; set; }

            public bool Loaded { get; set; }
        }

        static int Main(string[] args)
        {
            var options = new Options();

            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                options = o;
                options.Loaded = true;
            });

            if (!options.Loaded)
            {
                return -1;
            }


            var world = new World();
            try
            {
                world.Open(options.MinecraftWorld);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open world at '{options.MinecraftWorld}'!. Did you specify the .../db folder?");
                Console.WriteLine("The reason was:");
                Console.WriteLine(ex.Message);
                return -1;
            }

            Console.WriteLine("Generating a list of all chunk keys in the database. This could take a few minutes");
            var keys = world.ChunkKeys.ToList();

            int chunkCount = keys.Count;
            Console.WriteLine($"Total Chunk count {keys.Count}");
            Console.WriteLine();
            _totalChunk = chunkCount;

            var xmin = keys.Min(x => x.X);
            var xmax = keys.Max(x => x.X);
            var zmin = keys.Min(x => x.Y);
            var zmax = keys.Max(x => x.Y);


            Console.WriteLine($"The total dimensions of the map are");
            Console.WriteLine($"  X: {xmin} to {xmax}");
            Console.WriteLine($"  Z: {zmin} to {zmax}");
            Console.WriteLine();


            Console.WriteLine("Reading terrain_texture.json...");
            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Textures\terrain_texture.json"));
            var ts = new TerrainTextureJsonParser(json, "");
            var textures = ts.Textures;
            Console.WriteLine();

            const int chunkSize = 256;
            int chunksPerDimension = 2;
            int tileSize = chunkSize * chunksPerDimension;

            var maxDiameter = Math.Max(Math.Abs(xmax - xmin + 1), Math.Abs(zmax - zmin + 1));
            Console.WriteLine($"The maximum diameter of the map is {maxDiameter}");

            maxDiameter = (maxDiameter+(chunksPerDimension-1)) / chunksPerDimension;
            Console.WriteLine($"For {chunksPerDimension} chunks per tile, new max diameter is {maxDiameter}");

            var zoom = Math.Ceiling(Math.Log(maxDiameter) / Math.Log(2));
            int extendedDia = (int) Math.Pow(2, zoom);

            Console.WriteLine($"To generate the zoom levels, we expand the diameter to {extendedDia}");
            Console.WriteLine($"This results in {zoom+1} zoom levels");
            List<Exception> exes = new List<Exception>();

            var missingTextures = new ConcurrentBag<string>();

            _time = Stopwatch.StartNew();

            var strat = new ParallelForStragety()
            {
                XMax = xmax,
                XMin = xmin,
                ZMax = zmax,
                ZMin = zmin,
                ChunkSize = chunkSize,
                ChunksPerDimension = chunksPerDimension,
                TileSize = tileSize,
                OutputPath = options.OutputPath,
                TextureDictionary = textures,
                TexturePath = Path.Combine(Environment.CurrentDirectory, "Textures"),
                TotalChunkCount = chunkCount,
                World = world,
                InitialZoomLevel = (int)zoom,
                InitialDiameter = extendedDia,
                
            };
            strat.ChunksRendered += RenderDisplay; 
            strat.RenderInitialLevel();

            File.WriteAllLines("missingtextures.txt", missingTextures.Distinct());

            strat.RenderZoomLevels();
          

            world.Close();

            Console.WriteLine("Total Time {0}", _time.Elapsed);

            return 0;
        }

        private static int _totalChunksRendered = 0;
        private static int _totalChunk = 0;
        private static Stopwatch _time;
        private static void RenderDisplay(object sender, ChunksRenderedEventArgs e)
        {
            Interlocked.Add(ref _totalChunksRendered, e.RenderedChunks);
            Console.Write($"\r{_totalChunksRendered} of {_totalChunk} @ {(_totalChunksRendered)/_time.Elapsed.TotalSeconds:0.0}");
        }

      
    }

    public class ParallelForStragety
    {
        public int XMin { get; set; }
        public int XMax { get; set; }
        public int ZMin { get; set; }
        public int ZMax { get; set; }

        public string OutputPath { get; set; }

        public Dictionary<string, Texture> TextureDictionary {get;set;}

        public string TexturePath { get; set; } //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures")


        public int ChunkSize { get; set; } = 16;
        public int ChunksPerDimension { get; set; } = 2;
        public int TileSize { get; set; }

        public World World { get; set; }

        public int TotalChunkCount { get; set; }

        public int InitialZoomLevel { get; set; }

        public List<string> MissingTextures { get; } = new List<string>();
        public List<Exception> Exceptions { get; } = new List<Exception>();

        public EventHandler<ChunksRenderedEventArgs> ChunksRendered;

        public void RenderInitialLevel()
        {
            int chunkNo = 0;
            int chunkCount = TotalChunkCount;
            //Initial Renderung
            Parallel.ForEach(BetterEnumerable.SteppedRange(XMin, XMax + 1, ChunksPerDimension), new ParallelOptions() { MaxDegreeOfParallelism = 8 }, x =>
            {

                try
                {
                    var finder = new TextureFinder(TextureDictionary, TexturePath);
                    var chunkRenderer = new ChunkRenderer(finder);
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

                                var path = Path.Combine(OutputPath, $"map\\{InitialZoomLevel}\\{fx}");
                                var filepath = Path.Combine(path, $"{fz}.png");

                                if (!Directory.Exists(path))
                                    Directory.CreateDirectory(path);
                                b.Save(filepath);
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

        private Bitmap LoadBitmap(double zoom, int x, int z)
        {
            var path = Path.Combine(OutputPath, $"map\\{zoom}\\{x}");
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

                Parallel.ForEach(BetterEnumerable.SteppedRange(0,sourceDiameter, 2), x =>
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
                                var path = Path.Combine(OutputPath, $"map\\{destZoom}\\{x / 2}");
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

        public int InitialDiameter { get; set; }
    }

    public class ChunksRenderedEventArgs : EventArgs
    {
        public ChunksRenderedEventArgs(int renderedChunks)
        {
            RenderedChunks = renderedChunks;
        }

        public int RenderedChunks { get; }
    }
}

