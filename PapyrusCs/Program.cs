using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using Maploader.Renderer;
using Maploader.Renderer.Texture;
using Maploader.World;
using PapyrusCs.Strategies;

namespace PapyrusCs
{
    class Program
    {
        public enum Strategy
        {
            SingleFor,
            ParallelFor,
        }

        public enum RenderMode
        {
            Basic,
            Heightmap,
        }


        public class Options
        {

            [Option('w', "world", Required = true, HelpText = "Sets the path the Minecraft Bedrock Edition Map")]
            public string MinecraftWorld { get; set; }

            [Option('o', "output", Required = false, HelpText = "Sets the output path for the generated map tiles", Default = ".")]
            public string OutputPath { get; set; }

            [Option("htmlfile", Required = false, HelpText = "Sets name of html map file", Default = "map.html")]
            public string MapHtml { get; set; }

            [Option('s', "strategy", Required = false, HelpText = "Sets the render strategy. Valid are SingleFor and ParallelFor (Multithreaded)", Default = Strategy.ParallelFor)]
            public Strategy Strategy { get; set; }

            [Option("coords", Required = false, HelpText = "Render text coordinates in each chunk", Default = true)]
            public bool RenderCoords { get; set; }

            [Option("limitx", Required = false, HelpText = "Limits the chunk rendering in the x dimension (inclusive). Provide two values with comma separated, eg: -10,10")]
            public string LimitX { get; set; }

            [Option("limitz", Required = false, HelpText = "Limits the chunk rendering in the z dimension (inclusive). Provide two values with comma separated, eg: -10,10")]
            public string LimitZ { get; set; }

            [Option('y', "limity", Required = false, HelpText = "Limits the chunk rendering in the y dimension (inclusive). For y provide just one positive value, eg: 10. -1 means: all", Default = -1)]
            public int LimitY { get; set; }

            [Option("threads", Required = false, HelpText = "Set maximum of used threads", Default = 16)]
            public int MaxNumberOfThreads { get; set; }

            [Option('j', "brillouinj", Required = false, HelpText = "Sets factor j for heightmap brightness formular brillouin", Default = 10000f)]
            public float BrillouinJ { get; set; }

            [Option('r', "rendermode", Required = false, HelpText = "RenderMode: Basic - Render without brightness adjustment.\nHeightmap - Render with brightness adjustment based on.height of block", Default = RenderMode.Heightmap)]
            public RenderMode RenderMode { get; set; }

            public bool Loaded { get; set; }
            public int? LimitXLow { get; set; }
            public int? LimitXHigh { get; set; }
            public int? LimitZLow { get; set; }
            public int? LimitZHigh { get; set; }
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

            // Parameter Validation
            try
            {
                if (options.LimitX != null)
                {
                    var splittedLimit = options.LimitX.Split(',').Select(x => Convert.ToInt32(x)).OrderBy(x => x).ToArray();
                    if (splittedLimit.Length != 2)
                        throw new ArgumentOutOfRangeException("LimitX");
                    options.LimitXLow = splittedLimit[0];
                    options.LimitXHigh = splittedLimit[1];
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"The value '{options.LimitX}' for the LimitZ parameter is not valid. Try something like -10,10");
                return -1;
            }

            try
            {
                if (options.LimitZ != null)
                {
                    var splittedLimit = options.LimitZ.Split(',').Select(x => Convert.ToInt32(x)).OrderBy(x => x).ToArray();
                    if (splittedLimit.Length != 2)
                        throw new ArgumentOutOfRangeException("LimitZ");
                    options.LimitZLow = splittedLimit[0];
                    options.LimitZHigh = splittedLimit[1];
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"The value '{options.LimitZ}' for the LimitZ parameter is not valid. Try something like -10,10");
                return -1;
            }

            var world = new World();
            try
            {
                Console.WriteLine("Opening world...");
                world.Open(options.MinecraftWorld);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open world at '{options.MinecraftWorld}'!. Did you specify the .../db folder?");
                Console.WriteLine("The reason was:");
                Console.WriteLine(ex.Message);
                return -1;
            }





            // Start Generation
            Console.WriteLine("Generating a list of all chunk keys in the database. This could take a few minutes");
            var keys = world.ChunkKeys.ToHashSet();
            
