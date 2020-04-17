using System;
using System.ComponentModel.DataAnnotations;

namespace PapyrusAlgorithms.Database
{
    public class Checksum
    {
        [Key] public int Id { get; set; }
        public byte[] LevelDbKey { get; set; }
        public string Profile { get; set; }
        public UInt32 Crc32 { get; set; }
    }
}