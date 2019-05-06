using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using fNbt;
using leveldb_sharp_std;

namespace Maploader.World
{
    public class DbWrapper
    {
        public DB db { get; private set; }

        public void Open(string pathDb)
        {
            var options = new Options();
            options.Compression = CompressionType.ZlibRaw;

            db = new DB(options, pathDb);
        }
        public void Close()
        {
            db.Dispose();
            db = null;
        }
    }

    public class World
    {
        private byte[] staticKey = new byte[10];
        private DB db;

        public World(DbWrapper wrapper)
        {
            this.db = wrapper.db;
            staticKey[8] = 47;
        }


        public Chunk GetChunk(int x, int z)
        {
            bool haveData = false;
            if (db == null)
                throw new InvalidOperationException("Open Db first");

            var subChunks = new Dictionary<byte, byte[]>();

            CreateKey(x, z, ref staticKey);

            for (byte subChunkIdx = 0; subChunkIdx < 15; subChunkIdx++)
            {
                staticKey[9] = subChunkIdx;

                var data = db.Get(staticKey);
                if (data != null)
                {
                    subChunks[subChunkIdx] = data;
                    haveData = true;
                }
            }

            if (!haveData) return null;

            Chunk c = new Chunk(x, z);
            foreach (var subChunkRaw in subChunks)
            {
                CopySubChunkToChunk(c, subChunkRaw);
            }
            return c;

        }

        private LookupTable.BlockLookupTable Table { get; } = new LookupTable.BlockLookupTable();

        public IEnumerable<Coordinate2D> ChunkKeys
        {
            get
            {
                foreach (var element in db)
                {
                    var key = Coordinate2D.FromKey(element.Key);
                    if (key != null)
                        yield return key;
                }
            }

        }

        private void CopySubChunkToChunk(Chunk chunk, KeyValuePair<byte, byte[]> subChunkRawData)
        {
            byte subChunkId = subChunkRawData.Key;
            int yOffset = subChunkId * 16;

            using (MemoryStream ms = new MemoryStream(subChunkRawData.Value))
            using (var bs = new BinaryReader(ms, Encoding.Default))
            {

                int version = bs.ReadByte();
                int storages = 1;
                switch (version)
                {
                    case 0:
                        for (int position = 0; position < 4096; position++)
                        {
                            int blockId = bs.ReadByte();
                            int blockData = 0;

                            int dataPos = 1 + 4096 + (position / 2);

                            if (position % 2 == 0)
                                blockData = (subChunkRawData.Value[dataPos] >> 4) & 0xF;
                            else
                                blockData = (subChunkRawData.Value[dataPos] >> 0) & 0xF;

                            int x = (position >> 8) & 0xF;
                            int y = position & 0xF;
                            int z = (position >> 4) & 0xF;

                            chunk.SetBlockId(x, yOffset + y, z,
                                new BlockData(Table.Lookups[Table.CreateKey(blockId, 0)].name, (blockData))
                                {
                                    Version = 0,
                                },
                                true);
                        }

                        break;

                    case 8:
                        storages = bs.ReadByte();
                        goto case 1;
                    case 1:
                        for (int storage = 0; storage < storages; storage++)
                        {
                            byte paletteAndFlag = bs.ReadByte();
                            //bool isRuntime = (paletteAndFlag & 1) != 0;
                            int bitsPerBlock = paletteAndFlag >> 1;
                            int blocksPerWord = (int)Math.Floor(32.0 / bitsPerBlock);
                            int wordCount = (int)Math.Ceiling(4096.0 / blocksPerWord);
                            long blockIndex = ms.Position;


                            ms.Seek(wordCount * 4, SeekOrigin.Current); //4 bytes per word.

                            PersistancePalette localPalette = null;
                            localPalette = new PersistancePalette(bs.ReadInt32());
                            for (int palletId = 0; palletId < localPalette.Size; palletId++)
                            {
                                var (name, val) = GetNbtVal(ms);
                                localPalette.Put(palletId, name, val);
                            }

                            long afterPaletteIndex = ms.Position;
                            ms.Position = blockIndex;

                            int position = 0;
                            for (int wordi = 0; wordi < wordCount; wordi++)
                            {
                                int word = bs.ReadInt32();
                                for (int block = 0; block < blocksPerWord; block++)
                                {
                                    // Todo ist diese Zeile hier richtig?

                                    int state = (word >> ((position % blocksPerWord) * bitsPerBlock)) & ((1 << bitsPerBlock) - 1);
                                    int x = (position >> 8) & 0xF;
                                    int y = position & 0xF;
                                    int z = (position >> 4) & 0xF;
                                    chunk.SetBlockId(x, yOffset + y, z, localPalette.Keys[state], true);

                                    position++;

                                    // Todo: irgendwas läuft hier noch nicht ganz rund, wir brechen mal ab
                                    if (position >= 4096)
                                        break;
                                }
                                // Todo: irgendwas läuft hier noch nicht ganz rund, wir brechen mal ab
                                if (position >= 4096)
                                    break;
                            }
                            ms.Position = afterPaletteIndex;
                        }
                        break;
                }
            }
        }

        private static (string, int) GetNbtVal(MemoryStream ms)
        {
            int value = 0;
            string name = "";
            var nbt = new NbtReader(ms, false);
            nbt.ReadToFollowing();
            if (!nbt.IsCompound)
                throw new Exception("Could not read nbt");

            if (nbt.ReadToDescendant("name"))
            {
                name = nbt.ReadValueAs<string>();
            }
            if (nbt.ReadToNextSibling("val"))
            {
                switch (nbt.TagType)
                {
                    case NbtTagType.Int:
                        value = nbt.ReadValueAs<int>();
                        break;

                    case NbtTagType.Short:
                        value = nbt.ReadValueAs<short>();
                        break;
                    case NbtTagType.Long:
                        value = (int)nbt.ReadValueAs<long>();
                        break;
                    default:
                        Console.WriteLine("could not comprehend val");
                        break;
                }
            }

            while (!nbt.IsAtStreamEnd)
            {
                nbt.ReadToFollowing();
            }

            return (name, value);

        }

        private static void CreateKey(int x, int z, ref byte[] key)
        {
            // Todo: make it faster
            //var key = new byte[10];
            //using (var ms = new MemoryStream(key))
            //using (var bs = new BinaryWriter(ms))
            //{
            //bs.Write(x);
            //bs.Write(z);
            //bs.Write((byte) 47);
            //}

            key[0] = (byte)(x & 0xFF);
            key[1] = (byte)((x >> 8)& 0xFF);
            key[2] = (byte)((x >> 16)& 0xFF);
            key[3] = (byte) ((x >> 24)& 0xFF);
            key[4] = (byte)(z & 0xFF);
            key[5] = (byte)((z >> 8) & 0xFF);
            key[6] = (byte)((z >> 16) & 0xFF);
            key[7] = (byte)((z >> 24) & 0xFF);
        }
    }
}
