using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
using PapyrusCs.Database;

namespace PapyrusCs.Strategies.Dataflow
  {
      public class DataFlowStrategy<TImage> : IRenderStrategy where TImage : class
      {
          private readonly IGraphicsApi<TImage> graphics;

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
          public Func<PapyrusContext> DatabaseCreator { get; set; }
          public HashSet<LevelDbWorldKey2> AllWorldKeys { get; set; }
          public ImmutableDictionary<LevelDbWorldKey2, KeyAndCrc> RenderedSubChunks { get; set; }
          public bool IsUpdate { get; set; }
          public string FileFormat { get; set; }

          public void RenderInitialLevel()
          {
              var keysByXZ = AllWorldKeys.Where(c => c.X <= XMax && c.X >= XMin && c.Z <= ZMax && c.Z >= ZMin).GroupBy(x => x.XZ);

              Console.Write("Grouping subchunks... ");
              List<GroupedChunkSubKeys> chunkKeys = new List<GroupedChunkSubKeys>();
              foreach (var chunkGroup in keysByXZ)
              {
                  chunkKeys.Add(new GroupedChunkSubKeys(chunkGroup));
              }

              Console.WriteLine(chunkKeys.Count);

              var getOptions = new ExecutionDataflowBlockOptions()
                  {BoundedCapacity = 64, EnsureOrdered = false, MaxDegreeOfParallelism = 1};
              var chunkCreatorOptions = new ExecutionDataflowBlockOptions()
                  {BoundedCapacity = 16, EnsureOrdered = false, MaxDegreeOfParallelism = 8};
              var bitmapOptions = new ExecutionDataflowBlockOptions()
                  {BoundedCapacity = 32, EnsureOrdered = false, MaxDegreeOfParallelism = 16};
              var saveOptions = new ExecutionDataflowBlockOptions()
                  {BoundedCapacity = 16, EnsureOrdered = false, MaxDegreeOfParallelism = 1};

              var groupedToTiles = chunkKeys.GroupBy(x => x.Subchunks.First().Value.GetXZGroup(ChunksPerDimension)).ToList();
              Console.WriteLine($"Grouped by {ChunksPerDimension} to {groupedToTiles.Count} tiles");
              var average = groupedToTiles.Average(x => x.Count());
              Console.WriteLine($"Average of {average} chunks per tile");

              var db = DatabaseCreator();

              var getDataBlock = new GetDataBlock(World, RenderedSubChunks, getOptions);
              var createChunkBlock = new CreateDataBlock(World, chunkCreatorOptions);
              var bitmapBlock = new BitmapRenderBlock<TImage>(TextureDictionary, TexturePath, RenderSettings, graphics, ChunkSize, ChunksPerDimension, bitmapOptions);
              var saveBitmapBlock = new SaveBitmapBlock<TImage>(OutputPath, InitialZoomLevel, IsUpdate, FileFormat, saveOptions, graphics);

              var inserts = 0;
              var updates = 0;
              var dbBLock = new ActionBlock<IEnumerable<SubChunkData>>(datas =>
              {
                  var toInsert = datas.Where(x => x.FoundInDb == false).Select(x => new Checksum {Crc32 = x.Crc32, LevelDbKey = x.Key}).ToList();

                  if (toInsert.Count > 0)
                  {
                      db.BulkInsert(toInsert);
                      inserts += toInsert.Count;
                  }

                  var toUpdate = datas.Where(x => x.FoundInDb).Select(x => new Checksum() {Id = x.ForeignDbId, Crc32 = x.Crc32, LevelDbKey = x.Key}).ToList();
                  if (toUpdate.Count > 0)
                  {
                      db.BulkUpdate(toUpdate);
                      updates += toUpdate.Count;
                  }

              }, saveOptions);

              bitmapBlock.ChunksRendered += (sender, args) => ChunksRendered?.Invoke(sender, args);

              getDataBlock.Block.LinkTo(createChunkBlock.Block, new DataflowLinkOptions() {PropagateCompletion = true,});
              createChunkBlock.Block.LinkTo(bitmapBlock.Block, new DataflowLinkOptions() {PropagateCompletion = true});
              bitmapBlock.Block.LinkTo(saveBitmapBlock.Block, new DataflowLinkOptions() {PropagateCompletion = true});
              saveBitmapBlock.Block.LinkTo(dbBLock, new DataflowLinkOptions() {PropagateCompletion = true});

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
                      Console.WriteLine($"\n{inserts}, {updates}");
                  }
              }

              Console.WriteLine("Post complete");

              getDataBlock.Block.Complete();
              while (!saveBitmapBlock.Block.Completion.Wait(1000))
              {
                  Console.WriteLine($"\n{getDataBlock.ProcessedCount} {createChunkBlock.ProcessedCount} {bitmapBlock.ProcessedCount} {saveBitmapBlock.ProcessedCount}");
              }

              Console.WriteLine($"\n{inserts}, {updates}");
              Console.WriteLine($"\n{getDataBlock.ProcessedCount} {createChunkBlock.ProcessedCount} {bitmapBlock.ProcessedCount} {saveBitmapBlock.ProcessedCount}");
          }


          protected Func<IEnumerable<int>, ParallelOptions, Action<int>, ParallelLoopResult> OuterLoopStrategy =>
              Parallel.ForEach;

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


                  Console.WriteLine(
                      $"\nRendering Level {destZoom} with source coordinates X {sourceLevelXmin} to {sourceLevelXmax}, Z {sourceLevelZmin} to {sourceLevelZmax}");

                  OuterLoopStrategy(BetterEnumerable.SteppedRange(sourceLevelXmin, sourceLevelXmax, 2),
                      new ParallelOptions() {MaxDegreeOfParallelism = RenderSettings.MaxNumberOfThreads},
                      x =>
                      {
                          for (int z = sourceLevelZmin; z < sourceLevelZmax; z += 2)
                          {
                              var b1 = LoadBitmap(sourceZoom, x, z, IsUpdate);
                              var b2 = LoadBitmap(sourceZoom, x + 1, z, IsUpdate);
                              var b3 = LoadBitmap(sourceZoom, x, z + 1, IsUpdate);
                              var b4 = LoadBitmap(sourceZoom, x + 1, z + 1, IsUpdate);

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

                                      SaveBitmap(destZoom, x / 2, z / 2, IsUpdate, bfinal);
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
              var mapPath = isUpdate ? "mapupdate" : "map";

              var path = Path.Combine(OutputPath, mapPath, $"{zoom}", $"{x}");
              var filepath = Path.Combine(path, $"{z}.{FileFormat}");
              if (File.Exists(filepath))
              {
                  return graphics.LoadImage(filepath);
              }

              return null;
          }

          private void SaveBitmap(int zoom, int x, int z, bool isUpdate, TImage b)
          {
              var mapPath = isUpdate ? "mapupdate" : "map";

              var path = Path.Combine(OutputPath, mapPath, $"{zoom}", $"{x}");
              var filepath = Path.Combine(path, $"{z}.{FileFormat}");

              if (!Directory.Exists(path))
                  Directory.CreateDirectory(path);
              graphics.SaveImage(b, filepath);
          }


          public event EventHandler<ChunksRenderedEventArgs> ChunksRendered;
          public event EventHandler<ZoomRenderedEventArgs> ZoomLevelRenderd;
      }
  }