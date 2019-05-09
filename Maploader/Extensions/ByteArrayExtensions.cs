using System;
using System.Collections.Generic;
using System.Text;

namespace Maploader.Extensions
{

    public static class ByteArrayExtensions
    {
        public static Int32 GetIntLe(this byte[] array, int startPos)
        {
            return array[startPos] |
                   (array[startPos + 1] << 8) |
                   (array[startPos + 2] << 16) |
                   (array[startPos + 3] << 24);
        }
    }
}
 