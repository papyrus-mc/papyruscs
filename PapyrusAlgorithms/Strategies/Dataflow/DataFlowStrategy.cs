using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Maploader.Core;
using Maploader.Extensions;
using Maploader.Renderer;
using Maploader.Renderer.Imaging;
using Maploader.Renderer.Texture;
using Maploader.World;
using Microsoft.EntityFrameworkCore;
using PapyrusAlgorithms.Data;
using PapyrusAlgorithms.Database;
using PapyrusCs.Database;

namespace PapyrusAlgorithms.Strategies.Dataflow
{
    public class DataFlowStrategy<TImage> : IRenderStrategy where TImage : class
    {
        private readonly IGraphicsApi<TImage> graphics;
        private PapyrusContext db;
        private ImmutableDictionary<LevelDbWorldKey2, KeyAndCrc> renderedSubchunks;
        private bool isUpdate;

        public DataFlowStrategy(IGraphicsApi<TImage> graphics)
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
        public HashSet<LevelDbWorldKey2> AllWorldKeys { get; set; }
        public string FileFormat { get; set; }
        public int FileQuality { get; set; }
        public int Dimension { get; set; }
        public string Profile { get; set; }

        public bool IsUpdate => isUpdate;
        public bool DeleteExistingUpdateFolder { get; set; }

        public int NewInitialZoomLevel { get; set; }
        public int NewLastZoomLevel { get; set; }
        private string pathToDb;
        private string pathToDbUpdate;
        private string pathToDbBackup;
        private string pathToMapUpdate;
        private string pathToMap;

