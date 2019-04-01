namespace Maploader.Renderer.Texture
{
    /// <summary>
    /// Contains the information of the "terrain_texture.json" file from the vanilla texture pack
    /// </summary>
    public class SubTexture
    {
        public string Path { get; }
        public string OverlayColor { get; }

        public bool HasOverlayColor { get; }

        public SubTexture(string path, string overlayColor = null)
        {
            Path = path;
            HasOverlayColor = overlayColor != null;
            OverlayColor = overlayColor;
        }

        public override string ToString()
        {
            return $"{Path}";
        }
    }
}