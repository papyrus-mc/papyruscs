using System;

namespace PapyrusCs.Strategies
{
    public class ImageInfo<TImage> where TImage : class
    {
        public TImage Image { get; set; }
        public int X { get; set; }
        public int Z { get; set; }

        public void Dispose()
        {
            if (Image is IDisposable disp)
            {
                disp.Dispose();
            }
        }
    }
}