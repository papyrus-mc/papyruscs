using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PapyrusCs.Database
{
    public class Checksum
    {
        [Key] public int Id { get; set; }
        public byte[] LevelDbKey { get; set; }
        public UInt32 Crc32 { get; set; }
    }
}
