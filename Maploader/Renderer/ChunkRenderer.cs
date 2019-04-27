using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Maploader.Extensions;
using Maploader.Renderer.Texture;
using Maploader.World;

namespace Maploader.Renderer
{
    public class ChunkRenderer
    {
        private readonly TextureFinder textureFinder;

        public ChunkRenderer(TextureFinder textureFinder)
        {
            this.textureFinder = textureFinder;
        }

        public List<string> MissingTextures { get; } = new List<string>();

        public void RenderChunk(Chunk c, Graphics g, int xOffset, int zOffset)
        {
            var xzColumns = c.Blocks.GroupBy(x => x.Value.XZ);

            List<Action> textsToAdd = new List<Action>();
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
                        Console.WriteLine($"Missing2: {block.ToString().PadRight(30)}");
                        continue;
                    }

                    foreach (var texture in textures.Infos)
                    {
                        var bitmapTile = textureFinder.GetTextureImage(texture);
                        if (bitmapTile != null)
                        {
                            g.DrawImage(bitmapTile, xOffset + block.X * 16, zOffset + block.Z * 16);
                            //Console.WriteLine($"OK      : {block.ToString().PadRight(30)} -- {texture.Filename}");
                        }
                        else
                        {
                            Console.WriteLine($"Missing1: {block.ToString().PadRight(30)} -- {texture.Filename}");
                            MissingTextures.Add($"ID: {block.Block.Id}, {texture.Filename}");
                        }
                    }
                }
            }
            g.DrawString($"{c.X * 16}, {c.Z * 16}", new Font(FontFamily.GenericSansSerif, 10), Brushes.Black, 0, 0);
            foreach (var drawText in textsToAdd)
            {
                drawText();
            }
        }
    }
}
