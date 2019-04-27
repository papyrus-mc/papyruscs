using System.Collections.Generic;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Maploader.Renderer.Texture
{
    public class TextureStack
    {
        public TextureStack(string filename, TextureTranslation translation, RotateFlipType rotation)
        {
            Infos.Add(new TextureInfo(filename, translation, rotation));
        }

        public TextureStack()
        {
        }

        public List<TextureInfo> Infos { get; } = new List<TextureInfo>();
       

        public static  implicit operator TextureStack(string filename)
        {
            return new TextureStack(filename, null, RotateFlipType.RotateNoneFlipNone);
        }
        public static TextureStack operator +(TextureStack a, TextureStack b)
        {
            return a.Add(b);
        }

        private TextureStack Add(TextureStack otherStack)
        {
            var stack = new TextureStack();
            stack.Infos.AddRange(this.Infos);
            stack.Infos.AddRange(otherStack.Infos);
            return stack;
        }

        public static implicit operator string(TextureStack info)
        {
            return info?.Infos.FirstOrDefault()?.Filename;
        }

        public TextureStack Rotate(RotateFlipType rotation)
        {
            if (!Infos.Any())
                return this;

            Infos.Last().Rotation = rotation;
            return this;
        }
        public TextureStack Translate(int x, int z, int w, int h)
        {
            return Translate(new Rectangle(x, z, w, h));
        }
        public TextureStack Translate(Rectangle source)
        {
            if (!Infos.Any())
                return this;

            Infos.Last().Translation = new TextureTranslation(source, source);
            return this;
        }
        public TextureStack Translate(Rectangle source, Rectangle dest)
        {
            if (!Infos.Any())
                return this;

            Infos.Last().Translation = new TextureTranslation(dest, source);
            return this;
        }
    }
}