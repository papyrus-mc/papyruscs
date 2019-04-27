using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PapyrusCs.Strategies
{
    public class ParallelForRenderStrategy : ForRenderStrategy
    {
        protected override Func<IEnumerable<int>, ParallelOptions, Action<int>, ParallelLoopResult> OuterLoopStrategy => Parallel.ForEach;
    }
}