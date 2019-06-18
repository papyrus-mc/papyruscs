using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using Maploader.Extensions;
using Maploader.Renderer.Heightmap;
using Maploader.Renderer.Imaging;
using Maploader.Renderer.Texture;
using Maploader.World;

namespace Maploader.Renderer
{
    public class ChunkRenderer<TImage> where TImage : class
    {
        private readonly TextureFinder<TImage> textureFinder;
        private readonly IGraphicsApi<TImage> graphics;
        private readonly RenderSettings renderSettings;

        public ChunkRenderer([NotNull] TextureFinder<TImage> textureFinder, IGraphicsApi<TImage> graphics, RenderSettings settings = null)
        {
            this.textureFinder = textureFinder ?? throw new ArgumentNullException(nameof(textureFinder));
            this.graphics = graphics;
            this.renderSettings = settings ?? new RenderSettings();

            b = new Brillouin(renderSettings.BrillouinJ, renderSettings.BrillouinDivider);
        }

        public List<string> MissingTextures { get; } = new List<string>();

        private Brillouin b;

        public void RenderChunk(TImage dest, Chunk c, int xOffset, int zOffset)
        {
            var xzColumns = c.Blocks.GroupBy(x => x.Value.XZ);
            var blocksOrderedByXZ = xzColumns.OrderBy(x => x.Key.GetLeByte(0)).ThenBy(x => x.Key.GetLeByte(1));
            var brightnessOffset = Math.Min(renderSettings.BrillouinOffset, renderSettings.YMax);
            if (brightnessOffset < 0)
                brightnessOffset = renderSettings.BrillouinOffset;


            foreach (var blocks in blocksOrderedByXZ)
            {
                var blocksToRender = new Stack<BlockCoord>();

                List<KeyValuePair<uint, BlockCoord>> blockColumns = blocks.OrderByDescending(x => x.Value.Y).ToList();
                if (renderSettings.YMax > 0)
                    blockColumns = blockColumns.Where(x => x.Value.Y <= renderSettings.YMax).ToList();

                if (renderSettings.TrimCeiling)
                {
                    int start = -1;
                    for (int i = 1; i < blockColumns.Count(); i++)
                    {
                        if (Math.Abs(blockColumns[i].Value.Y - blockColumns[i - 1].Value.Y) > 4)
                        {
                            start = i;
                            break;
                        }
                    }

                    if (start != -1)
                    {
                        blockColumns.RemoveRange(0, start);
                    }

                }


                foreach (var blockColumn in blockColumns) // Look for transparent blocks in single y column
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
                        Console.WriteLine($"\nMissing Texture(2): {block.ToString().PadRight(30)}");
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

                            if (renderSettings.RenderMode == RenderMode.Heightmap)
                            {
                                graphics.DrawImageWithBrightness(dest, bitmapTile, x, z, b.GetBrightness(block.Y - brightnessOffset));
                            }
                            else
                            {
                                graphics.DrawImageWithBrightness(dest, bitmapTile, x, z, 1);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"\nMissing Texture(1): {block.ToString().PadRight(30)} -- {texture.Filename}");
                            MissingTextures.Add($"ID: {block.Block.Id}, {texture.Filename}");
                        }
                    }
                }
            }

            if (renderSettings.RenderCoordinateStrings)
            {
                graphics.DrawString(dest, $"{c.X * 1}, {c.Z * 1}", new Font(FontFamily.GenericSansSerif, 10), Brushes.Black, xOffset, zOffset);
            }
        }
    }
}
