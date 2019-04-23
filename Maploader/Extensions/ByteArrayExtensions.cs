using System;
using System.Collections.Generic;
using System.Text;

namespace Maploader.Extensions
{
    public static class IntExtensions
    {
        public static byte GetLeByte(this int b, int index)
        {
            return (byte) ((b >> (index * 8)) & 0xFF);
        } 
    }
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
 