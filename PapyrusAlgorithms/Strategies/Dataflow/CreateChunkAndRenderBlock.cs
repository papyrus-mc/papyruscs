using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Maploader.Core;
using Maploader.Renderer;
using Maploader.Renderer.Imaging;
using Maploader.Renderer.Texture;
using Maploader.World;

namespace PapyrusAlgorithms.Strategies.Dataflow
{
    class CreateChunkAndRenderBlock<TImage> : ITplBlock where TImage : class
    {
        private int processedCount;
        public TransformBlock<IEnumerable<ChunkData>, ImageInfo<TImage>> Block { get; }
        Random r = new Random();

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
            var renderCombiPool = new GenericPool<RendererCombi<TImage>>(() => new RendererCombi<TImage>(textureDictionary, texturePath, renderSettings, graphics));

            Block = new TransformBlock<IEnumerable<ChunkData>, ImageInfo<TImage>>(chunkDatas =>
            {
                RendererCombi<TImage> chunkRenderer = null;
                try
                {
                    var b = graphics.GetPooledImage(); // pooled
                    //var b = graphics.CreateEmptyImage(tileSize, tileSize);

                    chunkRenderer = renderCombiPool.Get();
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
                        try
                        {
                            chunkRenderer.ChunkRenderer.RenderChunk(b, chunk, x * chunkSize, z * chunkSize);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("RenderChunk Error at " + chunkData.X + " " + chunkData.Z + " : " + ex.Message);
                        }

                        world.ChunkPool?.Return(chunk);
                        count++;
                    }


                    var fx = CoordHelpers.GetGroupedCoordinate(firstX, chunksPerDimension);
                    var fz = CoordHelpers.GetGroupedCoordinate(firstZ, chunksPerDimension);

                    Interlocked.Increment(ref processedCount);
                    Interlocked.Add(ref chunkRenderedCounter, count);

                    if (chunkRenderedCounter >= 128)
                    {
                        var v = chunkRenderedCounter;
                        ChunksRendered?.Invoke(this, new ChunksRenderedEventArgs(v));
                        Interlocked.Add(ref chunkRenderedCounter, -v);
                    }

                    /*if (r.Next(100) == 0)
                    {
                        throw new ArgumentOutOfRangeException("Test Error in CreateChunkAndRender");
                    }*/

                    

                    return new ImageInfo<TImage>()
                    {
                        Image = b,
                        X = fx,
                        Z = fz,
                        Cd = chunkDataList.SelectMany(x => x.SubChunks)
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in CreateChunkAndRenderBlock: " + ex.Message);
                    return null;
                }
                finally
                {
                    if (chunkRenderer != null) renderCombiPool.Return(chunkRenderer);
                }
            }, options);
        }

        public event EventHandler<ChunksRenderedEventArgs> ChunksRendered;

        public int InputCount => Block.InputCount;
        public int OutputCount => Block.OutputCount;
        public int ProcessedCount => processedCount;
    }
}

