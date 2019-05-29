using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Maploader.Renderer;
using Maploader.Renderer.Texture;
using Maploader.World;
using PapyrusCs.Database;

namespace PapyrusCs.Strategies
{
    public interface IRenderStrategy
    {
        int XMin { get; set; }
        int XMax { get; set; }
        int ZMin { get; set; }
        int ZMax { get; set; }
        string OutputPath { get; set; }
        Dictionary<string, Texture> TextureDictionary { get; set; }
        string TexturePath { get; set; }
        int ChunkSize { get; set; }
        int ChunksPerDimension { get; set; }
        int TileSize { get; set; }
        World World { get; set; }
        int TotalChunkCount { get; set; }
        int InitialZoomLevel { get; set; }
        ConcurrentBag<string> MissingTextures { get; }
        List<Exception> Exceptions { get; }
        RenderSettings RenderSettings { get; set; }
        int InitialDiameter { get; set; }
        Func<PapyrusContext> DatabaseCreator { get; set; }
        HashSet<LevelDbWorldKey2> AllWorldKeys { get; set; }
        void RenderInitialLevel();
        void RenderZoomLevels();

        event EventHandler<ChunksRenderedEventArgs> ChunksRendered;
        event EventHandler<ZoomRenderedEventArgs> ZoomLevelRenderd;
    }

    class RendererCombi
    {
        public TextureFinder Finder { get; }
        public ChunkRenderer ChunkRenderer { get; }

        public RendererCombi(Dictionary<string, Texture> textureDictionary, string texturePath, RenderSettings renderSettings)
        {
            Finder = new TextureFinder(textureDictionary, texturePath);
            ChunkRenderer = new ChunkRenderer(Finder, renderSettings);
        }
    }

    class BitmapInfo
    {
        public Bitmap B { get; set; }
        public int X { get; set; }
        public int Z { get; set; }
    }
}