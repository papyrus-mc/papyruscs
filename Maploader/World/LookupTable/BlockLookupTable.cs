using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Maploader.World.LookupTable
{
    public class BlockLookupTable
    {
        public BlockLookupTable()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"LookupTable", "runtimeid_table.json");
            var json = File.ReadAllText(path);

            var table = JsonConvert.DeserializeObject<List<LookUp>>(json);
            foreach (var t in table)
            {
                if (t.data.HasValue)
                {
                    var key = CreateKey(t.id, t.data.Value);
                    if (!Lookups.ContainsKey(key))
                    {
                        Lookups.Add(key, t);
                    }
                }
            }
        }

        public Dictionary<UInt32, LookUp> Lookups { get; } = new Dictionary<UInt32, LookUp>();

        public UInt32 CreateKey(int v1, int v2)
        {
            return (UInt32) (((v1 & 0xFFFF) << 16) + (v2 & 0xFFFF));
        }
    }

    public class LookUp
    {
        public int? data;
        public int id;
        public string name;
        public int runtimeId;
    }
}