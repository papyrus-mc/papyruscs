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
                    }
                    else
                    {
                        Console.WriteLine($"{block.ToString().PadRight(30)} {texturePath}");
                    }
                }
            }
        }

        private Bitmap RotatePic(Bitmap bmpBU, float w, bool keepWholeImg)
        {
            Bitmap bmp = null;
            Graphics g = null;
            try
            {
                //Modus
                if (!keepWholeImg)
                {
                    bmp = new Bitmap(bmpBU.Width, bmpBU.Height);

                    g = Graphics.FromImage(bmp);
                    float hw = bmp.Width / 2f;
                    float hh = bmp.Height / 2f;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    //translate center
                    g.TranslateTransform(hw, hh);
                    //rotate
                    g.RotateTransform(w);
                    //re-translate
                    g.TranslateTransform(-hw, -hh);
                    g.DrawImage(bmpBU, 0, 0);
                    g.Dispose();
                }
                else
                {
                    //get the new size and create the blank bitmap
                    float rad = (float)(w / 180.0 * Math.PI);
                    double fW = Math.Abs((Math.Cos(rad) * bmpBU.Width)) + Math.Abs((Math.Sin(rad) * bmpBU.Height));
                    double fH = Math.Abs((Math.Sin(rad) * bmpBU.Width)) + Math.Abs((Math.Cos(rad) * bmpBU.Height));

                    bmp = new Bitmap((int)Math.Ceiling(fW), (int)Math.Ceiling(fH));

                    g = Graphics.FromImage(bmp);

                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    float hw = bmp.Width / 2f;
                    float hh = bmp.Height / 2f;

                    System.Drawing.Drawing2D.Matrix m = g.Transform;

                    //here we do not need to translate, we rotate at the specified point
                    m.RotateAt(w, new PointF((float)(bmp.Width / 2), (float)(bmp.Height / 2)), System.Drawing.Drawing2D.MatrixOrder.Append);

                    g.Transform = m;

                    //draw the rotated image
                    g.DrawImage(bmpBU, new PointF((float)((bmp.Width - bmpBU.Width) / 2), (float)((bmp.Height - bmpBU.Height) / 2)));
                    g.Dispose();
                }
            }
            catch
            {
                if ((bmp != null))
                {
                    bmp.Dispose();
                    bmp = null;
                }

                if ((g != null))
                {
                    g.Dispose();
                }
            }

            return bmp;
        }
    }
}
