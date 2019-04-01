using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Maploader.Source.LookupTable
{
    public class LookupTable
    {
        public LookupTable()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"LookupTable\runtimeid_table.json");
            var json = File.ReadAllText(path);

            var table = JsonConvert.DeserializeObject<List<LookUp>>(json);
            foreach (var t in table)
            {
                if (t.data.HasValue)
                {
                    var key = new Coordinate2D(t.id, t.data.Value);
                    if (!Lookups.ContainsKey(key))
                    {
                        Lookups.Add(key, t);
                    }
                }
            }
        }

        public Dictionary<Coordinate2D, LookUp> Lookups { get; } = new Dictionary<Coordinate2D, LookUp>();
    }

    public class LookUp
    {
        public int? data;
        public int id;
        public string name;
        public int runtimeId;
    }
}