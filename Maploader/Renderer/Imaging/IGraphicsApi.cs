using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using JetBrains.Annotations;
// ReSharper disable InconsistentNaming

namespace Maploader.Renderer.Imaging
{
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
        int DefaultQuality { get; set; }
    }
}
