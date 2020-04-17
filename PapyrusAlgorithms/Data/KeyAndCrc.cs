namespace PapyrusCs
{
    public struct KeyAndCrc
    {
        public KeyAndCrc(int dbId, uint crc32)
        {
            DbId = dbId;
            Crc32 = crc32;
        }

        public uint Crc32 { get; set; }
        public int  DbId { get; set; }
    }
}