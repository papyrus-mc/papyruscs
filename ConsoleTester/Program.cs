using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using fNbt;
using leveldb_sharp_std;

namespace ConsoleTester
{
    class Program
    {
        public static byte[] CreateKey(int x, int z, int type, int subChunk)
        {
            var ret = new byte[]
            {
                (byte) ((x >> 0) & 0xFF),
                (byte) ((x >> 8) & 0xFF),
                (byte) ((x >> 16) & 0xFF),
                (byte) ((x >> 24) & 0xFF),
                (byte) ((z >> 0) & 0xFF),
                (byte) ((z >> 8) & 0xFF),
                (byte) ((z >> 16) & 0xFF),
                (byte) ((z >> 24) & 0xFF),
                (byte) (type),
                (byte) (subChunk),
            };
            return ret;
        }

        static void Main(string[] args)
        {
            //string path = @"C:\Users\deepblue1\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\sImeXEdXdgA=\db";
            string path2 = @"C:\Users\deepblue1\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\RhIAAFEzQQA=\db";
            var op = new Options();
            op.Compression = CompressionType.ZlibRaw;
            
            var db = DB.Open(op, path2);


            int i = 0;
            foreach (var e in db)
            {
                if (e.Key.Length >= 10 && e.Key[8] == 47)
                {
                    if (e.Key[9] == 0)
                    {
                        i++;
                    }

                    using (var mskey = new MemoryStream(e.Key))
                    using (var bskey = new BinaryReader(mskey))
                    {

                        int x = bskey.ReadInt32();
                        int z = bskey.ReadInt32();
                        bskey.ReadByte();
                        int s = bskey.ReadByte();


                        if (i % 100 == 0)
                        {
                            Console.WriteLine($"Chunk {i.ToString().PadLeft(8)}::  x={x.ToString().PadLeft(8)}, z={z.ToString().PadLeft(8)}, subchunk={s}");
                        }
                    }

                    using (MemoryStream ms = new MemoryStream(e.Value))
                    using (var bs = new BinaryReader(ms, Encoding.Default))
                    {

                        int version = bs.ReadByte();
                        int storages = 1;
                        switch (version)
                        {
                            case 8:
                                storages = bs.ReadByte();
                                goto case 1;
                            case 1:
                                for (int storage = 0; storage < storages; storage++)
                                {
                                    byte paletteAndFlag = bs.ReadByte();
                                    bool isRuntime = (paletteAndFlag & 1) != 0;
                                    int bitsPerBlock = paletteAndFlag >> 1;
                                    int blocksPerWord = (int) Math.Floor(32.0 / bitsPerBlock);
                                    int wordCount = (int) Math.Ceiling(4096.0 / blocksPerWord);
                                    long blockIndex = ms.Position;


                                    ms.Seek(wordCount * 4, SeekOrigin.Current); //4 bytes per word.
                                    //Palette localPallete; //To get 'real' data
                                    PersistancePallete localPallete = null;
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
                                        //Says varint, but I don't think so on flatfile :wink:
                                        localPallete = new PersistancePallete(bs.ReadInt32());
                                        for (int palletId = 0; palletId < localPallete.size(); palletId++)
                                        {


                                            Int32 runtimeId = GetNbtVal(ms);
                                            localPallete.put(palletId, runtimeId);
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
                                            int state = (word >> ((position % blocksPerWord) * bitsPerBlock)) & ((1 << bitsPerBlock) - 1);
                                            int x = (position >> 8) & 0xF;
                                            int y = position & 0xF;
                                            int z = (position >> 4) & 0xF;
                                            //section.setBlockId(x, y, z, localPallete.getBlockId(state));
                                            //section.setBlockData(x, y, z, localPallete.getBlockData(state));
                                            position++;
                                        }
                                    }

                                    ms.Position = afterPaletteIndex;
                                }

                                break;
                        }


                    }
                }
            }

            //Console.WriteLine(db.Count(x => x.Key[8] == 47));
            Console.WriteLine(db.Count());

            db.Get(CreateKey(0, 0, 47, 0));

            Console.ReadKey();

        }

        private static int GetNbtVal(MemoryStream ms)
        {
                int value = 0;
            var nbt = new NbtReader(ms, false);
            nbt.ReadToFollowing();
            if (!nbt.IsCompound)
                return -1;
            if (nbt.ReadToDescendant("val"))
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
                        value = (int) nbt.ReadValueAs<long>();
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


            return value;

        }
    }

    internal class PersistancePallete
    {
        private readonly int readIntLe;
        public Dictionary<Int32, Int32> keys = new Dictionary<int, int>();
        public void put(Int32 key, Int32 runtimeId)
        {
            keys[key] = runtimeId;
        }

        public int size()
        {
            return readIntLe;
        }

        public PersistancePallete(int readIntLe)
        {
            this.readIntLe = readIntLe;
        }
    }
}
