using System;
using System.Collections.Generic;
using System.Text;

namespace Maploader.Extensions
{
    public static class IntExtensions
    {
        public static bool IsEven(this int i)
        {
            return i % 2 == 0;
        }

        public static bool IsOdd(this int i)
        {
            return !IsEven(i);
        }

        public static byte GetLeByte(this int b, int index)
        {
            return (byte) ((b >> (index * 8)) & 0xFF);
        }

    }
}
