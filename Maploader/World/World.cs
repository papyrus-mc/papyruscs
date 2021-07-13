using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using fNbt;
using leveldb_sharp_std;
using Maploader.Core;
using PapyrusCs.Database;

namespace Maploader.World
{
    public class World
    {
        private DB db;
        private string _dbPath;

        public string WorldName { get; private set; }

        public string WorldPath { get; private set; }

        public void Open(string pathDb)
        {
            if (pathDb == null)
                throw new ArgumentNullException(nameof(pathDb));

            if (!pathDb.ToLowerInvariant().EndsWith("db") &&
                !pathDb.ToLowerInvariant().EndsWith("db" + Path.DirectorySeparatorChar) &&
                !pathDb.ToLowerInvariant().EndsWith("db" + Path.AltDirectorySeparatorChar))
            {
                pathDb = Path.Combine(pathDb, "db");
            }

            WorldPath = DbPathToWorldPath(pathDb);

            LoadWorldName();
            LoadLevelDat();
            LoadDatabase();
        }

        private void LoadWorldName()
        {
            string worldNameFilePath = Path.Combine(WorldPath, "levelname.txt");
            WorldName = File.ReadLines(worldNameFilePath).First();
        }

        private void LoadLevelDat()
        {
            string levelDatFilePath = Path.Combine(WorldPath, "level.dat");
            // Process the level.dat file (NBT).
            // Hopefully will get changes merged into CoreFNBT to allow reading this specific variation on the NBT file format.
        }

        private void LoadDatabase()
        {
            _dbPath = Path.Combine(WorldPath, "db");

            var options = new Options();
            options.Compression = CompressionType.ZlibRaw;

            db = new DB(options, _dbPath);
        }
        private string DbPathToWorldPath(string dbPath)
        {
            return Directory.GetParent(dbPath).FullName;
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
                UIntPtr length;     
                var data = db.Get(key, out length);
                if (data != null)
                {
                    var subChunkData = new SubChunkData()
                    {
                        Index = subChunkIdx,
                        Data = data,
                        DataLength = (int)length,
                        Key = key,
                        Crc32 = Force.Crc32.Crc32CAlgorithm.Compute(data, 0, (int)length)
                    };
                    ret.SubChunks.Add(subChunkData);
                }
            }

            return ret;
        }

        public Chunk GetChunk(int x, int z, ChunkData data)
        {
            Chunk c;
            if (ChunkPool != null)
            {
                c = ChunkPool.Get();
                c.X = x;
                c.Z = z;
            }
            else
            {
               c = new Chunk(x, z);
            }

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
                UIntPtr length;
                var data = db.Get(key, out length);
                if (data != null)
                {
                    subChunks[subChunkIdx] = data;
                    haveData = true;
                }
            }

            if (!haveData) return null;

            Chunk c;
            if (ChunkPool != null)
            {
                c = ChunkPool.Get();
                c.X = x;
                c.Z = z;
            }
            else
            {
                c = new Chunk(x, z);
            }

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


        public IEnumerable<byte[]> OverworldKeys => GetDimension(0);
        public IEnumerable<byte[]> NetherKeys => GetDimension(1);
        public IEnumerable<byte[]> EndKeys => GetDimension(2);

        public IEnumerable<byte[]> GetDimension(int index)
        {
            if (index == 0)
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
            else
            {
                foreach (var element in db)
                {
                    var key = element.Key;
                    if (key.Length != 14)
                        continue;
                    if (key[12] != 47)
                        continue;
                    if (key[8] != index)
                    {
                        continue;
                    }

                    if (key != null)
                        yield return key;
                }
            }
        }

