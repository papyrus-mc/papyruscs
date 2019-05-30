using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maploader.Renderer.Imaging;

namespace PapyrusCs.Strategies.For
{
    public class ParallelForRenderStrategy<TImage> : ForRenderStrategy<TImage> where TImage : class
    {
        protected override Func<IEnumerable<int>, ParallelOptions, Action<int>, ParallelLoopResult> OuterLoopStrategy => Parallel.ForEach;

        public ParallelForRenderStrategy(IGraphicsApi<TImage> graphics) : base(graphics)
        {
        }
    }
}