using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks.Dataflow;
using Maploader.Renderer.Imaging;
using Maploader.World;

namespace PapyrusCs.Strategies.Dataflow
{
    public class OutputBlock<TImage> : ITplBlock where TImage : class
    {
        private IGraphicsApi<TImage> graphics;
        private int processedCount;
        public string OutputPath { get; }
        public TransformBlock<ImageInfo<TImage>, IEnumerable<SubChunkData>> Block { get; }

        public OutputBlock(string outputPath, int initialZoomLevel, ExecutionDataflowBlockOptions options,
            IGraphicsApi<TImage> graphics)
        {
            OutputPath = outputPath;
            this.graphics = graphics;
            Block = new TransformBlock<ImageInfo<TImage>, IEnumerable<SubChunkData>>(info =>
            {
                SaveBitmap(initialZoomLevel, info.X, info.Z, info.Image);
                processedCount++;
                info.Dispose();
                return info.Cd;
            }, options);
        }


        private void SaveBitmap(int zoom, int x, int z, TImage b)
        {
            var path = Path.Combine(OutputPath, "map", $"{zoom}", $"{x}");
            var filepath = Path.Combine(path, $"{z}.png");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            graphics.SaveImage(b, filepath);
        }

        public int InputCount => Block.InputCount;
        public int OutputCount => 0;
        public int ProcessedCount => processedCount;
    }
}