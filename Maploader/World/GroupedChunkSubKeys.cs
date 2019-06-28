using System;
using System.Collections.Generic;
using System.Linq;
using PapyrusCs.Database;

namespace Maploader.World
{
    public class GroupedChunkSubKeys
    {
        public Dictionary<byte, LevelDbWorldKey2> Subchunks { get; } = new Dictionary<byte, LevelDbWorldKey2>();

        public GroupedChunkSubKeys(IGrouping<ulong, LevelDbWorldKey2> groupedSubChunks)
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

        public GroupedChunkSubKeys(LevelDbWorldKey2 mainKey)
        {
            var keys = Enumerable.Range(0, 16).Select(x =>
            {
               var newkey = new LevelDbWorldKey2(mainKey.Key);
               newkey.Key[9] = (byte)x;
               newkey.SubChunkId = (byte) x;
               return newkey;
            });
      
            foreach (var sc in keys)
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