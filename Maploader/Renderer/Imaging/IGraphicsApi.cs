using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Text;
using DmitryBrant.ImageFormats;
using JetBrains.Annotations;
// ReSharper disable InconsistentNaming

namespace Maploader.Renderer.Imaging
{
    public struct Rect {
        public Rect(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }

    }
    public enum RotateFlip
    {
        Rotate180FlipXY = 0,
        RotateNoneFlipNone = 0,
        Rotate270FlipXY = 1,
        Rotate90FlipNone = 1,
        Rotate180FlipNone = 2,
        RotateNoneFlipXY = 2,
        Rotate270FlipNone = 3,
        Rotate90FlipXY = 3,
        Rotate180FlipY = 4,
        RotateNoneFlipX = 4,
        Rotate270FlipY = 5,
        Rotate90FlipX = 5,
        Rotate180FlipX = 6,
        RotateNoneFlipY = 6,
        Rotate270FlipX = 7,
        Rotate90FlipY = 7,
    }

    public interface IGraphicsApi<TImage>
    {
        TImage LoadImage(string path);
        TImage CreateEmptyImage(int with, int height);
        void RotateFlip(TImage image, RotateFlip infoRotation);
        void SaveImage(TImage image, string filepath);
        void DrawImageWithBrightness(TImage dest, TImage image, int x, int y, float brightness);
        void DrawImage(TImage dest, TImage src, Rect translationDest, Rect translationSource);
        void DrawImage(TImage dest, TImage src, int x, int y, int w, int h);
        void DrawString(TImage dest, string str, Font font, Brush brush, int x, int y);
    }

    public class SystemDrawing : IGraphicsApi<Bitmap>
    {
        public Bitmap LoadImage(string path)
        {
            if (path.EndsWith("tga"))
            {
                return TgaReader.Load(path);
            }
            else
            {
                return (Bitmap) Image.FromFile(path);
            }
        }

        public Bitmap CreateEmptyImage(int with, int height)
        {
            return new Bitmap(with, height);
        }

        public void DrawImage(Bitmap dest, Bitmap src, Rect translationDest, Rect translationSource)
        {
            using (var g = Graphics.FromImage(dest))
            {
                g.DrawImage(src, 
                    new Rectangle(translationDest.X, translationDest.Y, translationDest.W, translationDest.H),
                    new Rectangle(translationSource.X, translationSource.Y, translationSource.W, translationSource.H), GraphicsUnit.Pixel);
            }
        }

        public void DrawImage(Bitmap dest, Bitmap src, int x, int y, int w, int h)
        {
            using (var g = Graphics.FromImage(dest))
            {
                g.DrawImage(src, x, y, w, h);
            }
        }

        public void DrawString(Bitmap dest, string str, Font font, Brush brush, int x, int y)
        {
            using (var g = Graphics.FromImage(dest))
            {
                g.DrawString(str, font, brush, x, y);
            }
        }

        public void RotateFlip(Bitmap image, RotateFlip infoRotation)
        {
            image.RotateFlip((RotateFlipType) infoRotation);
        }

        private static Vector3 v255 = Vector3.One * 255;

        public unsafe void DrawImageWithBrightness(Bitmap dest, Bitmap image, int x, int y, float brightness)
        {
            int w = image.Width;
            if (w > 16) w = 16;
            int h = image.Height;
            if (h > 16) h = 16;

            BitmapData bDest = dest.LockBits(new Rectangle(x, y, w, h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
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

                    var vSrc = new Vector3(srcPixel[0], srcPixel[1], srcPixel[2]);
                    var vDest = new Vector3(destPixel[0], destPixel[1], destPixel[2]);

                    var vComb = (brightness * vSrc * aa + ((1 - aa) * ab) * vDest) / ac;

                    vComb = Vector3.Clamp(vComb, Vector3.Zero, v255);
                    destPixel[0] = (byte)vComb.X;
                    destPixel[1] = (byte)vComb.Y;
                    destPixel[2] = (byte)vComb.Z;
                    destPixel[3] = (byte)(ac * 255);
                }
            }

            image.UnlockBits(bSrc);
            dest.UnlockBits(bDest);
        }

        public void SaveImage(Bitmap image, string filepath) 
        {
            image.Save(filepath);
        }
    }
}
