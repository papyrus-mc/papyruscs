using System;
using System.Collections.Generic;
using System.Linq;
using PapyrusCs.Database;

namespace PapyrusCs.Strategies
{
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