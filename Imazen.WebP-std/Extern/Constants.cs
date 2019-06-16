using System;
using System.Collections.Generic;
using System.Text;
#pragma warning disable 1591
namespace Imazen.WebP.Extern {
    public partial class NativeMethods {

        /// WEBP_DECODER_ABI_VERSION 0x0208    // MAJOR(8b) + MINOR(8b)
        public const int WEBP_DECODER_ABI_VERSION = 520;

        /// WEBP_ENCODER_ABI_VERSION 0x0209    // MAJOR(8b) + MINOR(8b)
        public const int WEBP_ENCODER_ABI_VERSION = 521;

        /// <summary>
        /// The maximum length of any dimension of a WebP image is 16383
        /// </summary>
        public const int WEBP_MAX_DIMENSION = 16383;
    }
}
#pragma warning restore 1591