        public void RenderInitialLevel()
        {
            World.ChunkPool = new ChunkPool();
            graphics.DefaultQuality = FileQuality;
          
            Console.Write("Grouping subchunks... ");

            var keysByXZ = AllWorldKeys.Where(c => c.X <= XMax && c.X >= XMin && c.Z <= ZMax && c.Z >= ZMin)
                .GroupBy(x => x.XZ);

            List<GroupedChunkSubKeys> chunkKeys = new List<GroupedChunkSubKeys>();
            foreach (var chunkGroup in keysByXZ)
            {
                chunkKeys.Add(new GroupedChunkSubKeys(chunkGroup));
            }

            Console.WriteLine(chunkKeys.Count);

            AllWorldKeys.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();


            var t = Math.Max(1, this.RenderSettings.MaxNumberOfThreads);

            var tsave = FileFormat == "webp" ? t : 2;

            var getOptions = new ExecutionDataflowBlockOptions()
                {BoundedCapacity = Math.Min(2*t, RenderSettings.MaxNumberOfQueueEntries), EnsureOrdered = false, MaxDegreeOfParallelism = 1};
            var bitmapOptions = new ExecutionDataflowBlockOptions()
                {BoundedCapacity = Math.Min(2 * t, RenderSettings.MaxNumberOfQueueEntries), EnsureOrdered = false, MaxDegreeOfParallelism = t };
            var saveOptions = new ExecutionDataflowBlockOptions()
                {BoundedCapacity = Math.Min(2 * t, RenderSettings.MaxNumberOfQueueEntries), EnsureOrdered = false, MaxDegreeOfParallelism = tsave};
            var dbOptions = new ExecutionDataflowBlockOptions()
                {BoundedCapacity = Math.Min(2 * t, RenderSettings.MaxNumberOfQueueEntries), EnsureOrdered = false, MaxDegreeOfParallelism = 1};
            
            
            var groupedToTiles = chunkKeys.GroupBy(x => x.Subchunks.First().Value.GetXZGroup(ChunksPerDimension))
                .ToList();

            Console.WriteLine($"Grouped by {ChunksPerDimension} to {groupedToTiles.Count} tiles");
            var average = groupedToTiles.Average(x => x.Count());
            Console.WriteLine($"Average of {average:0.0} chunks per tile");

            var getDataBlock = new GetDataBlock(World, renderedSubchunks, getOptions, ForceOverwrite);

            var createAndRender = new CreateChunkAndRenderBlock<TImage>(World, TextureDictionary, TexturePath, RenderSettings, graphics, ChunkSize, ChunksPerDimension, bitmapOptions);
            
            var saveBitmapBlock = new SaveBitmapBlock<TImage>(isUpdate ? pathToMapUpdate : pathToMap, NewInitialZoomLevel, FileFormat, saveOptions, graphics);
            
            var batchBlock = new BatchBlock<IEnumerable<SubChunkData>>(128, new GroupingDataflowBlockOptions() {BoundedCapacity = 128*8, EnsureOrdered = false});

            // Todo, put in own class
            var inserts = 0;
            var updates = 0;
            var r = new Random();
            var dbBLock = new ActionBlock<IEnumerable<IEnumerable<SubChunkData>>>(data =>
            {
                if (data == null)
                    return;

                var datas = data.Where(x => x != null).SelectMany(x => x).ToList();
                try
                {
                    /*
                    if (r.Next(100) == 0)
                    {
                        throw new ArgumentOutOfRangeException("Test Error in dbBLock");
                    }*/
                    
                    var toInsert = datas.Where(x => x.FoundInDb == false)
                        .Select(x => new Checksum {Crc32 = x.Crc32, LevelDbKey = x.Key, Profile = Profile}).ToList();

                    if (toInsert.Count > 0)
                    {
                        db.BulkInsert(toInsert);
                        inserts += toInsert.Count;
                    }

                    var toUpdate = datas.Where(x => x.FoundInDb).Select(x => new Checksum()
                        {Id = x.ForeignDbId, Crc32 = x.Crc32, LevelDbKey = x.Key, Profile = Profile}).ToList();
                    if (toUpdate.Count > 0)
                    {
                        db.BulkUpdate(toUpdate);
                        updates += toUpdate.Count;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in CreateChunkAndRenderBlock: " + ex.Message);
                }

            }, dbOptions);

            createAndRender.ChunksRendered += (sender, args) => ChunksRendered?.Invoke(sender, args);

            getDataBlock.Block.LinkTo(createAndRender.Block, new DataflowLinkOptions() {PropagateCompletion = true,});
            createAndRender.Block.LinkTo(saveBitmapBlock.Block, new DataflowLinkOptions() {PropagateCompletion = true});
            saveBitmapBlock.Block.LinkTo(batchBlock, new DataflowLinkOptions() {PropagateCompletion = true});
            batchBlock.LinkTo(dbBLock, new DataflowLinkOptions {PropagateCompletion = true});
            //saveBitmapBlock.Block.LinkTo(dbBLock, new DataflowLinkOptions() {PropagateCompletion = true});

            int postCount = 0;
            foreach (var groupedToTile in groupedToTiles)
            {
                if (getDataBlock.Block.Post(groupedToTile))
                {
                    postCount++;
                    continue;
                }

                postCount++;
                getDataBlock.Block.SendAsync(groupedToTile).Wait();
                if (postCount > 1000)
                {
                    postCount = 0;
                    Console.WriteLine($"\nQueue Stat: GetData {getDataBlock.InputCount} Render {createAndRender.InputCount} Save {saveBitmapBlock.InputCount} Db {dbBLock.InputCount}");
                }
            }

            Console.WriteLine("Post complete");

            getDataBlock.Block.Complete();
            while (!dbBLock.Completion.Wait(1000))
            {
                Console.WriteLine($"\nQueue Stat: GetData {getDataBlock.InputCount} Render {createAndRender.InputCount} Save {saveBitmapBlock.InputCount} Db {dbBLock.InputCount}");
            }
            Console.WriteLine("DbUpdate complete");


            Console.WriteLine($"\n{inserts}, {updates}");
            Console.WriteLine($"\n{getDataBlock.ProcessedCount} {createAndRender.ProcessedCount}  {saveBitmapBlock.ProcessedCount}");
        }


        protected Func<IEnumerable<int>, ParallelOptions, Action<int>, ParallelLoopResult> OuterLoopStrategy =>
            Parallel.ForEach;

        public bool ForceOverwrite { get; set; }

        public void RenderZoomLevels()
        {
            var sourceZoomLevel = this.NewInitialZoomLevel;
            var sourceDiameter = this.InitialDiameter;

            var sourceLevelXmin = XMin / ChunksPerDimension + (XMin < 0 ? -1 : 0);
            var sourceLevelXmax = XMax / ChunksPerDimension + (XMax < 0 ? 0 : +1);
            var sourceLevelZmin = ZMin / ChunksPerDimension + (ZMin < 0 ? -1 : 0);
            var sourceLevelZmax = ZMax / ChunksPerDimension + (ZMax < 0 ? 0 : +1);


            while (sourceZoomLevel > NewLastZoomLevel)
            {
                // Force garbage collection (may not be necessary)
                GC.Collect();
                GC.WaitForPendingFinalizers();

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


                Console.WriteLine(
                    $"\nRendering Level {destZoom} with source coordinates X {sourceLevelXmin} to {sourceLevelXmax}, Z {sourceLevelZmin} to {sourceLevelZmax}");

                OuterLoopStrategy(BetterEnumerable.SteppedRange(sourceLevelXmin, sourceLevelXmax, 2),
                    new ParallelOptions() {MaxDegreeOfParallelism = RenderSettings.MaxNumberOfThreads},
                    x =>
                    {
                        for (int z = sourceLevelZmin; z < sourceLevelZmax; z += 2)
                        {
                            var b1 = LoadBitmap(sourceZoom, x, z, isUpdate);
                            var b2 = LoadBitmap(sourceZoom, x + 1, z, isUpdate);
                            var b3 = LoadBitmap(sourceZoom, x, z + 1, isUpdate);
                            var b4 = LoadBitmap(sourceZoom, x + 1, z + 1, isUpdate);

                            if (b1 != null || b2 != null || b3 != null || b4 != null)
                            {
                                var bfinal = graphics.CreateEmptyImage(TileSize, TileSize);
                                {
                                    b1 = b1 ?? LoadBitmap(sourceZoom, x, z, false);
                                    b2 = b2 ?? LoadBitmap(sourceZoom, x + 1, z, false);
                                    b3 = b3 ?? LoadBitmap(sourceZoom, x, z + 1, false);
                                    b4 = b4 ?? LoadBitmap(sourceZoom, x + 1, z + 1, false);

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
                                        graphics.DrawImage(bfinal, b4, halfTileSize, halfTileSize, halfTileSize,
                                            halfTileSize);
                                    }

                                    SaveBitmap(destZoom, x / 2, z / 2, isUpdate, bfinal);
                                }

                                // Dispose of any bitmaps, releasing memory
                                foreach (var bitmap in new[] { b1, b2, b3, b4, bfinal }.OfType<IDisposable>())
                                {
                                    bitmap.Dispose();
                                }
                            }
                        }

                        Interlocked.Add(ref linesRendered, 2);

                        ZoomLevelRenderd?.Invoke(this,
                            new ZoomRenderedEventArgs(linesRendered, sourceDiameter, destZoom));
                    });

                sourceLevelZmin /= 2;
                sourceLevelZmax /= 2;
                sourceLevelXmin /= 2;
                sourceLevelXmax /= 2;

                sourceDiameter = destDiameter;
                sourceZoomLevel = destZoom;
            }

        }

