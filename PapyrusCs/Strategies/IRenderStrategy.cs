using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Maploader.Renderer;
using Maploader.Renderer.Texture;
using Maploader.World;
using PapyrusCs.Database;

namespace PapyrusCs.Strategies
{
    public interface IRenderStrategy
    {
        int XMin { get; set; }
        int XMax { get; set; }
        int ZMin { get; set; }
        int ZMax { get; set; }
        string OutputPath { get; set; }
        Dictionary<string, Texture> TextureDictionary { get; set; }
        string TexturePath { get; set; }
        int ChunkSize { get; set; }
        int ChunksPerDimension { get; set; }
        int TileSize { get; set; }
        World World { get; set; }
        int TotalChunkCount { get; set; }
        int InitialZoomLevel { get; set; }
        ConcurrentBag<string> MissingTextures { get; }
        List<Exception> Exceptions { get; }
        RenderSettings RenderSettings { get; set; }
        int InitialDiameter { get; set; }
        Func<PapyrusContext> DatabaseCreator { get; set; }
        HashSet<LevelDbWorldKey2> AllWorldKeys { get; set; }
        void RenderInitialLevel();
        void RenderZoomLevels();

        event EventHandler<ChunksRenderedEventArgs> ChunksRendered;
        event EventHandler<ZoomRenderedEventArgs> ZoomLevelRenderd;
    }

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

            Console.WriteLine("Counting chunks...");
            List<ChunkKeyStack> chunkKeys = new List<ChunkKeyStack>();
            foreach (var chunkGroup in keysByXZ)
            {
                chunkKeys.Add(new ChunkKeyStack(chunkGroup));
            }

           

            var groupedToTiles = chunkKeys.GroupBy(x => x.Subchunks.First().Value.GetXZGroup(ChunksPerDimension)).ToList();

            var gdbCount = 0;
            var ccbCount = 0;
            var outpCount = 0;

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
                    
                }, new ExecutionDataflowBlockOptions() { BoundedCapacity = 512 });

            var createChunkBlock = new TransformBlock<IEnumerable<ChunkData>, IEnumerable<Chunk>>(datas =>
                {
                    var chunks = new List<Chunk>();
                    foreach (var cd in datas)
                    {
                        chunks.Add(World.GetChunk(cd.X, cd.Z, cd));
                    }

                    ccbCount++;
                    return chunks;
                }, new ExecutionDataflowBlockOptions() { BoundedCapacity = 16, MaxDegreeOfParallelism = 16});

            var chunkRenderedCounter = 0;


            ThreadLocal<RendererCombi> RenderCombi = new ThreadLocal<RendererCombi>(() =>
                new RendererCombi(TextureDictionary, TexturePath, RenderSettings));

            var output = new ActionBlock<IEnumerable<Chunk>>(datas =>
            {
                var b = new Bitmap(TileSize, TileSize);
                var g = Graphics.FromImage(b);

                var dataList = datas.ToList();

                var first = dataList.First();
                if (!RenderCombi.IsValueCreated)
                {
                    Console.WriteLine("First time on thread " + Thread.CurrentThread.ManagedThreadId);
                }
                var chunkRenderer = RenderCombi.Value.ChunkRenderer;


                var chunks = dataList.OrderBy(x => x.X).ThenBy(x => x.Z);
                foreach (var chunk in chunks)
                {
                    var x = chunk.X % ChunksPerDimension;
                    var z = chunk.Z % ChunksPerDimension;
                    if (x < 0) x += ChunksPerDimension;
                    if (z < 0) z += ChunksPerDimension;
                    chunkRenderer.RenderChunk(b, chunk, g, x*ChunkSize,z*ChunkSize );
                }

                var dx = first.X - first.X % ChunksPerDimension;
                var dz = first.Z - first.Z % ChunksPerDimension;

                var fx = (dx / ChunksPerDimension);
                var fz = (dz / ChunksPerDimension);

                SaveBitmap(InitialZoomLevel, fx, fz, b);
                outpCount++;

                chunkRenderedCounter+=dataList.Count;
                if (chunkRenderedCounter >= 100)
                {
                    ChunksRendered?.Invoke(this, new ChunksRenderedEventArgs(chunkRenderedCounter));
                    chunkRenderedCounter = 0;
                }

                g.Dispose();
                b.Dispose();

            }, new ExecutionDataflowBlockOptions() { BoundedCapacity = 16, EnsureOrdered = false, MaxDegreeOfParallelism = 16});

            getDataBlock.LinkTo(createChunkBlock, new DataflowLinkOptions() { PropagateCompletion = true, });
            createChunkBlock.LinkTo(output, new DataflowLinkOptions() { PropagateCompletion = true });
            //getDataBlock.

            int sacCount = 0;
            foreach (var groupedToTile in groupedToTiles)
            {
                getDataBlock?.SendAsync(groupedToTile).Wait();
                sacCount++;
                if (sacCount % 100 == 0)
                {
                    Console.WriteLine($"{gdbCount} {ccbCount} {outpCount}");
                }
            }

            Console.WriteLine("Post complete");
            
            getDataBlock.Complete();
            while (!output.Completion.Wait(1000))
            {
                Console.WriteLine($"\n{gdbCount} {ccbCount} {outpCount}\n");
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

        public void RenderZoomLevels()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<ChunksRenderedEventArgs> ChunksRendered;
        public event EventHandler<ZoomRenderedEventArgs> ZoomLevelRenderd;
    }

    class RendererCombi
    {
        public TextureFinder Finder { get; }
        public ChunkRenderer ChunkRenderer { get; }

        public RendererCombi(Dictionary<string, Texture> textureDictionary, string texturePath, RenderSettings renderSettings)
        {
            Finder = new TextureFinder(textureDictionary, texturePath);
            ChunkRenderer = new ChunkRenderer(Finder, renderSettings);
        }
    }
}