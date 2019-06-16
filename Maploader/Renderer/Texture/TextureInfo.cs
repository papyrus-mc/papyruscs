using System;
using System.Drawing;
using Maploader.Renderer.Imaging;

namespace Maploader.Renderer.Texture
{
    public class TextureInfo : IEquatable<TextureInfo>
    {
        #region equals
        public bool Equals(TextureInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Filename, other.Filename) && Equals(Translation, other.Translation) && Rotation == other.Rotation;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextureInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Filename != null ? Filename.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Translation != null ? Translation.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Rotation;
                return hashCode;
            }
        }

        public static bool operator ==(TextureInfo left, TextureInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TextureInfo left, TextureInfo right)
        {
            return !Equals(left, right);
        }

     
        #endregion
        public TextureInfo(string filename, TextureTranslation translation, RotateFlip rotation)
        {
            Filename = filename;
            Translation = translation;
            Rotation = rotation;
        }

        public string Filename { get;}
        public TextureTranslation Translation { get; set; }
        public RotateFlip Rotation { get; set; }


    }
}