            HashSet<UInt64> keys64 = null; 
            unchecked
            {
                keys64 = keys.Select(x => (UInt64)(
                                    ((UInt64)(x.X) << 32) |
                                    ((UInt64)(x.Y) & 0xFFFFFFFF)
                                 )).ToHashSet();
            }
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

            if (options.LimitXLow.HasValue && options.LimitXHigh.HasValue)
            {
                xmin = options.LimitXLow.Value;
                xmax = options.LimitXHigh.Value;
                Console.WriteLine($"Limiting X to {xmin} to {xmax}");
            }
            if (options.LimitZLow.HasValue && options.LimitZHigh.HasValue)
            {
                zmin = options.LimitZLow.Value;
                zmax = options.LimitZHigh.Value;
                Console.WriteLine($"Limiting Z to {zmin} to {zmax}");
            }
            if (options.LimitY > 0)
            {
                Console.WriteLine($"Limiting Y to {options.LimitY}");
            }
  


            Console.WriteLine("Reading terrain_texture.json...");
            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"textures","terrain_texture.json"));
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

            var zoom = (int) (Math.Ceiling(Math.Log(maxDiameter) / Math.Log(2)));
            int extendedDia = (int) Math.Pow(2, zoom);

            Console.WriteLine($"To generate the zoom levels, we expand the diameter to {extendedDia}");
            Console.WriteLine($"This results in {zoom+1} zoom levels");
            List<Exception> exes = new List<Exception>();

            _time = Stopwatch.StartNew();

            IRenderStrategy strat = null;
            switch (options.Strategy)
            {
                case Strategy.ParallelFor:
                    strat = new ParallelForRenderStrategy();
                    break;
                case Strategy.SingleFor:
                    strat = new SingleForRenderStrategy();
                    break;
                default:
                    strat = new SingleForRenderStrategy();
                    break;
            }
            strat.RenderSettings = new RenderSettings() {
                RenderCoords = options.RenderCoords,
                MaxNumberOfThreads = options.MaxNumberOfThreads,
                Keys = keys64,
                YMax = options.LimitY,
                BrillouinJ = options.BrillouinJ
            };
            strat.InitialDiameter = extendedDia;
            strat.InitialZoomLevel = (int)zoom;
            strat.World = world;
            strat.TotalChunkCount = chunkCount;
            strat.TexturePath = Path.Combine(Environment.CurrentDirectory, "textures");
            strat.TextureDictionary = textures;
            strat.OutputPath = options.OutputPath;
            strat.TileSize = tileSize;
            strat.ChunksPerDimension = chunksPerDimension;
            strat.ChunkSize = chunkSize;
            strat.ZMin = zmin;
            strat.ZMax = zmax;
            strat.XMin = xmin;
            strat.XMax = xmax;
            strat.ChunksRendered += RenderDisplay; 
            strat.RenderInitialLevel();
            var missingTextures = strat.MissingTextures;

            File.WriteAllLines("missingtextures.txt", missingTextures.Distinct());

            strat.ZoomLevelRenderd += RenderZoom;
            strat.RenderZoomLevels();


            try
            {
                var mapHtml = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "map.thtml"));
                mapHtml = mapHtml.Replace("%maxnativezoom%", zoom.ToString());
                mapHtml = mapHtml.Replace("%maxzoom%", (zoom + 2).ToString());
                mapHtml = mapHtml.Replace("%tilesize%", (tileSize).ToString());

                File.WriteAllText(Path.Combine(options.OutputPath, options.MapHtml), mapHtml);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not write map.html");
                Console.WriteLine(ex.Message);
            }

            world.Close();

            Console.WriteLine("Total Time {0}", _time.Elapsed);

            return 0;
        }

        private static void RenderZoom(object sender, ZoomRenderedEventArgs e)
        {
            Console.WriteLine($"{e.LinesRendered} of {e.TotalLines} lines render @ zoom level {e.ZoomLevel}      ");
        }

        private static int _totalChunksRendered = 0;
        private static int _totalChunk = 0;
        private static Stopwatch _time;
        private static void RenderDisplay(object sender, ChunksRenderedEventArgs e)
        {
            Interlocked.Add(ref _totalChunksRendered, e.RenderedChunks);
            Console.Write($"\r{_totalChunksRendered} of {_totalChunk} Chunks render @ {(_totalChunksRendered)/_time.Elapsed.TotalSeconds:0.0} c/s");
        }

      
    }
}

