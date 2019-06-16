using System.Collections.Generic;
using Maploader.Renderer;
using Maploader.Renderer.Imaging;
using Maploader.Renderer.Texture;

namespace PapyrusCs.Strategies
{
    class RendererCombi<TImage> where TImage : class
    {
        public TextureFinder<TImage> Finder { get; }
        public ChunkRenderer<TImage> ChunkRenderer { get; }

        public RendererCombi(Dictionary<string, Texture> textureDictionary, string texturePath, RenderSettings renderSettings, IGraphicsApi<TImage> graphics)
        {
            Finder = new TextureFinder<TImage>(textureDictionary, texturePath, graphics);
            ChunkRenderer = new ChunkRenderer<TImage>(Finder, graphics, renderSettings);
        }
    }
}