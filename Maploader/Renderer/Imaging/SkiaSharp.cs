using System;
using System.Drawing;
using Maploader.Core;
using SkiaSharp;

namespace Maploader.Renderer.Imaging
{
    public class SkiaSharp : IGraphicsApi<SKBitmap>
    {
        private int height = -1;
        private int width = -1;
        private GenericPool<SKBitmap> pool;

        public SKBitmap LoadImage(string path)
        {
            return SKBitmap.Decode(path);
        }

        public SKBitmap CreateEmptyImage(int width, int height)
        {
            return new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        }

        public void RotateFlip(SKBitmap image, RotateFlip infoRotation)
        {
            switch (infoRotation)
            {
                case Imaging.RotateFlip.Rotate180FlipNone:
                    Rotate(image, 180);
                    break;
                case Imaging.RotateFlip.Rotate270FlipNone:
                    Rotate(image, 270);
                    break;
                case Imaging.RotateFlip.Rotate90FlipNone:
                    Rotate(image, 90);
                    break;
            }
        }

        public void SaveImage(SKBitmap image, string filepath)
        {
            SKEncodedImageFormat format;
            if (filepath.EndsWith("png"))
                format = SKEncodedImageFormat.Png;
            else if (filepath.EndsWith("jpg"))
                format = SKEncodedImageFormat.Jpeg;
            else //else if (filepath.EndsWith("webp"))
                throw new InvalidOperationException("webp not supported");

            SKPixmap.Encode(new SKFileWStream(filepath), image, format, DefaultQuality);
        }

        public void DrawImageWithBrightness(SKBitmap dest, SKBitmap image, int x, int y, float brightness)
        {
            throw new NotImplementedException();
        }

        public void DrawImage(SKBitmap dest, SKBitmap src, Rect translationDest, Rect ts)
        {
            using (var surface = new SKCanvas(dest))
            {
                surface.DrawBitmap(src, new SKRect(ts.X, ts.Y, ts.X + ts.W, ts.Y + ts.H), new SKRect(ts.X, ts.Y, ts.X + ts.W, ts.Y + ts.H));
            }
        }

        public void DrawImage(SKBitmap dest, SKBitmap src, int x, int y, int w, int h)
        {
            using (var surface = new SKCanvas(dest))
            {
                surface.DrawBitmap(src, new SKRect(x, y, x + w, y + h));
            }
        }

        public void DrawString(SKBitmap dest, string str, Font font, Brush brush, int x, int y)
        {
            //
        }

        public int DefaultQuality { get; set; }

        public void SetPoolDimensions(int pwidth, int pheight)
        {
            width = pwidth;
            height = pheight;
            this.pool = new GenericPool<SKBitmap>(() => new SKBitmap(pwidth, pheight));
        }

        //private static SKBitmap Rotate(SKBitmap bitmap, double angle)
        //{
        //    double radians = Math.PI * angle / 180;
        //    float sine = (float)Math.Abs(Math.Sin(radians));
        //    float cosine = (float)Math.Abs(Math.Cos(radians));
        //    int originalWidth = bitmap.Width;
        //    int originalHeight = bitmap.Height;
        //    int rotatedWidth = (int)(cosine * originalWidth + sine * originalHeight);
        //    int rotatedHeight = (int)(cosine * originalHeight + sine * originalWidth);

        //    var rotatedBitmap = new SKBitmap(rotatedWidth, rotatedHeight);

        //    using (var surface = new SKCanvas(rotatedBitmap))
        //    {
        //        surface.Translate(rotatedWidth / 2, rotatedHeight / 2);
        //        surface.RotateDegrees((float)angle);
        //        surface.Translate(-originalWidth / 2, -originalHeight / 2);
        //        surface.DrawBitmap(bitmap, new SKPoint());
        //    }
        //    return rotatedBitmap;
        //}
        private static void Rotate(SKBitmap bitmap, double angle)
        {
            var radians = Math.PI * angle / 180;
            var sine = (float) Math.Abs(Math.Sin(radians));
            var cosine = (float) Math.Abs(Math.Cos(radians));

            var originalWidth = bitmap.Width;
            var originalHeight = bitmap.Height;

            var rotatedWidth = (int) (cosine * originalWidth + sine * originalHeight);
            var rotatedHeight = (int) (cosine * originalHeight + sine * originalWidth);

            using (var surface = new SKCanvas(bitmap))
            {
                surface.Translate(rotatedWidth / 2, rotatedHeight / 2);
                surface.RotateDegrees((float) angle);
                surface.Translate(-originalWidth / 2, -originalHeight / 2);
                surface.DrawBitmap(bitmap, new SKPoint());
            }
        }

        public SKBitmap GetPooledImage()
        {
            return pool.Get();
        }

  

        public void ReturnImage(SKBitmap b)
        {
            using (var surface = new SKCanvas(b))
            {
                surface.Clear(SKColor.Empty);
            }

            this.pool?.Return(b);
        }
    }
}