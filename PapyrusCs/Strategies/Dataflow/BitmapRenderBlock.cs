using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Maploader.Renderer;
using Maploader.Renderer.Imaging;
using Maploader.Renderer.Texture;
using Maploader.World;

namespace PapyrusCs.Strategies.Dataflow
{
    public class BitmapRenderBlock<TImage> : ITplBlock where TImage : class
    {
        private int processedCount;
        public TransformBlock<IEnumerable<Chunk>, ImageInfo<TImage>> Block { get; }

        public BitmapRenderBlock(Dictionary<string, Texture> textureDictionary, string texturePath,
            RenderSettings renderSettings, IGraphicsApi<TImage> graphics, int chunkSize, int chunksPerDimension,
            ExecutionDataflowBlockOptions options)
        {
            int tileSize = chunksPerDimension * chunkSize;
            int chunkRenderedCounter = 0;
            ThreadLocal<RendererCombi<TImage>> renderCombi = new ThreadLocal<RendererCombi<TImage>>(() =>
                new RendererCombi<TImage>(textureDictionary, texturePath, renderSettings, graphics));

            Block = new TransformBlock<IEnumerable<Chunk>, ImageInfo<TImage>>(chunks =>
            {
                var b = graphics.CreateEmptyImage(tileSize, tileSize);
                {
                    var chunkList = chunks.ToList();
                    var first = chunkList.First();
                    var chunkRenderer = renderCombi.Value.ChunkRenderer;

                    foreach (var chunk in chunkList)
                    {
                        var x = chunk.X % chunksPerDimension;
                        var z = chunk.Z % chunksPerDimension;
                        if (x < 0) x += chunksPerDimension;
                        if (z < 0) z += chunksPerDimension;
                        chunkRenderer.RenderChunk(b, chunk, x * chunkSize, z * chunkSize);
                    }


                    var fx = CoordHelpers.GetGroupedCoordinate(first.X, chunksPerDimension);
                    var fz = CoordHelpers.GetGroupedCoordinate(first.Z, chunksPerDimension);

                    Interlocked.Increment(ref processedCount);
                    Interlocked.Add(ref chunkRenderedCounter, chunkList.Count);

                    if (chunkRenderedCounter >= 32)
                    {
                        var v = chunkRenderedCounter;
                        ChunksRendered?.Invoke(this, new ChunksRenderedEventArgs(v));
                        Interlocked.Add(ref chunkRenderedCounter, -v);
                    }

                    return new ImageInfo<TImage>() {Image = b, X = fx, Z = fz};
                }
            }, options);
        }

        public event EventHandler<ChunksRenderedEventArgs> ChunksRendered;

        public int InputCount => Block.InputCount;
        public int OutputCount => Block.OutputCount;
        public int ProcessedCount => processedCount;
    }
}