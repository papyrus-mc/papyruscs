using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Maploader.Renderer;
using Maploader.Renderer.Heightmap;
using Maploader.Renderer.Imaging;
using Maploader.Renderer.Texture;
using NUnit.Framework;
using PapyrusCs.Database;

namespace MapLoader.NUnitTests
{
    [TestFixture]
    public class OtherTests
    {
        [TestCase(0, 0, 1)]
        [TestCase(1, 1, 1)]
        [TestCase(2, 2, 1)]
        [TestCase(3, 3, 1)]

        [TestCase(0, 0, 2)]
        [TestCase(0, 1, 2)]
        [TestCase(1, 2, 2)]
        [TestCase(1, 3, 2)]
        [TestCase(2, 4, 2)]
        [TestCase(2, 5, 2)]

        [TestCase(0, 0, 3)]
        [TestCase(0, 1, 3)]
        [TestCase(0, 2, 3)]
        [TestCase(1, 3, 3)]
        [TestCase(1, 4, 3)]
        [TestCase(1, 5, 3)]
        [TestCase(2, 6, 3)]
        [TestCase(2, 7, 3)]
        [TestCase(2, 8, 3)]

        [TestCase(-1, -1, 1)]
        [TestCase(-2, -2, 1)]
        [TestCase(-3, -3, 1)]
        [TestCase(-4, -4, 1)]

        [TestCase(-1, -1, 2)]
        [TestCase(-1, -2, 2)]
        [TestCase(-2, -3, 2)]
        [TestCase(-2, -4, 2)]
        [TestCase(-3, -5, 2)]
        [TestCase(-3, -6, 2)]

        [TestCase(-1, -1, 3)]
        [TestCase(-1, -2, 3)]
        [TestCase(-1, -3, 3)]
        [TestCase(-2, -4, 3)]
        [TestCase(-2, -5, 3)]
        [TestCase(-2, -6, 3)]
        [TestCase(-3, -7, 3)]
        [TestCase(-3, -8, 3)]
        [TestCase(-3, -9, 3)]
        public void CombineChunks(int result, int x, int chunkPerDimension)
        {
            var group = GetGroup(x, chunkPerDimension);
            Assert.That(group, Is.EqualTo(result));
        }

        private int GetGroup(int coord, int chunkPerDimension)
        {
            if (coord >= 0)
                return coord / chunkPerDimension;
            return ((coord + 1) / chunkPerDimension) - 1;
        }

        [Test]
        public void ChunkKeyTest()
        {
            var key = new LevelDbWorldKey2(new byte[]{2,0,0,0,2,0,0,0,47,0});
            Assert.That(key.X, Is.EqualTo(2));
            Assert.That(key.Z, Is.EqualTo(2));

            var group = key.GetXZGroup(2);
            Assert.That(group, Is.EqualTo(((UInt64)1<<32) + 1));

        }

    }

    [TestFixture]
    class BenchmarkTests
    {
        [Test]
        [Ignore("debugging")]
        public void Open()
        {
            Console.WriteLine("hello world");
            var dut = new Maploader.World.World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "benchmark", "world", "db"));

           
            foreach (var d in dut.Keys.Where(x => x.Length >= 9 & x[8] == 45 ))
            {
                //Console.WriteLine(string.Join(" ", d.Select(e => $"{e:d3}")));
                //Console.WriteLine(dut.GetData(d).Length);
            }

            //dut.Close();

            Assert.Pass();
        }

        [Test]
        public void HashCodeByteArray()
        {
            var a = new byte[] {1, 2, 3, 4};
            var b = new byte[] {1, 2, 3, 4};

            Console.WriteLine(a.GetHashCode());
            Console.WriteLine(b.GetHashCode());
        }

        [Test]
        [Ignore("debugging")]
        public void TestRender()
        {
            var dut = new Maploader.World.World();
            dut.Open(@"C:\papyruscs\homeworld\db");
            int chunkRadius = 2;
            int centerOffsetX = 12; //65;
            int centerOffsetZ = -55; //65;
            string filename = "testrender.png";

            RenderMap(chunkRadius, dut, centerOffsetX, centerOffsetZ, filename);
        }

        [Test]
        public void BenchmarkRender()
        {
            var dut = new Maploader.World.World();
            dut.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "benchmark", "world", "db"));
            int chunkRadius = 1;
            int centerOffsetX = 1; //65;
            int centerOffsetZ = 1; //65;
            string filename = "benchmark.png";

            RenderMap(chunkRadius, dut, centerOffsetX, centerOffsetZ, filename);
        }

        private static void RenderMap(int chunkRadius, Maploader.World.World dut, int centerOffsetX, int centerOffsetZ, string filename)
        {
            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"textures",
                "terrain_texture.json"));
            var ts = new TerrainTextureJsonParser(json, "");
            var textures = ts.Textures;
            var g = new SystemDrawing();
            var finder = new TextureFinder<Bitmap>(textures,  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "textures"), g);
            finder.Debug = false;

            var b = g.CreateEmptyImage(16 * 16 * (2 * chunkRadius + 1), 16 * 16 * (2 * chunkRadius + 1));

            var render = new ChunkRenderer<Bitmap>(finder, g, new RenderSettings(){ YMax = 40});

            //Parallel.For(-chunkRadius, chunkRadius + 1,new ParallelOptions(){MaxDegreeOfParallelism = 8} , dx =>
            for (int dz = -chunkRadius; dz <= chunkRadius; dz++)
            {
                for (int dx = -chunkRadius; dx <= chunkRadius; dx++)
                {
                    var c = dut.GetChunk(dx + centerOffsetX, dz + centerOffsetZ);
                    if (c != null)
                    {
                        render.RenderChunk(b, c, (chunkRadius + dx) * 256, (chunkRadius + dz) * 256);
                    }
                }
            }

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            b.Save(path);
            Console.WriteLine(path);
            dut.Close();
        }

        [Test]
        public void Uint64Test()
        {
            int x = 1;
            int z = 1;
            unchecked
            {
                var k = (UInt64) (
                    ((UInt64) (x) << 32) |
                    ((UInt64) (z) & 0xFFFFFFFF)
                );

                Console.WriteLine("{0:x8}", k);
                Assert.AreEqual(k, 0x100000001);
            }

        }

        [Test]
        public void BrillouinFkt()
        {
            var dut = new Brillouin(10000);

        }
    }
}
