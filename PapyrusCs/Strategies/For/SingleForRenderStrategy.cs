using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maploader.Core;
using Maploader.Renderer.Imaging;

namespace PapyrusCs.Strategies.For
{
    public class SingleForRenderStrategy<TImage> : ForRenderStrategy<TImage> where TImage : class
    {
        public SingleForRenderStrategy(IGraphicsApi<TImage> systemDrawing) : base(systemDrawing)
        {
        }

        protected override Func<IEnumerable<int>, ParallelOptions, Action<int>, ParallelLoopResult> OuterLoopStrategy => NotParallel.ForEach;
    }
}