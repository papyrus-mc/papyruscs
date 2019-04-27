using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Maploader.Core
{
    public static class NotParallel
    {
        public static ParallelLoopResult ForEach<T>(IEnumerable<T> source, ParallelOptions options, Action<T> body)
        {
            foreach (var e in source)
            {
                body(e);
            }

            return new ParallelLoopResult();
        }

        public static ParallelLoopResult ForEach<T>(IEnumerable<T> source, Action<T> body)
        {
            foreach (var e in source)
            {
                body(e);
            }
            return new ParallelLoopResult();
        }
    }
}