        public IEnumerable<(Guid Uuid, string Name, int DimensionId, float[] Position)> GetPlayerData()
        {
            var playerKeyIndicator = Encoding.UTF8.GetBytes("player_server");

            // Get all records whose key begins with "player_server"
            var playerData = db
                .Where(kvp => kvp.Key.Take(playerKeyIndicator.Length).SequenceEqual(playerKeyIndicator))
                .Select(i => new
                {
                    // Parse the UUID from the key
                    PlayerUuid = Guid.Parse(Encoding.UTF8.GetString(i.Key.Skip(playerKeyIndicator.Length + 1).ToArray())),
                    i.Value
                })
                .ToList();

            if (!playerData.Any())
            {
                yield break;
            }

            foreach (var playerNbtData in playerData)
            {
                using var memoryStream = new MemoryStream(playerNbtData.Value);

                var nbtReader = new NbtReader(memoryStream, false);

                // Example Player NBT tag contents: https://gist.github.com/barrett777/d7c02000aace08c536f13fb1d3f1cf3b
                var playerTag = nbtReader.ReadAsTag();

                yield return
                (
                    // This UUID different than the Minecraft Java edition UUID - I haven't been able to find a way to get a player name using this
                    // For now, I'll just rely on users manually entering names into a JSON file on the web server
                    Uuid: playerNbtData.PlayerUuid,
                    Name: $"Player {playerNbtData.PlayerUuid.ToString().Substring(0, 5)}",
                    DimensionId: playerTag["DimensionId"].IntValue,
                    Position: new[] { playerTag["Pos"][0].FloatValue, playerTag["Pos"][1].FloatValue, playerTag["Pos"][2].FloatValue }
                );
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

        public ChunkPool ChunkPool { get; set; }

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
                            
                            var dictData = new Dictionary<string, Object>();
                            dictData.Add("val", blockData);
                            BlockData b = new BlockData(Table.Lookups[Table.CreateKey(blockId, 0)].name, dictData)
                            {
                                Version = 0,
                            };

                            chunk.SetBlockId(x, yOffset + y, z, ref b, true);
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
                                        var b = localPalette.Keys[state];
                                        // Todo: doppelte keys treten immer noch auf!?
                                        chunk.SetBlockId(x, yOffset + y, z, ref b, true);
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

        private static (string, Dictionary<string, Object>) GetNbtVal(MemoryStream ms)
        {
            string name = "";
            var nbt = new NbtReader(ms, false);
            var dictParams = new Dictionary<string, Object>();
            
            nbt.ReadToFollowing();
            if (!nbt.IsCompound)
                throw new Exception("Could not read nbt");

            if (nbt.ReadToDescendant("name"))
            {
                name = nbt.ReadValueAs<string>();

                nbt.ReadToNextSibling();
                while(nbt.TagType != NbtTagType.End)
                {
                    fNbt.Tags.NbtTag tag = nbt.ReadAsTag();
                    switch(tag.Name)
                    {
                        case "version":
                            continue;
                        case "states":
                                IEnumerable<fNbt.Tags.NbtTag> enumTag = (IEnumerable<fNbt.Tags.NbtTag>)tag;
                                foreach(var subtag in enumTag)
                                {
                                    if((subtag.Name == "direction") || (subtag.Name == "facing_direction") || (subtag.Name == "open_bit") || (subtag.Name == "multi_face_direction_bits") || (subtag.Name == "stripped_bit") || (subtag.Name == "wall_post_bit") || (subtag.Name == "button_pressed_bit") || (subtag.Name == "rail_direction") || (subtag.Name == "rail_data_bit") || (subtag.Name == "bite_counter") || (subtag.Name == "respawn_anchor_charge") || (subtag.Name == "growth") || (subtag.Name == "powered_bit") || (subtag.Name == "upper_block_bit") || (subtag.Name == "hanging") || (subtag.Name == "coral_direction") || (subtag.Name == "coral_hang_type_bit") || (subtag.Name == "dead_bit") || (subtag.Name == "end_portal_eye_bit") || (subtag.Name == "head_piece_bit") || (subtag.Name == "age"))
                                    {
                                        int subtagvalue = GetTagValue(subtag);
                                        dictParams.Add(subtag.Name, subtagvalue); 
                                    }
                                    if((subtag.Name == "color") || (subtag.Name == "lever_direction") || (subtag.Name == "wall_block_type") || (subtag.Name == "pillar_axis") || (subtag.Name == "wood_type") || (subtag.Name == "old_log_type") || (subtag.Name == "new_log_type") || (subtag.Name == "old_leaf_type") || (subtag.Name == "new_leaf_type") || (subtag.Name == "sapling_type") || subtag.Name.StartsWith("wall_connection_type_") || (subtag.Name == "chisel_type") || subtag.Name.StartsWith("stone_slab_type") || (subtag.Name == "sponge_type") || (subtag.Name == "double_plant_type") || (subtag.Name == "flower_type") || (subtag.Name == "sand_type") || (subtag.Name == "stone_type") || (subtag.Name == "damage") || (subtag.Name == "dripstone_thickness") || (subtag.Name == "coral_color") || (subtag.Name == "monster_egg_stone_type") || (subtag.Name == "stone_brick_type"))
                                    {
                                        dictParams.Add(subtag.Name, subtag.StringValue);
                                    }
                                }
                            break;
                        case "val":
                            int value = GetTagValue(tag);
                            dictParams.Add(tag.Name, value); 
                            break;
                    }
                }
            }

            while (!nbt.IsAtStreamEnd)
            {
                nbt.ReadToFollowing();
            }

            return (name, dictParams);

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
                UIntPtr length;
                var data = db.Get(key.Key, out length);
                if (data != null)
                {
                    var subChunkData = new SubChunkData()
                    {
                        Index = kvp.Key,
                        Data = data,
                        DataLength = (int)length,
                        Key = kvp.Value.Key,
                        Crc32 = Force.Crc32.Crc32CAlgorithm.Compute(data, 0, (int)length),
                    };
                    ret.SubChunks.Add(subChunkData);
                }
            }

            return ret;
        }



        public ChunkData GetChunkData(IGrouping<ulong, LevelDbWorldKey2> groupedChunkSubKeys)
        {
            if (db == null)
                throw new InvalidOperationException("Open Db first");

            var ret = new ChunkData
            {
                X = (int) ((ulong) groupedChunkSubKeys.Key >> 32),
                Z = (int)((ulong)groupedChunkSubKeys.Key & 0xffffffff)
            };


            foreach (var kvp in groupedChunkSubKeys)
            {
                var key = kvp;
                UIntPtr length;
                var data = db.Get(key.Key, out length);
                if (data != null)
                {
                    var subChunkData = new SubChunkData()
                    {
                        Index = key.SubChunkId,
                        Data = data,
                        DataLength = (int)length,
                        Key = key.Key,
                        Crc32 = Force.Crc32.Crc32CAlgorithm.Compute(data, 0, (int)length),
                    };
                    ret.SubChunks.Add(subChunkData);
                }
            }

            return ret;
        }

        public ChunkData GetChunkData(int x, int z)
        {
            if (db == null)
                throw new InvalidOperationException("Open Db first");

            var ret = new ChunkData
            {
                X = x,
                Z = z,
            };


            foreach (var kvp in Enumerable.Range(0,15))
            {
                var key = CreateKey(x, z);
                key[9] = (byte)kvp;

                UIntPtr length;
                var data = db.Get(key, out length);
                if (data != null)
                {
                    var subChunkData = new SubChunkData()
                    {
                        Index = (byte)kvp,
                        Data = data,
                        DataLength = (int)length,
                        Key = key,
                        Crc32 = Force.Crc32.Crc32CAlgorithm.Compute(data, 0, (int)length),
                    };
                    ret.SubChunks.Add(subChunkData);
                }
            }

            return ret;
        }

        private static int GetTagValue (fNbt.Tags.NbtTag tag)
        {
            switch (tag.TagType)
            {
                case NbtTagType.Byte:
                    return tag.ByteValue;

                case NbtTagType.Int:
                    return tag.IntValue;

                case NbtTagType.Short:
                    return  tag.ShortValue;

                case NbtTagType.Long:
                    return  (int)tag.LongValue;

                default:
                    return 0;
            }
        }
    }
}
