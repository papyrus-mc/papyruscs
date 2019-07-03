using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Maploader.Core;
using Maploader.Renderer;
using Maploader.Renderer.Imaging;
using Maploader.Renderer.Texture;
using Maploader.World;

namespace PapyrusCs.Strategies.Dataflow
{
    class CreateChunkAndRenderBlock<TImage> : ITplBlock where TImage : class
    {
        private int processedCount;
        public TransformBlock<IEnumerable<ChunkData>, ImageInfo<TImage>> Block { get; }

        public CreateChunkAndRenderBlock(
            World world,
            Dictionary<string, Texture> textureDictionary,
            string texturePath,
            RenderSettings renderSettings,
            IGraphicsApi<TImage> graphics,
            int chunkSize, int chunksPerDimension,
            ExecutionDataflowBlockOptions options)
        {
            int tileSize = chunksPerDimension * chunkSize;
            graphics.SetPoolDimensions(tileSize,tileSize);
            int chunkRenderedCounter = 0;
            ThreadLocal<RendererCombi<TImage>> renderCombi = new ThreadLocal<RendererCombi<TImage>>(() => new RendererCombi<TImage>(textureDictionary, texturePath, renderSettings, graphics));

            Block = new TransformBlock<IEnumerable<ChunkData>, ImageInfo<TImage>>(chunkDatas =>
            {
                var b = graphics.GetPooledImage(); // pooled
                //var b = graphics.CreateEmptyImage(tileSize, tileSize);

                var chunkRenderer = renderCombi.Value.ChunkRenderer;
                var firstX = -1;
                var firstZ = -1;

                var count = 0;
                var chunkDataList = chunkDatas.ToList();
                foreach (var chunkData in chunkDataList)
                {
                    var chunk = world.GetChunk(chunkData.X, chunkData.Z, chunkData);

                    if (firstX == -1)
                        firstX = chunkData.X;
                    if (firstZ == -1)
                        firstZ = chunkData.Z;

                    var x = chunk.X % chunksPerDimension;
                    var z = chunk.Z % chunksPerDimension;
                    if (x < 0) x += chunksPerDimension;
                    if (z < 0) z += chunksPerDimension;
                    chunkRenderer.RenderChunk(b, chunk, x * chunkSize, z * chunkSize);

                    world.ChunkPool?.Return(chunk);
                    count++;

             
                }

                var fx = CoordHelpers.GetGroupedCoordinate(firstX, chunksPerDimension);
                var fz = CoordHelpers.GetGroupedCoordinate(firstZ, chunksPerDimension);

                Interlocked.Increment(ref processedCount);
                Interlocked.Add(ref chunkRenderedCounter, count);

                if (chunkRenderedCounter >= 32)
                {
                    var v = chunkRenderedCounter;
                    ChunksRendered?.Invoke(this, new ChunksRenderedEventArgs(v));
                    Interlocked.Add(ref chunkRenderedCounter, -v);
                }

                return new ImageInfo<TImage>()
                {
                    Image = b,
                    X = fx,
                    Z = fz,
                    Cd = chunkDataList.SelectMany(x => x.SubChunks)
                };

            }, options);
        }

        public event EventHandler<ChunksRenderedEventArgs> ChunksRendered;

        public int InputCount => Block.InputCount;
        public int OutputCount => Block.OutputCount;
        public int ProcessedCount => processedCount;
    }
}

