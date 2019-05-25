using System.Collections.Generic;

namespace Maploader.World
{
    public class ChunkData
    {
        public List<SubChunkData> SubChunks { get; } = new List<SubChunkData>();
        public bool HasData => SubChunks.Count > 0;
        public bool Empty => SubChunks.Count == 0;
        public int X { get; set; }
        public int Z { get; set; }
    }
}