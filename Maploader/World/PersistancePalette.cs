using System;
using System.Collections.Generic;

namespace Maploader.World
{
    public class PersistancePalette
    {
        public Dictionary<Int32, BlockData> Keys = new Dictionary<int, BlockData>();

        public void Put(Int32 key, string name, List<KeyValuePair<string, Object>> value)
        {
            Keys[key] = new BlockData(name, value) { Version = 8};
        }

        public int Size { get; }

        public PersistancePalette(int size)
        {
            this.Size = size;
        }
    }
}