using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DmitryBrant.ImageFormats;
using Maploader.Source;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Maploader.Tests
{
    [TestFixture]
    public class WorldTests
    {

        [Test]
        public void OpenWorld()
        {
            var dut = new World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestLevel\db"));

            dut.Close();
        }

        [Test]
        public void ReadChunk()
        {
            var dut = new World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestLevel\db"));

            var c = dut.GetChunk(0, 0);

            var blocks = c.Blocks.Select(x => x.Value).Distinct();
            foreach (var block in blocks)
            {
                Console.WriteLine(block);
            }
        }

        [Test]
        public void MissingChunk()
        {
            var dut = new World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"C:\Users\deepblue1\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\RhIAAFEzQQA=\db"));

            var b = new Bitmap(16 * 16, 16 * 16);
            var g = Graphics.FromImage(b);

            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Textures\terrain_texture.json"));
            var ts = new TextureSource(json, "");
            var textures = ts.Textures;
            var finder = new TextureFinder(textures, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures"));

            var c = dut.GetChunk(-2, -2);

            var chunkBlocks = c.Blocks.GroupBy(x => x.Key.X * 256 + x.Key.Z);

            foreach (var block in chunkBlocks)
            {
                var blocks = block.ToList();
                var hBlock = blocks.OrderByDescending(x => x.Key.Y).First();

                string texturePath = finder.FindTexturePath(hBlock.Value.Id, hBlock.Value.Data);

                Console.WriteLine($"{hBlock.ToString().PadRight(30)} {texturePath}");
                var tile = finder.GetTextureImage(texturePath);

                if (tile != null)
                {
                    g.DrawImage(tile, hBlock.Key.X * 16, hBlock.Key.Z * 16);
                }
            }
            b.Save(AppDomain.CurrentDomain.BaseDirectory + "\\chunkmissing.png");

        }


        [Test]
        public void CombineTextures()
        {
            var dut = new World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"C:\Users\deepblue1\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\RhIAAFEzQQA=\db"));


            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Textures\terrain_texture.json"));
            var ts = new TextureSource(json, "");
            var textures = ts.Textures;
            var finder = new TextureFinder(textures, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures"));

            int chunkRadius = 20;
            int centerOffsetX = 65;
            int centerOffsetY = 65;

            var b = new Bitmap(16 * 16 * (2 * chunkRadius + 1), 16 * 16 * (2 * chunkRadius + 1));
            var g = Graphics.FromImage(b);

            var lockObj = new  object();


            //Parallel.For(-chunkRadius, chunkRadius + 1, dx =>
            for (int dx = -chunkRadius; dx <= chunkRadius; dx++)
            {
                for (int dy = -chunkRadius; dy <= chunkRadius; dy++)
                {
                    var c = dut.GetChunk(dx + centerOffsetX, dy + centerOffsetY);
                    var chunkBlocks = c.Blocks.GroupBy(x => x.Key.X * 256 + x.Key.Z);

                    foreach (var block in chunkBlocks)
                    {
                        var blocks = block.ToList();
                        var hBlock = blocks.OrderByDescending(x => x.Key.Y).First();

                        string texturePath = finder.FindTexturePath(hBlock.Value.Id, hBlock.Value.Data);

                        var tile = finder.GetTextureImage(texturePath);

                        if (tile != null)
                        {
                            lock (lockObj)
                            {
                                g.DrawImage(tile, (chunkRadius + dx) * 256 + hBlock.Key.X * 16, (chunkRadius + dy) * 256 + hBlock.Key.Z * 16);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"{hBlock.ToString().PadRight(30)} {texturePath}");
                        }
                    }
                }
            }//);


            b.Save(AppDomain.CurrentDomain.BaseDirectory + "\\chunk.png");


        }

        public class TextureFinder
        {
            private readonly Dictionary<string, Texture> textures;
            private readonly string path;

            public TextureFinder(Dictionary<string, Texture> textures, string path)
            {
                this.textures = textures;
                this.path = path;
            }

            public string FindTexturePath(string name, long data)
            {
                name = name.Replace("minecraft:", "");

                if (name.StartsWith("leaves2"))
                    return "textures/blocks/leaves_oak_carried";
                else if (name.StartsWith("leaves"))
                    return "textures/blocks/" + name + "_oak_carried";
                if (textures.ContainsKey(name))
                {
                    return GetTexture(name);
                }
                else
                {
                    switch (name)
                    {
                        case "water":
                            return "textures/blocks/water_placeholder";
                        case "lava":
                            return "textures/blocks/lava_placeholder";
                        case "fire":
                            return "textures/blocks/fire_0_placeholder";
                        case "bed":
                            return "textures/blocks/bed_head_top";
                        case "wooden_slab":
                            return GetTexture("planks");
                        case "double_wooden_slab":
                            return GetTexture("planks");
                        case "wooden_pressure_plate":
                            return GetTexture("planks");
                        case "oak_stairs":
                            return GetTexture("planks");
                        case "brick_stairs":
                            return GetTexture("brick");
                        case "spruce_stairs":
                            return GetTexture("spruce_planks");
                        case "birch_stairs":
                            return GetTexture("birch_planks");
                        case "dark_oak_stairs":
                            return GetTexture("dark_oak_planks");
                        case "log":
                            return GetTexture("log_top");
                        case "carpet":
                            return GetTexture("wool");
                        case "hay_block":
                            return GetTexture("hayblock_top");
                        case "hopper":
                            return GetTexture("hopper_top");
                        case "double_plant":
                            return GetTexture("double_plant_top");
                        case "rail":
                            return GetTexture("rail_normal");
                        case "golden_rail":
                            return GetTexture("rail_golden");
                        case "red_mushroom_block":
                            return GetTexture("mushroom_red");
                        case "brown_mushroom_block":
                            return GetTexture("mushroom_brown");
                        case "mossy_cobblestone":
                            return GetTexture("cobblestone_mossy");
                        case "glass_pane":
                            return GetTexture("glass_pane_top");
                        case "fence_gate":
                            return GetTexture("grass_carried_top");
                        case "fence":
                            return GetTexture("grass_carried_top");
                        case "grass":
                            return GetTexture("grass_carried_top");
                        case "seagrass":
                            return GetTexture("seagrass_carried");
                        case "sandstone":
                            return GetTexture("sandstone_top");
                        case "grass_path":
                            return GetTexture("grass_path_top");
                        case "lit_pumpkin":
                            return GetTexture("pumpkin_top");
                        case "pumpkin":
                            return GetTexture("pumpkin_top");
                        case "torch":
                            return GetTexture("torch_on");
                        case "nether_brick_stairs":
                            return GetTexture("nether_brick");
                        case "stone_brick_stairs":
                            return GetTexture("stonebrick");
                        case "crafting_table":
                            return GetTexture("crafting_table_top");
                        case "double_stone_slab":
                            return GetTexture("stone");
                        case "stone_stairs":
                            return GetTexture("stone");
                        case "chest":
                            return GetTexture("chest_inventory_top");
                        case "snow_layer":
                            return GetTexture("snow");
                    }
                }

                return null;

            }

            public Dictionary<string, Bitmap> Cache { get; } = new Dictionary<string, Bitmap>();

            public Bitmap GetTextureImage(string localPath)
            {
                if (localPath == null)
                    return null;

             

                localPath = localPath.Replace("textures/", "");
                localPath = localPath.Replace("/", "\\");

                lock (Cache)
                {
                    if (Cache.ContainsKey(localPath))
                    {
                        return Cache[localPath];
                    }
                }

                try
                {
                    string extension = ".jpg";
                    string filepath = Path.Combine(path, localPath);
                    Bitmap b = null;
                    if (File.Exists(filepath + ".png"))
                    {
                        extension = ".png";
                        b = new Bitmap(filepath + extension);
                    }
                    else if (File.Exists(filepath + ".tga"))
                    {
                        extension = ".tga";
                        b = TgaReader.Load(filepath + extension);
                    }

                    lock (Cache)
                    {
                        if (!Cache.ContainsKey(localPath))
                        {
                            Cache.Add(localPath, b);
                        }
                    }

                    return b;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return null;
            }

            private string GetTexture(string name)
            {
                if (textures.ContainsKey(name))
                {
                    return textures[name].Subtextures.First().Path;
                }

                return null;
            }
        }

        [Test]
        public void JsonLoadTest3()
        {
            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Textures\terrain_texture.json"));

            var tut = new TextureSource(json, "");
            var textures = tut.Textures;
            foreach (var t in textures)
                if (t.Value.Subtextures.Count == 1)
                {
                    Console.WriteLine(t.Key.PadRight(30) + " " + t.Value.Subtextures.First().Path);
                }
                else
                {
                    Console.WriteLine(t.Key.PadRight(27) + " " + t.Value.Subtextures.Count);


                }
        }


    }

    public class TextureSource
    {
        private readonly string json;
        private readonly string path;

        public TextureSource(string json, string path)
        {
            this.json = json;
            this.path = path;

            ParseJsonFile(json);
        }

        public Dictionary<string, Texture> Textures { get; } = new Dictionary<string, Texture>();

        private void ParseJsonFile(string json)
        {
            var jobj = JObject.Parse(json);
            var texturesData = jobj.SelectToken("texture_data").ToObject<JObject>();

            //Console.WriteLine(tobj);
            int i = 0;
            foreach (var textureData in texturesData.Properties())
            {
                var texture = new Texture(textureData.Name);
                Textures.Add(textureData.Name, texture);

                var textureObject = textureData.Value.ToObject<JObject>();
                var textures = textureObject["textures"];

                if (textures is JObject jo)
                {
                    HandleJOject(texture, jo);

                }
                else if (textures is JArray ja)
                {
                    foreach (var t in ja)
                    {
                        if (t is JObject ajo)
                        {
                            HandleJOject(texture, ajo);
                        }
                        else
                        {
                            HandleJToken(texture, t);
                        }
                    }
                }
                else
                {
                    HandleJToken(texture, textures);
                }
            }
        }

        private void HandleJOject(Texture texture, JObject jo)
        {
            string overlayColor = null;
            string tintColor = null;
            string path = jo["path"].Value<string>();

            if (jo.ContainsKey("overlay_color"))
                overlayColor = jo["overlay_color"].Value<string>();
            if (jo.ContainsKey("tint_color"))
                tintColor = jo["tint_color"].Value<string>();

            texture.AddSubTexture(path, overlayColor, tintColor);
        }

        private void HandleJToken(Texture texture, JToken textures)
        {
            texture.AddSubTexture(textures.Value<string>());
        }
    }

    public class SubTexture
    {
        public string Path { get; }
        public string OverlayColor { get; }

        public bool HasOverlayColor { get; }

        public SubTexture(string path, string overlayColor = null)
        {
            Path = path;
            HasOverlayColor = overlayColor != null;
            OverlayColor = overlayColor;
        }

        public override string ToString()
        {
            return $"{Path}";
        }
    }

    public class Texture
    {
        public Texture(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public List<SubTexture> Subtextures { get; } = new List<SubTexture>();


        public void AddSubTexture(string value, string overlayColor = null, string tintColor = null)
        {
            Subtextures.Add(new SubTexture(value, overlayColor));
        }

        public override string ToString()
        {
            return $"{Name} - {Subtextures.Count} Subtextures";
        }
    }

}
