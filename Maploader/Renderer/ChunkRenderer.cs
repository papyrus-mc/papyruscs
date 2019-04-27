using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Maploader.Extensions;
using Maploader.Renderer.Texture;
using Maploader.World;
using PapyrusCs;

namespace Maploader.Renderer
{
    public class ChunkRenderer
    {
        private readonly TextureFinder textureFinder;
        private readonly RenderSettings renderSettings = new RenderSettings();

        public ChunkRenderer(TextureFinder textureFinder, RenderSettings renderSettings = null)
        {
            this.textureFinder = textureFinder;
            if (renderSettings != null)
            {
                this.renderSettings = renderSettings;
            }
        }

        public List<string> MissingTextures { get; } = new List<string>();

        public void RenderChunk(Chunk c, Graphics g, int xOffset, int zOffset)
        {
            var xzColumns = c.Blocks.GroupBy(x => x.Value.XZ);

            foreach (var blocks in xzColumns.OrderBy(x => x.Key.GetLeByte(0)).ThenBy(x => x.Key.GetLeByte(1)))
            {
                var blocksToRender = new Stack<BlockCoord>();

                foreach (var blockColumn in blocks.OrderByDescending(x => x.Value.Y)) // Look for transparent blocks
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
                    var textures = textureFinder.FindTexturePath(block.Block.Id, block.Block.Data, block.X, block.Z);
                    if (textures == null)
                    {
                        Console.WriteLine($"Missing(2): {block.ToString().PadRight(30)}");
                        continue;
                    }

                    foreach (var texture in textures.Infos)
                    {
                        var bitmapTile = textureFinder.GetTextureImage(texture);
                        if (bitmapTile != null)
                        {
                            g.DrawImage(bitmapTile, xOffset + block.X * 16, zOffset + block.Z * 16);
                        }
                        else
                        {
                            Console.WriteLine($"Missing(1): {block.ToString().PadRight(30)} -- {texture.Filename}");
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
    }
}
