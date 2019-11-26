using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using fNbt;
using Maploader.Core;
using Maploader.Extensions;
using Maploader.Renderer;
using Maploader.Renderer.Imaging;
using Maploader.Renderer.Texture;
using Maploader.World;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using PapyrusCs.Database;
using PapyrusCs.Strategies;
using PapyrusCs.Strategies.Dataflow;
using SkiaSharp;
using SkiaSharp = Maploader.Renderer.Imaging.SkiaSharp;

namespace PapyrusCs
{
    public partial class Program
    {
        private static int _totalChunksRendered = 0;
        private static int _totalChunk = 0;
        private static Stopwatch _time = new Stopwatch();
        private static Stopwatch _time2 = new Stopwatch();



        static int Main(string[] args)
        {

            var newargs = args;

            if (args.Length == 0 || !(new string[]{"map", "test","find"}.Contains(args[0])))
            {
                newargs = new[] { "map" }.Concat((args)).ToArray();
            }


            return CommandLine.Parser.Default.ParseArguments<Options, TestOptions, FindOptions>(newargs)
                .MapResult(
                    (Options opts) => { opts.Loaded = true;
                        return RunMapCommand(opts);
                    },
                    (TestOptions opts) => RunTestOptions(opts),
                    (FindOptions opts) => RunFindOptions(opts),
                    errs => 1);
        }

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

        private static int RunTestOptions(TestOptions opts)
        {
            if (opts.TestDbRead)
            {
                TestCommands.TestDbRead(opts);
            } else if (opts.Decode)
            {
                TestCommands.TestDecode(opts);
            }
            else if (opts.Smallflow)
            {
                TestCommands.TestSmallFlow(opts);
            }

            return 0;
        }
  
        private static int  RunMapCommand(Options options)
        {
            _time = Stopwatch.StartNew();
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
                Console.WriteLine(
                    $"The value '{options.LimitX}' for the LimitX parameter is not valid. Try something like -10,10");
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
                Console.WriteLine(
                    $"The value '{options.LimitZ}' for the LimitZ parameter is not valid. Try something like -10,10");
                return -1;
            }

            options.FileFormat = options.FileFormat.ToLowerInvariant();
            if (new string[] {"jpg", "png", "webp", "none"}.All(x => x != options.FileFormat))
            {
                Console.WriteLine($"The value {options.FileFormat} is not allowed for option -f");
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
            int xmin = 0;
            int xmax = 0;
            int zmin = 0;
            int zmax = 0;
            HashSet<LevelDbWorldKey2> allSubChunks = null;

            Func<LevelDbWorldKey2, bool> constraintX = b => true;
            Func<LevelDbWorldKey2, bool> constraintZ = b => true;

            if (options.LimitXLow.HasValue && options.LimitXHigh.HasValue)
            {
                xmin = options.LimitXLow.Value;
                xmax = options.LimitXHigh.Value;
                Console.WriteLine($"Limiting X to {xmin} to {xmax}");
                var xmax1 = xmax;
                var xmin1 = xmin;
                constraintX = key => key.X >= xmin1 && key.X <= xmax1;
            }

            if (options.LimitZLow.HasValue && options.LimitZHigh.HasValue)
            {
                zmin = options.LimitZLow.Value;
                zmax = options.LimitZHigh.Value;
                Console.WriteLine($"Limiting Z to {zmin} to {zmax}");
                var zmax1 = zmax;
                var zmin1 = zmin;
                constraintZ = key => key.Z >= zmin1 && key.Z <= zmax1;
            }

            if (options.Dimension == 1 && options.NoAutoTrimCeiling == false)
            {
                // Nether
                options.TrimCeiling = true;
                if (options.LimitY == -1)
                {
                    options.LimitY = 120;
                }
            }


            Console.WriteLine("Generating a list of all chunk keys in the database.\nThis could take a few minutes");
            var keys = world.GetDimension(options.Dimension).ToList();
            allSubChunks = Enumerable.ToHashSet(keys.Select(x => new LevelDbWorldKey2(x))
                    .Where(k => constraintX(k) && constraintZ(k)));

            _totalChunk = allSubChunks.GroupBy(x => x.XZ).Count();
            Console.WriteLine($"Total Chunk count {_totalChunk}");
            Console.WriteLine();

            xmin = allSubChunks.Min(x => x.X);
            xmax = allSubChunks.Max(x => x.X);
            zmin = allSubChunks.Min(x => x.Z);
            zmax = allSubChunks.Max(x => x.Z);

            Console.WriteLine($"The total dimensions of the map are");
            Console.WriteLine($"  X: {xmin} to {xmax}");
            Console.WriteLine($"  Z: {zmin} to {zmax}");
            Console.WriteLine();

            if (options.LimitY > 0)
            {
                Console.WriteLine($"Limiting Y to {options.LimitY}");
            }

            const int chunkSize = 256;
            int chunksPerDimension = options.ChunksPerDimension;
            int tileSize = chunkSize * chunksPerDimension;
            Console.WriteLine($"Tilesize is {tileSize}x{tileSize}");
            Directory.CreateDirectory(options.OutputPath);

            // db stuff
            var textures = ReadTerrainTextureJson();
            var zoom = CalculateZoom(xmax, xmin, zmax, zmin, chunksPerDimension, out var extendedDia);

            var strat = InstanciateStrategy(options);
            ConfigureStrategy(strat, options, allSubChunks, extendedDia, zoom, world, textures, tileSize, chunkSize, zmin, zmax, xmin, xmax);

            strat.Init();

            // other stuff

            strat.RenderInitialLevel();

            var missingTextures = strat.MissingTextures;
            if (missingTextures != null)
            {
                File.WriteAllLines("missingtextures.txt", missingTextures.Distinct());
            }


            Console.WriteLine("Time is {0}", _time.Elapsed);
            strat.RenderZoomLevels();


            WriteMapHtml(tileSize, options.OutputPath, options.MapHtml, strat.GetSettings(), strat.IsUpdate,
                options.UseLeafletLegacy);

            strat.Finish();
            Console.WriteLine("Total Time {0}", _time.Elapsed);
            world.Close();


            Console.WriteLine("Map generation finished!");
            return 0;
        }

