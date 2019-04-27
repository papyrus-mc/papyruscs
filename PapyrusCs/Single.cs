using System;
using System.Collections.Generic;

namespace PapyrusCs
{
    public static class Single
    {
        public static void ForEach<T>(IEnumerable<T> source, Action<T> body)
        {
            foreach (var e in source)
            {
                body(e);
            }
        }
    }
}