using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks.Dataflow;
using Maploader.World;
using PapyrusCs.Database;

namespace PapyrusCs.Strategies.Dataflow
{
    public class GetDataBlock : ITplBlock
    {
        public TransformManyBlock<IEnumerable<GroupedChunkSubKeys>, IEnumerable<ChunkData>> Block { get; }

        public GetDataBlock(World world, ImmutableDictionary<LevelDbWorldKey2, uint> renderedSubChunks, ExecutionDataflowBlockOptions options)
        {
            Block = new TransformManyBlock<IEnumerable<GroupedChunkSubKeys>, IEnumerable<ChunkData>>(
                groupedChunkSubKeys =>
                {
                    var ret2 = new List<List<ChunkData>>();
                    var ret = new List<ChunkData>();
                    foreach (var chunkSubKeys in groupedChunkSubKeys)
                    {
                        var data = world.GetChunkData(chunkSubKeys);

                        bool renderThisChunks = false;
                        foreach (var subKey in data.SubChunks)
                        {
                            if (!renderedSubChunks.TryGetValue(new LevelDbWorldKey2(subKey.Key), out uint crc32))
                            {
                                renderThisChunks = true;
                                break;
                            }
                            if (crc32 != subKey.Crc32)
                            {
                                renderThisChunks = true;
                                break;
                            }
                        }

                        if (renderThisChunks)
                        {
                            ret.Add(data);
                        }
                        else
                        {
                            Console.WriteLine();
                        }
                    }
                    if (ret.Count > 0)
                        ret2.Add(ret);

                    ProcessedCount++;
                    return ret2;
                }, options);
        }

        public int InputCount => Block.InputCount;
        public int OutputCount => Block.OutputCount;
        public int ProcessedCount { get; set; }
    }
}