using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Maploader.Extensions
{
    public static class GraphicsExtensions
    {
        public static void DrawImageBrightness(this Bitmap dest, Bitmap image, int x, int y, double brightness)
        {
            for (int ox = 0; ox < image.Width; ox++)
            {
                for (int oy = 0; oy < image.Height; oy++)
                {
                    var p = image.GetPixel(ox, oy);
                    dest.SetPixel(ox+x,oy+y, p);
                }

                // Make the result bitmap.
                //gr.DrawImage(image, points, rect, GraphicsUnit.Pixel, attributes);
                //gr.DrawImage(image, points, rect, GraphicsUnit.Pixel, attributes);
            }
        }

        public static Vector3 v255 = Vector3.One * 255;

        public static unsafe void DrawTest(this Bitmap dest, Bitmap image, int x, int y, float brightness)
        {
            int w = image.Width;
            if (w > 16) w = 16;
            int h = image.Height;
            if (h > 16) h = 16;

            BitmapData bDest = dest.LockBits(new Rectangle(x, y, w,h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bSrc = image.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            /*This time we convert the IntPtr to a ptr*/
            byte* scanDest = (byte*)bDest.Scan0.ToPointer();
            byte* scanSrc = (byte*)bSrc.Scan0.ToPointer();

            //Parallel.For(0, w, ox =>
            for (int ox = 0; ox < w; ox++)
            {
                for (int oy = 0; oy < h; oy++)
                {
                    byte* srcPixel = scanSrc + oy * bSrc.Stride + ox * 4;
                    byte* destPixel = scanDest + (oy) * bDest.Stride + (ox) * 4;

                    var aa = srcPixel[3] / 256f;
                    var ab = destPixel[3] / 256f;
                    var ac = aa + (1 - aa) * ab;
#if true
                    var a = new Vector3(srcPixel[0], srcPixel[1], srcPixel[2]);
                    var b = new Vector3(destPixel[0], destPixel[1], destPixel[2]);
                    var c = (brightness * a * aa + ((1 - aa) * ab) * b)/ac;

                    c = Vector3.Clamp(c, Vector3.Zero, v255);
                    
                    destPixel[0] = (byte) c.X;
                    destPixel[1] = (byte) c.Y;
                    destPixel[2] = (byte) c.Z;
#else
                    var dr = (brightness * srcPixel[0] * aa + (1 - aa) * ab * destPixel[0]);
                    var dg = (brightness * srcPixel[1] * aa + (1 - aa) * ab * destPixel[1]);
                    var db = (brightness * srcPixel[2] * aa + (1 - aa) * ab * destPixel[2]);
                    
                    if (dr > 255) dr = 255;
                    if (dg > 255) dg = 255;
                    if (db > 255) db = 255;

                    destPixel[0] = (byte)dr;
                    destPixel[1] = (byte)dg;
                    destPixel[2] = (byte)db;
#endif
                    destPixel[3] = (byte) (ac * 255);
                }
            }

            image.UnlockBits(bSrc);
            dest.UnlockBits(bDest);
        }
    }
}
