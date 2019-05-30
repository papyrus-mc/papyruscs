using System;
using System.Collections.Generic;
using System.Text;

namespace Maploader.World
{
    public static class CoordHelpers
    {
        public static int GetGroupedCoordinate(int coord, int chunkPerDimension)
        {
            if (coord >= 0)
                return coord / chunkPerDimension;
            return ((coord + 1) / chunkPerDimension) - 1;
        }
    }
}
