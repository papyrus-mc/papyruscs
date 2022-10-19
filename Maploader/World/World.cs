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

            for (byte subChunkIdx = 0; subChunkIdx < 16; subChunkIdx++)
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
                CopySubChunkToChunk(c, (sbyte)subChunkRaw.Index, subChunkRaw.Data);
            }

            return c;
        }

        [Obsolete("Use GetChunk(int x, int z, ChunkData data) instead")]
        public Chunk GetChunk(int x, int z)
        {
            bool haveData = false;
            if (db == null)
                throw new InvalidOperationException("Open Db first");

            var subChunks = new Dictionary<byte, byte[]>();

            var key = CreateKey(x, z);

            for (sbyte subChunkIdx = -4; subChunkIdx < 20; subChunkIdx++)
            {
                key[9] = (byte)subChunkIdx;
                UIntPtr length;
                var data = db.Get(key, out length);
                if (data != null)
                {
                    subChunks[(byte)subChunkIdx] = data;
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
            // Cast byte to signed byte (255 -> -1)
            CopySubChunkToChunk(chunk, (sbyte)yIndex, data);
        }

        private void CopySubChunkToChunk(Chunk chunk, sbyte yIndex, byte[] data)
        {
            int yOffset = yIndex * 16;

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
                    case 9:
                        storages = bs.ReadByte();
                        bs.ReadByte();
                        goto case 1;
                    case 1:
                        for (int storage = 0; storage < storages; storage++)
                        {
                            byte paletteAndFlag = bs.ReadByte();
                            bool isRuntime = (paletteAndFlag & 1) != 0;
                            int bitsPerBlock = paletteAndFlag >> 1;
                            if (bitsPerBlock == 0)
                            {
                                PersistancePalette localPalette = new PersistancePalette(1);
                                for (int palletId = 0; palletId < localPalette.Size; palletId++)
                                {
                                    // Console.WriteLine($"    Pallet {palletId}/{localPalette.Size} {paletteAndFlag}");
                                    var (name, val) = GetNbtVal(ms);
                                    localPalette.Put(palletId, name, val);
                                }
                            }
                            else
                            {
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
                        }
                        break;
                    default:
                        Console.WriteLine($"Invalid version: {version}");
                        break;
                }
            }
        }

        private static bool IsNbtTagValInt(fNbt.Tags.NbtTag tag)
        {
            return (
                (tag.Name == "direction")  // [0..3]
                || (tag.Name == "facing_direction")
                || (tag.Name == "open_bit")  // door,trapdoor: [0,1]
                || (tag.Name == "door_hinge_bit")  // door: [0,1]
                || (tag.Name == "multi_face_direction_bits")
                || (tag.Name == "stripped_bit")
                || (tag.Name == "wall_post_bit")
                || (tag.Name == "button_pressed_bit")
                || (tag.Name == "rail_direction")
                || (tag.Name == "rail_data_bit")
                || (tag.Name == "bite_counter")
                || (tag.Name == "respawn_anchor_charge")
                || (tag.Name == "growth")
                || (tag.Name == "powered_bit")
                || (tag.Name == "upper_block_bit")
                || (tag.Name == "hanging")
                || (tag.Name == "coral_direction")  // coral fan (wall): [0..3]
                || (tag.Name == "coral_hang_type_bit")  // coral fan (wall): [0,1]
                || (tag.Name == "dead_bit")  // coral fan (wall),sea pickle: [0,1]
                || (tag.Name == "end_portal_eye_bit")
                || (tag.Name == "head_piece_bit")
                || (tag.Name == "age")
                || (tag.Name == "fill_level")  // cauldron: [0..6]
                || (tag.Name == "extinguished")  // campfire: [0,1]
                || (tag.Name == "composter_fill_level")  // composter: [0..8]
                || (tag.Name == "attached_bit")  // tripwire: [0,1]
                || (tag.Name == "moisturized_amount‌")  // farmland: [0..7]
                || (tag.Name == "facing_bit")  // bed: [0..3]
                || (tag.Name == "head_bit")  // bed: [false,true]
                || (tag.Name == "redstone_signal")  // redstone dust: [0..15]
                || (tag.Name == "output_lit_bit")  // comparator: [0,1]
                || (tag.Name == "output_subtract_bit")  // comparator: [0,1]
                || (tag.Name == "repeater_delay")  // repeater: [0..3]
                || (tag.Name == "candles")  // candle: [0..3] where 0 means 1 candle and 3 means 4 candles
                || (tag.Name == "cluster_count")  // sea pickle: [0..3] where 0 means 1 pickle and 3 means 4 pickles
                || (tag.Name == "lit")  // candle: [0,1]
                || (tag.Name == "vine_direction_bits")  // vines: [0..15]
                || (tag.Name == "ground_sign_direction")  // sign (floor): [0..15]
                || (tag.Name == "item_frame_map_bit")  // item frame: [false,true]
                || (tag.Name == "item_frame_photo_bit")  // item frame?: [false,true]
                || (tag.Name == "coral_fan_direction")  // coral fan (floor): [0,1]
                || (tag.Name == "huge_mushroom_bits")  // mushroom block: [0..15]
            );
        }

        private static bool IsNbtTagValString(fNbt.Tags.NbtTag tag)
        {
            return (
                (tag.Name == "color")
                || (tag.Name == "lever_direction")
                || (tag.Name == "wall_block_type")
                || (tag.Name == "pillar_axis")  // many (e.g. logs) x,y,z
                || (tag.Name == "wood_type")  // planks: oak,spruce,birch,jungle,acacia,dark_oak
                || (tag.Name == "old_log_type")  // logs: oak,spruce,birch,jungle
                || (tag.Name == "new_log_type")  // logs: acacia,dark_oak
                || (tag.Name == "old_leaf_type")  // leaves: oak,spruce,birch,jungle
                || (tag.Name == "new_leaf_type")  // leaves: acacia,dark_oak
                || (tag.Name == "sapling_type")  // saplings,bamboo sapling (unused): oak,spruce,birch,jungle,acacia,dark_oak
                || tag.Name.StartsWith("wall_connection_type_")
                || (tag.Name == "chisel_type")
                || tag.Name.StartsWith("stone_slab_type")
                || (tag.Name == "sponge_type")
                || (tag.Name == "double_plant_type")
                || (tag.Name == "flower_type")
                || (tag.Name == "sand_type")
                || (tag.Name == "stone_type")
                || (tag.Name == "damage")
                || (tag.Name == "dripstone_thickness")
                || (tag.Name == "coral_color")  // coral: blue,pink,purple,red,yellow
                || (tag.Name == "monster_egg_stone_type")
                || (tag.Name == "stone_brick_type")
                || (tag.Name == "dirt_type")  // dirt: normal,coarse
                || (tag.Name == "cauldron_liquid")  // cauldron: lava,water,powder_snow
                || (tag.Name == "torch_facing_direction")  // torch: north,east,south,west,top,unknown
                || (tag.Name == "attachment")  // grindstone,bell: standing,side,hanging,multiple
                || (tag.Name == "sand_stone_type")  // sandstone: default,heiroglyphs,cut,smooth
                || (tag.Name == "tall_grass_type")  // tall grass: default,tall,fern,snow
                || (tag.Name == "sea_grass_type")  // seagrass: default,double_bot,double_top
                || (tag.Name == "bamboo_leaf_size")  // bamboo: no_leaves,small_leaves,large_leaves
                || (tag.Name == "bamboo_stalk_thickness")  // bamboo: thin,thick
                || (tag.Name == "cracked_state")  // turtle eggs: no_cracks,cracked,max_cracked
                || (tag.Name == "turtle_egg_count")  // turtle eggs: one_egg,two_egg,three_egg,four_egg
            );
        }

        private static bool IsNbtTagValIgnored(fNbt.Tags.NbtTag tag)
        {
            return (
                (tag.Name == "infiniburn_bit")  // bedrock: [false,true]
                || (tag.Name == "persistent_bit")  // leaves: [false,true]
                || (tag.Name == "update_bit")  // leaves: [false,true]
                || (tag.Name == "upside_down_bit")  // stairs: [false,true]
                || (tag.Name == "weirdo_direction")  // stairs: [0..3]
                || (tag.Name == "age_bit")  // saplings,bamboo: [false,true]
                || (tag.Name == "liquid_depth")  // water/lava: [0]-source [1..7]-distance [8..15]-falling
                || (tag.Name == "no_drop_bit")  // head: [0,1]
                || (tag.Name.StartsWith("brewing_stand_slot_") && tag.Name.EndsWith("_bit"))  // brewing stand: [0,1]
                || (tag.Name == "disarmed_bit")  // tripwire: [0,1]
                || (tag.Name == "suspended_bit")  // tripwire: [0,1]
                || (tag.Name == "honey_level")  // bee nest/hive: [0..5]
                || (tag.Name == "height")  // snow: [0..7]
                || (tag.Name == "covered_bit")  // snow: [0,1]
                || (tag.Name == "occupied_bit")  // bed: [false,true]
                || (tag.Name == "stability")  // scaffolding: [0..7]
                || (tag.Name == "stability_check")  // scaffolding: [false,true]
                || (tag.Name == "toggle_bit")  // hopper,bell: [0,1]
                || (tag.Name == "triggered_bit")  // dispenser,dropper: [false,true]
                || (tag.Name == "allow_underwater_bit")  // tnt: [false,true]
                || (tag.Name == "explode_bit")  // tnt: [false,true]
                || (tag.Name == "deprecated")  // hay bale: [0..3], not used
                || (tag.Name == "in_wall_bit")  // fence gate: [0,1]
                || (tag.Name == "twisting_vines_age")  // twisting vines: [0..25]
                || (tag.Name == "weeping_vines_age")  // weeping vines: [0..25]
                || (tag.Name == "top_slot_bit")  // slab: [false,true]
                || (tag.Name == "kelp_age")  // kelp
            );
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
                while (nbt.TagType != NbtTagType.End)
                {
                    fNbt.Tags.NbtTag tag = nbt.ReadAsTag();
                    switch (tag.Name)
                    {
                        case "version":
                            continue;
                        case "states":
                            IEnumerable<fNbt.Tags.NbtTag> enumTag = (IEnumerable<fNbt.Tags.NbtTag>)tag;
                            foreach (var subtag in enumTag)
                            {
                                if (IsNbtTagValInt(subtag))
                                {
                                    int subtagvalue = GetTagValue(subtag);
                                    dictParams.Add(subtag.Name, subtagvalue);
                                }
                                else if (IsNbtTagValString(subtag))
                                {
                                    dictParams.Add(subtag.Name, subtag.StringValue);
                                }
                                else if (IsNbtTagValIgnored(subtag))
                                {
                                    continue;
                                }
                                else
                                {
                                    // Console.WriteLine($"SUBTAG({subtag.TagType.ToString()} {subtag.Name})");
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


            foreach (byte kvp in Enumerable.Range(0, 16))
            {
                var key = CreateKey(x, z);
                key[9] = kvp;

                UIntPtr length;
                var data = db.Get(key, out length);
                if (data != null)
                {
                    var subChunkData = new SubChunkData()
                    {
                        Index = kvp,
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
