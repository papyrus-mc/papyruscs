using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Maploader.Core
{
    public static class NotParallel
    {
        /// <summary>
        /// Don't expect a useful ParallelLoopResult from this
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="options"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static ParallelLoopResult ForEach<T>(IEnumerable<T> source, ParallelOptions options, Action<T> body)
        {
            foreach (var e in source)
            {
                body(e);
            }

            return new ParallelLoopResult();
        }

        /// <summary>
        /// Don't expect a useful ParallelLoopResult from this
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="body"></param>
        /// <returns></returns>
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