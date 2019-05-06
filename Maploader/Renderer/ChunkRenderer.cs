using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using JetBrains.Annotations;
using Maploader.Extensions;
using Maploader.Renderer.Heightmap;
using Maploader.Renderer.Texture;
using Maploader.World;

namespace Maploader.Renderer
{
    public class ChunkRenderer
    {
        private readonly TextureFinder textureFinder;
        private readonly RenderSettings renderSettings;

        public ChunkRenderer([NotNull] TextureFinder textureFinder, RenderSettings settings = null)
        {
            this.textureFinder = textureFinder ?? throw new ArgumentNullException(nameof(textureFinder));
            this.renderSettings = settings ?? new RenderSettings();

            b = new Brillouin(renderSettings.BrillouinJ, renderSettings.BrillouinDivider);
        }

        public List<string> MissingTextures { get; } = new List<string>();

        private Brillouin b;

        public void RenderChunk(Bitmap dest, Chunk c, Graphics g, int xOffset, int zOffset)
        {
            var xzColumns = c.Blocks.GroupBy(x => x.Value.XZ);
            var blocksOrderedByXZ = xzColumns.OrderBy(x => x.Key.GetLeByte(0)).ThenBy(x => x.Key.GetLeByte(1));
            var brightnessOffset = Math.Min(64, renderSettings.YMax);


            foreach (var blocks in blocksOrderedByXZ)
            {
                var blocksToRender = new Stack<BlockCoord>();

                IEnumerable<KeyValuePair<uint, BlockCoord>> blockColumns = blocks.OrderByDescending(x => x.Value.Y);
                if (renderSettings.YMax > 0)
                    blockColumns = blockColumns.Where(x => x.Value.Y <= renderSettings.YMax);

                foreach (var blockColumn in blockColumns) // Look for transparent blocks
                {
                    var block = blockColumn.Value;

                    blocksToRender.Push(block);
                    if (!textureFinder.TransparentBlocks.ContainsKey(block.Block.Id))
                    {
                        break;
                    }
                }

                foreach (var block in blocksToRender)
                {
                    var textures =
                        textureFinder.FindTexturePath(block.Block.Id, block.Block.Data, block.X, block.Z, block.Y);
                    if (textures == null)
                    {
                        Console.WriteLine($"Missing Texture(2): {block.ToString().PadRight(30)}");
                        MissingTextures.Add($"ID: {block.Block.Id}");
                        continue;
                    }

                    foreach (var texture in textures.Infos)
                    {
                        var bitmapTile = textureFinder.GetTextureImage(texture);
                        if (bitmapTile != null)
                        {
                            var x = xOffset + block.X * 16;
                            var z = zOffset + block.Z * 16;

                            switch (renderSettings.RenderMode)
                            {
                                case RenderMode.Heightmap:
                                    dest.DrawImageWithBrightness(bitmapTile, x, z, b.GetBrightness(block.Y - brightnessOffset));
                                    break;
                                default:
                                    dest.DrawImageWithBrightness(bitmapTile, x, z, 1);
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Missing Texture(1): {block.ToString().PadRight(30)} -- {texture.Filename}");
                            MissingTextures.Add($"ID: {block.Block.Id}, {texture.Filename}");
                        }
                    }
                }
            }

            if (renderSettings.RenderCoords)
            {
                g.DrawString($"{c.X * 16}, {c.Z * 16}", new Font(FontFamily.GenericSansSerif, 10), Brushes.Black,
                    xOffset, zOffset);
            }
        }

        public static bool IsOpaque(Image image)
        {
            var bitmap = new Bitmap(image);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            unsafe
            {
                var p = (byte*) bitmapData.Scan0;
                for (var x = 0; x < bitmap.Width; x++)
                {
                    for (var y = 0; y < bitmap.Height; y++)
                    {
                        if (p[x * 4 + y * bitmapData.Stride + 3] == 255) continue;
                        bitmap.UnlockBits(bitmapData);
                        return false;
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);
            return true;
        }


        private void AdjustBrightness(Graphics gr, Image image, float brightness)
        {
            // Make the ColorMatrix.
            float b = brightness;
            ColorMatrix cm = new ColorMatrix(new float[][]
            {
                new float[] {b, 0, 0, 0, 0},
                new float[] {0, b, 0, 0, 0},
                new float[] {0, 0, b, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1},
            });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(cm);

            // Draw the image onto the new bitmap while applying
            // the new ColorMatrix.
            Point[] points =
            {
                new Point(0, 0),
                new Point(image.Width, 0),
                new Point(0, image.Height),
            };
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

            // Make the result bitmap.
            gr.DrawImage(image, points, rect,
                GraphicsUnit.Pixel, attributes);
        }
    }
}
