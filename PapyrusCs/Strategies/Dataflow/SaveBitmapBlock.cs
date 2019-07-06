using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Maploader.Renderer.Imaging;
using Maploader.World;

namespace PapyrusCs.Strategies.Dataflow
{
    public class SaveBitmapBlock<TImage> : ITplBlock where TImage : class
    {
        private readonly string fileFormat;
        private readonly IGraphicsApi<TImage> graphics;
        public string OutputPath { get; }
        public TransformBlock<ImageInfo<TImage>, IEnumerable<SubChunkData>> Block { get; }
        readonly Random r = new Random();

        public SaveBitmapBlock(string outputPath, int initialZoomLevel, string fileFormat, ExecutionDataflowBlockOptions options,
            IGraphicsApi<TImage> graphics)
        {
            OutputPath = outputPath;
            this.fileFormat = fileFormat;
            this.graphics = graphics;
            Block = new TransformBlock<ImageInfo<TImage>, IEnumerable<SubChunkData>>(info =>
            {
                if (info == null)
                    return null;
                try
                {
                    /*if (r.Next(100) == 0)
                    {
                        throw new ArgumentOutOfRangeException("TestError in SaveBitmap");
                    }*/

                    SaveBitmap(initialZoomLevel, info.X, info.Z, info.Image);
                    ProcessedCount++;
                    return info.Cd;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in SaveBitmapBlock: " + ex.Message);
                    return null;

                }
                finally
                {
                    if (info != null)
                    {
                        graphics.ReturnImage(info.Image);
                        info.Image = null;
                    }
                }

            }, options);
        }


        private void SaveBitmap(int zoom, int x, int z, TImage b)
        {
            var path = Path.Combine(OutputPath, $"{zoom}", $"{x}");
            var filepath = Path.Combine(path, $"{z}.{fileFormat}");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            graphics.SaveImage(b, filepath);
        }

        public int InputCount => Block.InputCount;
        public int OutputCount => 0;
        public int ProcessedCount { get; private set; }
    }
}