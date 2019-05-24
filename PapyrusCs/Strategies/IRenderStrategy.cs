using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            List<ChunkKeyStack> chunks = new List<ChunkKeyStack>();
            foreach (var chunkGroup in keysByXZ)
            {
                chunks.Add(new ChunkKeyStack(chunkGroup));
            }

            Console.WriteLine($"Found {chunks.Count}...");

            var groupedToTiles = chunks.GroupBy(x => x.Subchunks.First().Value.GetXZGroup(2))
                .ToDictionary(x => x.Key, x => x);


            Console.WriteLine();

        }

        public void RenderZoomLevels()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<ChunksRenderedEventArgs> ChunksRendered;
        public event EventHandler<ZoomRenderedEventArgs> ZoomLevelRenderd;
    }

    public class ChunkKeyStack
    {
        public Dictionary<byte, LevelDbWorldKey2> Subchunks { get; } = new Dictionary<byte, LevelDbWorldKey2>();

        public ChunkKeyStack(IGrouping<ulong, LevelDbWorldKey2> groupedSubChunks)
        {
            if (!groupedSubChunks.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(groupedSubChunks));
            }
            foreach (var sc in groupedSubChunks)
            {
                Subchunks.Add(sc.SubChunkId, sc);
            }
        }

        public override string ToString()
        {
            return $"{Subchunks.First().Value.X}, {Subchunks.First().Value.Z}";
        }
    }
}