        private static IRenderStrategy InstanciateStrategy(Options options)
        {
            IRenderStrategy strat = null;
            switch (options.Strategy)
            {
                case Strategy.Dataflow:
                default:
                    //strat = new DataFlowStrategy<SKBitmap>(new Maploader.Renderer.Imaging.SkiaSharp());
                    strat = new DataFlowStrategy<Bitmap>(new Maploader.Renderer.Imaging.SystemDrawing());
                    break;
               
            }

            return strat;
        }

        private static void ConfigureStrategy(IRenderStrategy strat, Options options,
            HashSet<LevelDbWorldKey2> allSubChunks,
            int extendedDia, int zoom, World world, Dictionary<string, Texture> textures, int tileSize,
            int chunkSize,
            int zmin, int zmax, int xmin, int xmax)
        {
            strat.RenderSettings = new RenderSettings()
            {
                RenderCoordinateStrings = options.RenderCoords,
                RenderMode = options.RenderMode,
                MaxNumberOfThreads = options.MaxNumberOfThreads,
                YMax = options.LimitY,
                BrillouinJ = options.BrillouinJ,
                BrillouinDivider = options.BrillouinDivider,
                BrillouinOffset = options.BrillouinOffset,
                TrimCeiling = options.TrimCeiling,
                Profile = options.Profile,
            };
            strat.ForceOverwrite = options.ForceOverwrite;
            strat.AllWorldKeys = allSubChunks;
            strat.InitialDiameter = extendedDia;
            strat.InitialZoomLevel = (int)zoom;
            strat.World = world;
            strat.TotalChunkCount = _totalChunk;
            strat.TexturePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "textures");
            strat.TextureDictionary = textures;
            strat.OutputPath = options.OutputPath;
            strat.TileSize = tileSize;
            strat.ChunksPerDimension = options.ChunksPerDimension;
            strat.ChunkSize = chunkSize;
            strat.ZMin = zmin;
            strat.ZMax = zmax;
            strat.XMin = xmin;
            strat.XMax = xmax;
            strat.ChunksRendered += RenderDisplay;
            strat.ZoomLevelRenderd += RenderZoom;
            strat.FileFormat = options.FileFormat;
            strat.FileQuality = options.Quality;
            strat.Dimension = options.Dimension;
            strat.Profile = options.Profile;
            strat.DeleteExistingUpdateFolder = options.DeleteExistingUpdateFolder;
        }
      

        private static void WriteMapHtml(int tileSize, string outputPath, string mapHtmlFile, Settings[] settings,
            bool isUpdate, bool useLegacyLeaflet)
        {
            try
            {
                var layernames = new Dictionary<string, string>
                {
                    { "dim0", "Overworld" },
                    { "dim0_underground", "Underground" },
                    { "dim0_aquatic", "Aquatic" },
                    { "dim0_ore", "Ores" },
                    { "dim0_stronghold", "Strongholds" },
                    { "dim1", "Nether" },
                    { "dim2", "The End" },
                };

                var mapHtmlContext = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, useLegacyLeaflet ? "map.leaflet.thtml" : "map.thtml"));

