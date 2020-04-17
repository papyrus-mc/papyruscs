using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Maploader.World;

namespace PapyrusAlgorithms.Strategies.Dataflow.obsolete
{
    public class CreateDataBlock : ITplBlock
    {
        public TransformBlock<IEnumerable<ChunkData>, IEnumerable<ChunkAndData>> Block { get; }

        public CreateDataBlock(World world, ExecutionDataflowBlockOptions options)
        {
            Block = new TransformBlock<IEnumerable<ChunkData>, IEnumerable<ChunkAndData>>(chunkDatas =>
            {
                var chunks = new List<ChunkAndData>();
                foreach (var cd in chunkDatas)
                {
                    chunks.Add(new ChunkAndData(world.GetChunk(cd.X, cd.Z, cd), cd));
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