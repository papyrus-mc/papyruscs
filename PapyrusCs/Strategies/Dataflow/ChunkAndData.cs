using System.Collections.Generic;
using Maploader.World;

namespace PapyrusCs.Strategies.Dataflow
{
    public class ChunkAndData
    {
        public Chunk C { get; }
        public List<SubChunkData> Cd { get; }
        

        public ChunkAndData(Chunk c, ChunkData cd)
        {
            C = c;
            Cd = cd.SubChunks;
            foreach (var subChunkData in Cd)
            {
                subChunkData.ClearData();
            }
        }
    }
}