                Func<Settings, string> getDimWithProfile = (Settings setting) => "dim" + setting.Dimension + (string.IsNullOrEmpty(setting.Profile) ? "" : $"_{setting.Profile}");

                var layersdef = settings.ToDictionary(
                    getDimWithProfile, 
                    setting => new LayerDef
                    {
                        name = layernames.ContainsKey(getDimWithProfile(setting)) ? layernames[getDimWithProfile(setting)] : $"Dimension{setting.Dimension}_{setting.Profile}",
                        attribution = "Generated by <a href=\"https://github.com/mjungnickel18/papyruscs\">PapyrusCS</a>",
                        minNativeZoom = setting.MinZoom,
                        maxNativeZoom = setting.MaxZoom,
                        noWrap = true,
                        tileSize = tileSize,
                        folder = "dim" + setting.Dimension + (string.IsNullOrEmpty(setting.Profile) ? "" : $"_{setting.Profile}"),
                        fileExtension = setting.Format,
                    }
                );

                var globalconfig = new GlobalConfig
                {
                    factor = (Math.Pow(2, settings.First().MaxZoom - 4)),
                    globalMaxZoom = settings.First(x => x.Dimension == settings.Min(y => y.Dimension)).MaxZoom,
                    globalMinZoom = settings.First(x => x.Dimension == settings.Min(y => y.Dimension)).MinZoom,
                    tileSize = tileSize,
                    blocksPerTile = tileSize/16
                };

                mapHtmlContext = mapHtmlContext.Replace(
                    "// # INJECT DATA HERE", 
                    "layers = " + JsonConvert.SerializeObject(layersdef) + "; \r\n"+
                    "config = " + JsonConvert.SerializeObject(globalconfig) + ";");

                Directory.CreateDirectory(Path.Combine(outputPath, "map"));
                File.WriteAllText(Path.Combine(outputPath, "map", mapHtmlFile), mapHtmlContext);
                if (isUpdate)
                {
                    Directory.CreateDirectory(Path.Combine(outputPath, "update"));
                    File.WriteAllText(Path.Combine(outputPath, "update", mapHtmlFile), mapHtmlContext);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not write map.html");
                Console.WriteLine(ex.Message);
            }
        }

        private static int CalculateZoom(int xmax, int xmin, int zmax, int zmin, int chunksPerDimension, out int extendedDia)
        {
            if (xmin > 0 && xmax > 0)
                xmin = 0;

            if (zmin > 0 && zmax > 0)
                zmin = 0;

            if (xmin < 0 && xmax < 0)
                xmax = 0;

            if (zmin < 0 && zmax < 0)
                zmax = 0;

            var maxDiameter = Math.Max(Math.Abs(xmax - xmin + 1), Math.Abs(zmax - zmin + 1));
            Console.WriteLine($"The maximum diameter of the map is {maxDiameter}");

            maxDiameter = (maxDiameter + (chunksPerDimension - 1)) / chunksPerDimension;
            Console.WriteLine($"For {chunksPerDimension} chunks per tile, new max diameter is {maxDiameter}");

            var zoom = (int)(Math.Ceiling(Math.Log(maxDiameter) / Math.Log(2)));
            extendedDia = (int)Math.Pow(2, zoom);

            Console.WriteLine($"To generate the zoom levels, we expand the diameter to {extendedDia}");
            Console.WriteLine($"This results in {zoom + 1} zoom levels");
            return zoom;
        }

        private static Dictionary<string, Texture> ReadTerrainTextureJson()
        {
            Console.WriteLine("Reading terrain_texture.json...");
            var json = File.ReadAllText(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"textures", "terrain_texture.json"));
            var ts = new TerrainTextureJsonParser(json, "");
            var textures = ts.Textures;
            Console.WriteLine();
            return textures;
        }

        private static void RenderZoom(object sender, ZoomRenderedEventArgs e)
        {
            Console.Write($"\r{e.LinesRendered} of {e.TotalLines} lines render @ zoom level {e.ZoomLevel}      ");
        }


        private static void RenderDisplay(object sender, ChunksRenderedEventArgs e)
        {
            if (!_time2.IsRunning)
                _time2.Start();
            Interlocked.Add(ref _totalChunksRendered, e.RenderedChunks);
            Console.Write($"\r{_totalChunksRendered} of {_totalChunk} Chunks render @ {(_totalChunksRendered) / _time2.Elapsed.TotalSeconds:0.0} cps     ");
        }


    }

}

