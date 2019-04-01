using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Maploader.Renderer;
using Maploader.Renderer.Texture;
using Maploader.World;

namespace MapCreatorCore
{
    class Program
    {
        static void Main(string[] args)
        {

            var world = new World();
            world.Open(@"C:\Users\deepblue1\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\RhIAAFEzQQA=\db");


            var centerX = 0;
            var centerZ = 0;
            var radius = 128;

            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Textures\terrain_texture.json"));
            var ts = new TerrainTextureJsonParser(json, "");
            var textures = ts.Textures;
           

            var zoom = Math.Ceiling(Math.Log(2 * radius) / Math.Log(2));

            List<Exception> exes = new List<Exception>();

            Parallel.For(-radius + centerX, radius + centerX, new ParallelOptions() {MaxDegreeOfParallelism = 4}, x =>
            {
                try
                {
                    var finder = new TextureFinder(textures, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures"));
                    var chunkRenderer = new ChunkRenderer(finder);

                    Console.WriteLine($"Processing {x}");
                    for (int z = -radius + centerZ; z < radius + centerZ; z++)
                    {
                        var chunk = world.GetChunk(x, z);

                        using (var b = new Bitmap(256, 256))
                        using (var g = Graphics.FromImage(b))
                        {
                            chunkRenderer.RenderChunk(chunk, g, 0, 0);

                            var fx = x + radius - centerX;
                            var fz = z + radius + centerZ;

                            var path = $"map\\{zoom}\\{fx}";
                            var filepath = Path.Combine(path, $"{fz}.png");

                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            b.Save(filepath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exes.Add(ex);
                }
            });

            while (zoom >= 0)
            {
                //for (int x = 0; x < 2 * radius; x += 2)
                var radiusLocal = radius;
                var zoomLocal = zoom;
                Parallel.For(0, radius, hx =>
                {
                    var x = 2 * hx;
                    Console.WriteLine($"Processing {x} at {zoomLocal-1}");
                    for (int z = 0; z < 2 * radiusLocal; z += 2)
                    {

                        var b1 = LoadBitmap(zoomLocal, x, z);
                        var b2 = LoadBitmap(zoomLocal, x + 1, z);
                        var b3 = LoadBitmap(zoomLocal, x, z + 1);
                        var b4 = LoadBitmap(zoomLocal, x + 1, z + 1);

                        using (var bfinal = new Bitmap(256, 256))
                        using (var gfinal = Graphics.FromImage(bfinal))
                        {
                            if (b1 != null)
                                gfinal.DrawImage(b1, 0, 0, 128, 128);
                            if (b2 != null)
                                gfinal.DrawImage(b2, 128, 0, 128, 128);
                            if (b3 != null)
                                gfinal.DrawImage(b3, 0, 128, 128, 128);
                            if (b4 != null)
                                gfinal.DrawImage(b4, 128, 128, 128, 128);

                            var path = $"map\\{zoomLocal - 1}\\{x / 2}";
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            var filepath = Path.Combine(path, $"{z / 2}.png");
                            bfinal.Save(filepath);
                        }
                    }
                });

                radius /= 2;
                zoom--;
            }



            world.Close();
        }

        private static Bitmap LoadBitmap(double zoom, int x, int z)
        {
            var path = $"map\\{zoom}\\{x}";
            var filepath = Path.Combine(path, $"{z}.png");
            if (File.Exists(filepath))
            {
                return new Bitmap(filepath);
            }

            return null;
        }
    }
}
