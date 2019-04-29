using Maploader.Renderer;
using Maploader.Renderer.Texture;
using Maploader.World;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maploader.Tests.Tests
{
    [TestFixture]
    class BenchmarkTests
    {
        [Test]
        public void OpenWorld()
        {
            var dut = new World.World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "benchmark", "world", "db"));
            dut.Close();

            Assert.Pass();
        }

        [Test]
        public void BenchmarkRender()
        {
            var dut = new World.World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "benchmark", "world", "db"));

            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Textures\terrain_texture.json"));
            var ts = new TerrainTextureJsonParser(json, "");
            var textures = ts.Textures;
            var finder = new TextureFinder(textures, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures"));

            int chunkRadius = 1;
            int centerOffsetX = 1;//65;
            int centerOffsetZ = 1;//65;

            var b = new Bitmap(16 * 16 * (2 * chunkRadius + 1), 16 * 16 * (2 * chunkRadius + 1));
            var g = Graphics.FromImage(b);

            var render = new ChunkRenderer(finder);

            //Parallel.For(-chunkRadius, chunkRadius + 1,new ParallelOptions(){MaxDegreeOfParallelism = 8} , dx =>
            for (int dz = -chunkRadius; dz <= chunkRadius; dz++)
            {
                for (int dx = -chunkRadius; dx <= chunkRadius; dx++)
                {
                    var c = dut.GetChunk(dx + centerOffsetX, dz + centerOffsetZ);
                    if (c != null)
                    {
                        render.RenderChunk(c, g, (chunkRadius + dx) * 256, (chunkRadius + dz) * 256);
                    }
                }
            };

            b.Save(AppDomain.CurrentDomain.BaseDirectory + "\\benchmark.png");
            dut.Close();
        }

        [Test]
        public void Uint64Test()
        {
            int x = 1;
            int z = 1;
            unchecked
            {
                var k = (UInt64)(
                    ((UInt64)(x) << 32) |
                    ((UInt64)(z) & 0xFFFFFFFF)
                );

                Console.WriteLine("{0:x8}", k);
                Assert.AreEqual(k, 0x100000001);
            }

        }


    }
}
