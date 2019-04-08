using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

        public void RenderChunk(Chunk c, Graphics g, int xOffset, int zOffset)
        {
            var xzColumns = c.Blocks.GroupBy(x => x.Value.XZ);

            List<Action> DrawTodos = new List<Action>();
            foreach (var blocks in xzColumns)
            {
                var blocksToRender = new Stack<BlockCoord>();

                foreach (var blockPair in blocks.OrderByDescending(x => x.Value.Y)) // Look for transparent blocks
                {
                    var block = blockPair.Value;
                    //var name = block.Block.Id;
                    //if (name != "minecraft:grass" && name != "minecraft:sand" && name != "minecraft:water")
                    //{
                    //    Console.WriteLine(block.Block.Id);
                    //}
                    blocksToRender.Push(block);
                    if (!textureFinder.TransparentBlocks.ContainsKey(block.Block.Id)) 
                    {
                        break;
                    }
                }

                foreach (var block in blocksToRender)
                {
                    string texturePath = textureFinder.FindTexturePath(block.Block.Id, block.Block.Data);

                    var tile = textureFinder.GetTextureImage(texturePath);
                    if (tile != null)
                    {
                        g.DrawImage(tile, xOffset + block.X * 16, zOffset + block.Z * 16);
                        //if (block.Block.Id == "minecraft:coral_block")
                        //{
                        //    DrawTodos.Add(() => g.DrawString($"{c.X * 16+block.X} {c.Z * 16+block.Z}", new Font(FontFamily.GenericSansSerif, 10), Brushes.Black, xOffset + block.X * 16, zOffset + block.Z * 16));
                        //}
                    }
                    else
                    {
                        Console.WriteLine($"{block.ToString().PadRight(30)} {texturePath}");
                    }
                }
            }
            g.DrawString($"{c.X * 16}, {c.Z * 16}", new Font(FontFamily.GenericSansSerif, 10), Brushes.Black, 0, 20);
            foreach (var toto in DrawTodos)
            {
                toto();
            }
        }
    }
}
