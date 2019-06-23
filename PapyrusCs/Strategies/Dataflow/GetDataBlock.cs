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
        private bool forceOverwrite;
        public TransformManyBlock<IEnumerable<GroupedChunkSubKeys>, IEnumerable<ChunkData>> Block { get; }

        public GetDataBlock(World world, ImmutableDictionary<LevelDbWorldKey2, KeyAndCrc> renderedSubChunks, ExecutionDataflowBlockOptions options, bool forceOverwrite)
        {
            this.forceOverwrite = forceOverwrite;
            Block = new TransformManyBlock<IEnumerable<GroupedChunkSubKeys>, IEnumerable<ChunkData>>(
                groupedChunkSubKeys =>
                {
                    var outerList = new List<List<ChunkData>>();
                    var chunkList = new List<ChunkData>();
                    bool renderThisChunks = false;
                    foreach (var chunkSubKeys in groupedChunkSubKeys)
                    {
                        var data = world.GetChunkData(chunkSubKeys);
                        
                        foreach (var subKey in data.SubChunks)
                        {
                            if (renderedSubChunks.TryGetValue(new LevelDbWorldKey2(subKey.Key), out KeyAndCrc crc32))
                            {
                                subKey.FoundInDb = true;
                                subKey.ForeignDbId = crc32.DbId;

                                if (forceOverwrite || (crc32.Crc32 != subKey.Crc32))
                                {
                                    renderThisChunks = true;
                                }
                            }
                            else
                            {
                                renderThisChunks = true;
                            }
                        }

                        chunkList.Add(data);
                    }
                    if (renderThisChunks)
                        outerList.Add(chunkList);

                    ProcessedCount++;
                    return outerList;
                }, options);
        }

        public int InputCount => Block.InputCount;
        public int OutputCount => Block.OutputCount;
        public int ProcessedCount { get; set; }
    }
}