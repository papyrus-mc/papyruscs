using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Maploader.Renderer;
using Maploader.Renderer.Texture;
using Maploader.World;

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
        void RenderInitialLevel();
        void RenderZoomLevels();

        event EventHandler<ChunksRenderedEventArgs> ChunksRendered;
    }
}