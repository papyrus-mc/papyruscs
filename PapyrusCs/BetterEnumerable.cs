using System.Collections.Generic;

namespace PapyrusCs
{
    public static class BetterEnumerable
    {
        public static IEnumerable<int> SteppedRange(int fromInclusive, int toExclusive, int step)
        {
            for (var i = fromInclusive; i < toExclusive; i += step)
            {
                yield return i;
            }
        }
    }
}