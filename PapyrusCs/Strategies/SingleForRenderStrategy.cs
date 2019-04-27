using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maploader.Core;

namespace PapyrusCs.Strategies
{
    public class SingleForRenderStrategy : ForRenderStrategy
    {
        protected override Func<IEnumerable<int>, ParallelOptions, Action<int>, ParallelLoopResult> OuterLoopStrategy => NotParallel.ForEach;
    }
}