using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using Imazen.WebP.Extern;
using System.IO;

namespace Imazen.WebP {
    /// <summary>
    /// Encodes Bitmap objects into WebP format
    /// </summary>
    public class SimpleEncoder {
        public SimpleEncoder() { }

        public static string GetEncoderVersion()
        {
            uint v = (uint)NativeMethods.WebPGetEncoderVersion();
            var revision = v % 256;
            var minor = (v >> 8) % 256;
            var major = (v >> 16) % 256;
            return major + "." + minor + "." + revision;
        }

        /// <summary>
        /// Encodes the given RGB(A) bitmap to the given stream. Specify quality = -1 for lossless, otherwise specify a value between 0 and 100.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="quality"></param>
        /// <param name="noAlpha"></param>
        [Obsolete]
        public void Encode(Bitmap from, Stream to, float quality, bool noAlpha) {
            Encode(from, to, quality);
        }

        /// <summary>
        /// Encodes the given RGB(A) bitmap to the given stream. Specify quality = -1 for lossless, otherwise specify a value between 0 and 100.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="quality"></param>
        public void Encode(Bitmap from, Stream to, float quality) {
            IntPtr result;
            long length;

            Encode(from, quality, out result, out length);
            try {
                byte[] buffer = new byte[4096];
                for (int i = 0; i < length; i += buffer.Length) {
                    int used = (int)Math.Min((int)buffer.Length, length - i);
                    Marshal.Copy((IntPtr)((long)result + i), buffer, 0, used);
                    to.Write(buffer, 0, used);
                }
            } finally {
                NativeMethods.WebPSafeFree(result);
            }

        }

        /// <summary>
        /// Encodes the given RGB(A) bitmap to the given stream. Specify quality = -1 for lossless, otherwise specify a value between 0 and 100.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="quality"></param>
        /// <param name="noAlpha"></param>
        /// <param name="result"></param>
        /// <param name="length"></param>
        [Obsolete]
        public void Encode(Bitmap b, float quality, bool noAlpha, out IntPtr result, out long length) {
            Encode(b, quality, out result, out length);
        }

        /// <summary>
        /// Encodes the given RGB(A) bitmap to an unmanged memory buffer (returned via result/length). Specify quality = -1 for lossless, otherwise specify a value between 0 and 100.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="quality"></param>
        /// <param name="result"></param>
        /// <param name="length"></param>
        public void Encode(Bitmap b, float quality, out IntPtr result, out long length) {
            if (quality < -1) quality = -1;
            if (quality > 100) quality = 100;
            int w = b.Width;
            int h = b.Height;
            var bd = b.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, b.PixelFormat);
            try {
                result = IntPtr.Zero;

                if (b.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb) {
                    if (quality == -1) length = (long)NativeMethods.WebPEncodeLosslessBGRA(bd.Scan0, w, h, bd.Stride, ref result);
                    else length = (long)NativeMethods.WebPEncodeBGRA(bd.Scan0, w, h, bd.Stride, quality, ref result);
                }else if (b.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                {
                    if (quality == -1) length = (long)NativeMethods.WebPEncodeLosslessBGR(bd.Scan0, w, h, bd.Stride, ref result);
                    else length = (long)NativeMethods.WebPEncodeBGR(bd.Scan0, w, h, bd.Stride, quality, ref result);
                }else
                {
                    using (Bitmap b2 = b.Clone(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        Encode(b2, quality, out result, out length);
                    }
                }
                if (length == 0) throw new Exception("WebP encode failed!");
            } finally {
                b.UnlockBits(bd);
            }
        }
    }
}
