using System;
using System.Drawing;
using Maploader.Renderer.Imaging;

namespace Maploader.Renderer.Texture
{
    public class TextureTranslation : IEquatable<TextureTranslation>
    {
        #region
        public bool Equals(TextureTranslation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Dest.Equals(other.Dest) && Source.Equals(other.Source);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextureTranslation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Dest.GetHashCode() * 397) ^ Source.GetHashCode();
            }
        }

        public static bool operator ==(TextureTranslation left, TextureTranslation right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TextureTranslation left, TextureTranslation right)
        {
            return !Equals(left, right);
        }

        public TextureTranslation(Rect dest, Rect source)
        {
            Dest = dest;
            Source = source;
        }
        #endregion

        public Rect Dest { get; }
        public Rect Source { get; }
    }
}