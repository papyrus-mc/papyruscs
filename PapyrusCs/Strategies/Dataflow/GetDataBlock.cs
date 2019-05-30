using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Maploader.World;
using PapyrusCs.Database;

namespace PapyrusCs.Strategies.Dataflow
{
    public class GetDataBlock : ITplBlock
    {
        private readonly PapyrusContext db;
        public TransformBlock<IEnumerable<GroupedChunkSubKeys>, IEnumerable<ChunkData>> Block { get; }

        public GetDataBlock(World world, PapyrusContext db, ExecutionDataflowBlockOptions options)
        {
            this.db = db;
            Block = new TransformBlock<IEnumerable<GroupedChunkSubKeys>, IEnumerable<ChunkData>>(
                groupedChunkSubKeys =>
                {
                    var ret = new List<ChunkData>();
                    foreach (var chunkSubKeys in groupedChunkSubKeys)
                    {
                        var data = world.GetChunkData(chunkSubKeys);
                        ret.Add(data);
                    }

                    ProcessedCount++;
                    return ret;
                }, options);
        }

        public int InputCount => Block.InputCount;
        public int OutputCount => Block.OutputCount;
        public int ProcessedCount { get; set; }
    }
}