        private TImage LoadBitmap(int zoom, int x, int z, bool isUpdate)
        {
            var mapPath = isUpdate ? pathToMapUpdate : pathToMap;

            var path = Path.Combine(mapPath, $"{zoom}", $"{x}");
            var filepath = Path.Combine(path, $"{z}.{FileFormat}");
            if (File.Exists(filepath))
            {
                try
                {
                    return graphics.LoadImage(filepath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error Loading tile at {filepath}, because {ex}");
                    return null;
                }
            }

            return null;
        }

        private void SaveBitmap(int zoom, int x, int z, bool isUpdate, TImage b)
        {
            var mapPath = isUpdate ? pathToMapUpdate : pathToMap;

            var path = Path.Combine(mapPath, $"{zoom}", $"{x}");
            var filepath = Path.Combine(path, $"{z}.{FileFormat}");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            graphics.SaveImage(b, filepath);
        }


        public event EventHandler<ChunksRenderedEventArgs> ChunksRendered;
        public event EventHandler<ZoomRenderedEventArgs> ZoomLevelRenderd;
       

        public void Init()
        {
            pathToDb = Path.Combine(OutputPath, "chunks.sqlite");
            pathToDbUpdate = Path.Combine(OutputPath, "chunks-update.sqlite");
            pathToDbBackup = Path.Combine(OutputPath, "chunks-backup.sqlite");

            pathToMapUpdate = Path.Combine(OutputPath, "update", "dim" + Dimension + (string.IsNullOrEmpty(Profile) ? "" : $"_{Profile}"));
            pathToMap = Path.Combine(OutputPath, "map", "dim" + Dimension + (string.IsNullOrEmpty(Profile) ? "" : $"_{Profile}"));

            isUpdate = File.Exists(pathToDb);

            NewInitialZoomLevel = 20;
            NewLastZoomLevel = NewInitialZoomLevel - InitialZoomLevel;

            if (isUpdate)
            {
                Console.WriteLine("Found chunks.sqlite, this must be an update of the map");

                if (File.Exists(pathToDbUpdate))
                {
                    Console.WriteLine($"Deleting {pathToDbUpdate} old update database file");
                    File.Delete(pathToDbUpdate);
                    File.Delete(pathToDbUpdate + "-wal");
                    File.Delete(pathToDbUpdate + "-shm");
                }

                File.Copy(pathToDb, pathToDbUpdate);

                if (Directory.Exists(pathToMapUpdate) && DeleteExistingUpdateFolder)
                {
                    Console.WriteLine("Deleting old update in {0}", pathToMapUpdate);
                    DirectoryInfo di = new DirectoryInfo(pathToMapUpdate);
                    var files = di.EnumerateFiles("*.*", SearchOption.AllDirectories);
                    var fileInfos = files.ToList();
                    if (fileInfos.Any(x => x.Extension != "."+FileFormat))
                    {
                        Console.WriteLine("Can not delete the update folder, because there are files in it not generated by PapyrusCs");
                        foreach (var f in fileInfos.Where(x => x.Extension != "." + FileFormat))
                        {
                            Console.WriteLine("Unknown file {0}", f.FullName);
                        }
                        throw new InvalidOperationException("Can not delete the update folder, because there are files in it not generated by PapyrusCs");
                    }

                    foreach (var f in fileInfos)
                    {
                        Console.WriteLine("Deleting update file {0}", f.FullName);
                        f.Delete();
                    }
                }
            }

            var c = new DbCreator();
            db = c.CreateDbContext(pathToDbUpdate, true);
            db.Database.Migrate();

            var settings = db.Settings.FirstOrDefault(x => x.Dimension == Dimension && x.Profile == Profile);
            if (settings != null)
            {
                this.FileFormat = settings.Format;
                this.FileQuality = settings.Quality;
                this.ChunksPerDimension = settings.ChunksPerDimension;
                Console.WriteLine("Overriding settings with: Format {0}, Quality {1} ChunksPerDimension {2}", FileFormat, FileQuality, ChunksPerDimension);

                settings.MaxZoom = NewInitialZoomLevel;
                settings.MinZoom = NewLastZoomLevel;
                Console.WriteLine("Setting Zoom levels to {0} down to {1}", NewInitialZoomLevel, NewLastZoomLevel);
                db.SaveChanges();
            }
            else
            {
                
                settings = new Settings()
                {
                    Dimension = Dimension,
                    Profile = Profile,
                    Quality = FileQuality,
                    Format = FileFormat,
                    MaxZoom = this.NewInitialZoomLevel,
                    MinZoom = this.NewLastZoomLevel,
                    ChunksPerDimension = db.Settings.FirstOrDefault()?.ChunksPerDimension ?? this.ChunksPerDimension
                };
                db.Add(settings);
                db.SaveChanges();
            }

            renderedSubchunks = db.Checksums.Where(x => x.Profile == Profile).ToImmutableDictionary(
                x => new LevelDbWorldKey2(x.LevelDbKey), x => new KeyAndCrc(x.Id, x.Crc32));
            Console.WriteLine($"Found {renderedSubchunks.Count} subchunks which are already rendered");
        }

        public void Finish()
        {
            if (string.IsNullOrWhiteSpace(pathToDbUpdate))
                return;
            if (string.IsNullOrWhiteSpace(pathToDb))
                return;
            if (string.IsNullOrWhiteSpace(pathToDbBackup))
                return;

            db.Database.CloseConnection();
            db.Dispose();

            if (File.Exists(pathToDbUpdate))
            {
                if (File.Exists(pathToDbBackup))
                {
                    Console.WriteLine("Deleting old chunks.sqlite backup...");
                    File.Delete(pathToDbBackup);
                }

                if (File.Exists(pathToDb))
                {
                    Console.WriteLine("Creating new chunks.sqlite backup...");
                    File.Move(pathToDb, pathToDbBackup);
                }

                Console.WriteLine("Updating chunks.sqlite...");
                File.Move(pathToDbUpdate, pathToDb);
            }

            if (Directory.Exists(pathToMapUpdate))
            {
                var filesToCopy = Directory.EnumerateFiles(pathToMapUpdate, "*." + FileFormat, SearchOption.AllDirectories);
                Console.WriteLine($"Copying {filesToCopy.Count()} to {pathToMap}");
                foreach (var f in filesToCopy)
                {
                    var newPath = f.Replace(pathToMapUpdate, pathToMap);
                    FileInfo fi = new FileInfo(newPath);
                    fi.Directory?.Create();
                    File.Copy(f, newPath, true);
                }
            }
        }

        public Settings[] GetSettings()
        {
            return db.Settings.ToArray();
        }
    }
}

