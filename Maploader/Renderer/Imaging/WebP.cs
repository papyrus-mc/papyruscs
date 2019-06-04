using System.Drawing;
using System.IO;
using Imazen.WebP;

namespace Maploader.Renderer.Imaging
{
    public class WebP
    {
        SimpleEncoder e;
        SimpleDecoder d;
        public WebP()
        {
            Imazen.WebP.Extern.LoadLibrary.LoadWebPOrFail();
            e = new SimpleEncoder();
            d = new SimpleDecoder();
        }

        public void Save(Bitmap b, string filename, int defaultQuality)
        {
            using (var f = File.Create(filename))
            {
                e.Encode(b, f, defaultQuality);
            }
        }

        public Bitmap LoadImage(string filename)
        {
            var f = File.ReadAllBytes(filename);
            {
                return d.DecodeFromBytes(f, f.Length);
            }
        }
    }
}