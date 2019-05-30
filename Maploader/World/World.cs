using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using fNbt;
using leveldb_sharp_std;

namespace Maploader.World
{
    public class World
    {
        private DB db;

        public void Open(string pathDb)
        {
            var options = new Options();
            options.Compression = CompressionType.ZlibRaw;

            db = new DB(options, pathDb);
        }


        public ChunkData GetOverworldChunkData(int x, int z)
        {
            if (db == null)
                throw new InvalidOperationException("Open Db first");

            var ret = new ChunkData();
            var key = CreateKey(x, z);

            for (byte subChunkIdx = 0; subChunkIdx < 15; subChunkIdx++)
            {
                key[9] = subChunkIdx;

                var data = db.Get(key);
                if (data != null)
                {
                    var subChunkData = new SubChunkData()
                    {
                        Index = subChunkIdx,
                        Data = data,
                        Key = key,
                        Crc32 = Force.Crc32.Crc32CAlgorithm.Compute(data)
                    };
                    ret.SubChunks.Add(subChunkData);
                }
            }

            return ret;
        }

        public Chunk GetChunk(int x, int z, ChunkData data)
        {
            Chunk c = new Chunk(x, z);
            foreach (var subChunkRaw in data.SubChunks)
            {
                CopySubChunkToChunk(c, subChunkRaw.Index, subChunkRaw.Data);
            }

            return c;
        }

        public Chunk GetChunk(int x, int z)
        {
            bool haveData = false;
            if (db == null)
                throw new InvalidOperationException("Open Db first");

            var subChunks = new Dictionary<byte, byte[]>();

            var key = CreateKey(x, z);

            for (byte subChunkIdx = 0; subChunkIdx < 15; subChunkIdx++)
            {
                key[9] = subChunkIdx;

                var data = db.Get(key);
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

        public IEnumerable<byte[]> Keys
        {
            get
            {
                foreach (var data in db)
                {
                    yield return data.Key;
                }
            }
        }


        public IEnumerable<byte[]> OverworldKeys
        {
            get
            {
                foreach (var element in db)
                {
                    var key = element.Key;
                    if (key.Length != 10)
                        continue;
                    if (key[8] != 47)
                        continue;
                    if (key != null)
                        yield return key;
                }
            }
        }


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
            CopySubChunkToChunk(chunk, subChunkRawData.Key, subChunkRawData.Value);
        }

        private void CopySubChunkToChunk(Chunk chunk, byte yIndex, byte[] data)
        {
            byte subChunkId = yIndex;
            int yOffset = subChunkId * 16;

            using (MemoryStream ms = new MemoryStream(data))
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
                                blockData = (data[dataPos] >> 4) & 0xF;
                            else
                                blockData = (data[dataPos] >> 0) & 0xF;

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
                            bool isRuntime = (paletteAndFlag & 1) != 0;
                            int bitsPerBlock = paletteAndFlag >> 1;
                            int blocksPerWord = (int)Math.Floor(32.0 / bitsPerBlock);
                            int wordCount = (int)Math.Ceiling(4096.0 / blocksPerWord);
                            long blockIndex = ms.Position;


                            ms.Seek(wordCount * 4, SeekOrigin.Current); //4 bytes per word.
                            //Palette localPallete; //To get 'real' data
                            PersistancePalette localPalette = null;
                            if (isRuntime)
                            {
                                /*localPallete = new RuntimePallete(VarNumberSerializer.readSVarInt(bytes));
                                for (int palletId = 0; palletId < localPallete.size(); palletId++)
                                {
                                    localPallete.put(palletId, VarNumberSerializer.readSVarInt(bytes));
                                }*/
                            }
                            else
                            {
                                localPalette = new PersistancePalette(bs.ReadInt32());
                                for (int palletId = 0; palletId < localPalette.Size; palletId++)
                                {
                                    var (name, val) = GetNbtVal(ms);
                                    localPalette.Put(palletId, name, val);
                                }
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

                                    try
                                    {
                                        // Todo: doppelte keys treten immer noch auf!?
                                        chunk.SetBlockId(x, yOffset + y, z, localPalette.Keys[state], true);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                    //section.setBlockId(x, y, z, localPallete.getBlockId(state));
                                    //section.setBlockData(x, y, z, localPallete.getBlockData(state));
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

        private static byte[] CreateKey(int x, int z)
        {
            // Todo: make it faster
            var key = new byte[10];
            using (var ms = new MemoryStream(key))
            using (var bs = new BinaryWriter(ms))
            {
                bs.Write(x);
                bs.Write(z);
                bs.Write((byte) 47);
            }

            return key;
        }

        public void Close()
        {
            try
            {
                db.Dispose();
                db = null;
            }
            catch
            {

            }
        }

        public byte[] GetData(byte[] key)
        {
            return db[key];
        }

        public ChunkData GetChunkData(GroupedChunkSubKeys groupedChunkSubKeys)
        {
            if (db == null)
                throw new InvalidOperationException("Open Db first");

            var firstSubChunk = groupedChunkSubKeys.Subchunks.First();

            var ret = new ChunkData
            {
                X = firstSubChunk.Value.X,
                Z = firstSubChunk.Value.Z
            };


            foreach (var kvp in groupedChunkSubKeys.Subchunks)
            {
                var key = kvp.Value;

                var data = db.Get(key.Key);
                if (data != null)
                {
                    var subChunkData = new SubChunkData()
                    {
                        Index = kvp.Key,
                        Data = data,
                        Key = kvp.Value.Key,
                        Crc32 = Force.Crc32.Crc32CAlgorithm.Compute(data),
                    };
                    ret.SubChunks.Add(subChunkData);
                }
            }

            return ret;
        }
    }
}
