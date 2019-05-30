using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Maploader.World;
using PapyrusCs.Database;

namespace PapyrusCs.Strategies.Dataflow
{
    public class CreateDataBlock : ITplBlock
    {
        public TransformBlock<IEnumerable<ChunkData>, IEnumerable<Chunk>> Block { get; }

        public CreateDataBlock(World world, ExecutionDataflowBlockOptions options)
        {
            this.db = db;
            Block = new TransformBlock<IEnumerable<ChunkData>, IEnumerable<Chunk>>(chunkDatas =>
            {
                var chunks = new List<Chunk>();
                foreach (var cd in chunkDatas)
                {
                    chunks.Add(world.GetChunk(cd.X, cd.Z, cd));
                }

                ProcessedCount++;
                return chunks;
            }, options);
        }

        public int InputCount => Block.InputCount;
        public int OutputCount => Block.OutputCount;
        public int ProcessedCount { get; set; }
    }
}