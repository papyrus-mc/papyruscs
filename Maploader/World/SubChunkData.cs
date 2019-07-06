using System;
using System.Buffers;

namespace Maploader.World
{
    public class SubChunkData
    {
        public byte[] Key { get; set; }
        public byte[] Data { get; set; }
        public UInt32 Crc32 { get; set; }
        public byte Index { get; set; }
        public bool FoundInDb { get; set; }
        public int ForeignDbId { get; set; }

        public void ClearData()
        {
            ArrayPool<byte>.Shared.Return(Data);
            Data = null;
        }
    }
}