using System.Collections.Generic;

namespace Maploader.Renderer.Texture
{
    /// <summary>
    /// Contains the information of the "terrain_texture.json" file from the vanilla texture pack
    /// </summary>
    public class Texture
    {
        public Texture(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public List<SubTexture> Subtextures { get; } = new List<SubTexture>();

        public void AddSubTexture(string value, string overlayColor = null, string tintColor = null)
        {
            Subtextures.Add(new SubTexture(value, overlayColor));
        }

        public override string ToString()
        {
            return $"{Name} - {Subtextures.Count} Subtextures";
        }
    }
}