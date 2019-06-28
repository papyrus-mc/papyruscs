using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using DmitryBrant.ImageFormats;

namespace Maploader.Renderer.Imaging
{
    public class LoadImageException : Exception
    {
        private readonly string message; 
        public LoadImageException(string path, Exception inner) : base()
        {
            message = $"Unable to load {path}, because {inner}";
        }

        public override string ToString()
        {
            return message;
        }
    }
    public class SystemDrawing : IGraphicsApi<Bitmap>
    {
        public Bitmap LoadImage(string path)
        {
            try
            {
                if (path.EndsWith("webp"))
                {
                    return WebP.Value.LoadImage(path);
                }

                if (path.EndsWith("tga"))
                {
                    return TgaReader.Load(path);
                }
                else
                {
                    return (Bitmap) Image.FromFile(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading {path}, because {ex}");
                throw new LoadImageException(path, ex);
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

        public int DefaultQuality { get; set; }

        public void RotateFlip(Bitmap image, RotateFlip infoRotation)
        {
            image.RotateFlip((RotateFlipType)infoRotation);
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

        private Lazy<WebP> WebP { get; } = new Lazy<WebP>(() => new WebP());

        public void SaveImage(Bitmap image, string filepath)
        {
            if (filepath.EndsWith("none"))
            {

            } else if (filepath.EndsWith("webp"))
            {
                WebP.Value.Save(image, filepath, DefaultQuality);
            }
            else if (filepath.EndsWith("jpg") && (DefaultQuality != -1))
            {
                var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                var encP = new EncoderParameters() { Param = new[] { new EncoderParameter(Encoder.Quality, DefaultQuality) } };
                image.Save(filepath, encoder, encP);
            }
            else
            {
                image.Save(filepath);
            }
        }
    }
}