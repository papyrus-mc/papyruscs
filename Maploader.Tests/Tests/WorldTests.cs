using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Maploader.Renderer;
using Maploader.Renderer.Texture;
using NUnit.Framework;

namespace Maploader.Tests.Tests
{
    [TestFixture]
    public class WorldTests
    {

        [Test]
        public void OpenWorld()
        {
            var dut = new World.World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestLevel\db"));

            dut.Close();
        }

        [Test]
        public void ReadChunk()
        {
            var dut = new World.World();
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
            var dut = new World.World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"C:\Users\deepblue1\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\RhIAAFEzQQA=\db"));

            var b = new Bitmap(16 * 16, 16 * 16);
            var g = Graphics.FromImage(b);

            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Textures\terrain_texture.json"));
            var ts = new TerrainTextureJsonParser(json, "");
            var textures = ts.Textures;
            var finder = new TextureFinder(textures, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures"));

            var c = dut.GetChunk(-2, -2);

            var chunkBlocks = c.Blocks.GroupBy(x => x.Key & 0xFFFF0000);

            foreach (var block in chunkBlocks)
            {
                var blocks = block.ToList();
                var hBlock = blocks.OrderByDescending(x => x.Key & 0xFF).First();

                string texturePath = finder.FindTexturePath(hBlock.Value.Block.Id, hBlock.Value.Block.Data);

                Console.WriteLine($"{hBlock.ToString().PadRight(30)} {texturePath}");
                var tile = finder.GetTextureImage(texturePath);

                if (tile != null)
                {
                    g.DrawImage(tile, hBlock.Value.X * 16, hBlock.Value.Z * 16);
                }
            }
            b.Save(AppDomain.CurrentDomain.BaseDirectory + "\\chunkmissing.png");

        }


        [Test]
        public void TestRender()
        {
            var dut = new World.World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"C:\Users\deepblue1\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\RhIAAFEzQQA=\db"));


            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Textures\terrain_texture.json"));
            var ts = new TerrainTextureJsonParser(json, "");
            var textures = ts.Textures;
            var finder = new TextureFinder(textures, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures"));

            int chunkRadius = 37;
            int centerOffsetX = 65;//65;
            int centerOffsetZ = 65;//65;

            var b = new Bitmap(16 * 16 * (2 * chunkRadius + 1), 16 * 16 * (2 * chunkRadius + 1));
            var g = Graphics.FromImage(b);

            var render = new ChunkRenderer(finder);

            //Parallel.For(-chunkRadius, chunkRadius + 1,new ParallelOptions(){MaxDegreeOfParallelism = 8} , dx =>
            for (int dx = -chunkRadius; dx <= chunkRadius; dx++)
            {
                for (int dz = -chunkRadius; dz <= chunkRadius; dz++)
                {
                    var c = dut.GetChunk(dx + centerOffsetX, dz + centerOffsetZ);
                    render.RenderChunk(c, g, (chunkRadius + dx) * 256, (chunkRadius + dz) * 256);
                }
            };


            b.Save(AppDomain.CurrentDomain.BaseDirectory + "\\chunk.png");


        }

        [Test]
        public void TestRender2()
        {
            var dut = new World.World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"C:\Users\deepblue1\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\RhIAAFEzQQA=\db"));


            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Textures\terrain_texture.json"));
            var ts = new TerrainTextureJsonParser(json, "");
            var textures = ts.Textures;
            var finder = new TextureFinder(textures, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures"));

            int chunkRadius = 2;
            int centerOffsetX = 0;//65;
            int centerOffsetZ = 0;//65;

            var b = new Bitmap(16 * 16 * (2 * chunkRadius + 1), 16 * 16 * (2 * chunkRadius + 1));
            var g = Graphics.FromImage(b);

            var render = new ChunkRenderer(finder);

            //Parallel.For(-chunkRadius, chunkRadius + 1,new ParallelOptions(){MaxDegreeOfParallelism = 8} , dx =>
            for (int dx = -chunkRadius; dx <= chunkRadius; dx++)
            {
                for (int dz = -chunkRadius; dz <= chunkRadius; dz++)
                {
                    var c = dut.GetChunk(dx + centerOffsetX, dz + centerOffsetZ);
                    render.RenderChunk(c, g, (chunkRadius + dx) * 256, (chunkRadius + dz) * 256);
                }
            };


            b.Save(AppDomain.CurrentDomain.BaseDirectory + "\\chunkmini.png");


        }

        [Test]
        public void DataValues()
        {
            var dut = new World.World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"C:\Users\deepblue1\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\RhIAAFEzQQA=\db"));


            int chunkRadius = 35;
            int centerOffsetX = 0;//65;
            int centerOffsetZ = 0;//65;

            int counter = 0;

            //Parallel.For(-chunkRadius, chunkRadius + 1,new ParallelOptions(){MaxDegreeOfParallelism = 8} , dx =>
            for (int dx = -chunkRadius; dx <= chunkRadius; dx++)
            {
                for (int dz = -chunkRadius; dz <= chunkRadius; dz++)
                {
                    var chunk = dut.GetChunk(dx + centerOffsetX, dz + centerOffsetZ);

                    foreach (var block in chunk.Blocks)
                    {
                        counter++;
                        if (block.Value.Block.Id == "minecraft:chest")
                        {
                            Console.WriteLine(block.Value);
                        }
                    }
                }
            };
            Console.WriteLine(counter);

        }

        [Test]
        public void JsonLoadTest3()
        {
            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Textures\terrain_texture.json"));

            var tut = new TerrainTextureJsonParser(json, "");
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
}
