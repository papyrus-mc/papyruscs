using System;
using System.Collections.Generic;
using System.Text;

namespace Imazen.WebP.Extern {
    public partial class NativeMethods {

        // Some useful macros:

        /// <summary>
        /// Returns true if the specified mode uses a premultiplied alpha
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static bool WebPIsPremultipliedMode(WEBP_CSP_MODE mode) {

            return (mode == WEBP_CSP_MODE.MODE_rgbA || mode == WEBP_CSP_MODE.MODE_bgrA || mode == WEBP_CSP_MODE.MODE_Argb ||
                mode == WEBP_CSP_MODE.MODE_rgbA_4444);

        }

        /// <summary>
        /// Returns true if the given mode is RGB(A)
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static bool WebPIsRGBMode(WEBP_CSP_MODE mode) {

            return (mode < WEBP_CSP_MODE.MODE_YUV);

        }


        /// <summary>
        /// Returns true if the given mode has an alpha channel
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static bool WebPIsAlphaMode(WEBP_CSP_MODE mode) {

            return (mode == WEBP_CSP_MODE.MODE_RGBA || mode == WEBP_CSP_MODE.MODE_BGRA || mode == WEBP_CSP_MODE.MODE_ARGB ||
                    mode == WEBP_CSP_MODE.MODE_RGBA_4444 || mode == WEBP_CSP_MODE.MODE_YUVA ||
                    WebPIsPremultipliedMode(mode));

        }



        // 

        /// <summary>
        /// Retrieve features from the bitstream. The *features structure is filled
        /// with information gathered from the bitstream.
        /// Returns false in case of error or version mismatch.
        /// In case of error, features->bitstream_status will reflect the error code.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="data_size"></param>
        /// <param name="features"></param>
        /// <returns></returns>
        public static VP8StatusCode WebPGetFeatures(IntPtr data, UIntPtr data_size, ref WebPBitstreamFeatures features) {
            return NativeMethods.WebPGetFeaturesInternal(data, data_size, ref features, WEBP_DECODER_ABI_VERSION);

        }
        /// <summary>
        /// Initialize the configuration as empty. This function must always be
        /// called first, unless WebPGetFeatures() is to be called.
        /// Returns false in case of mismatched version.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static int WebPInitDecoderConfig(ref WebPDecoderConfig config) {

            return WebPInitDecoderConfigInternal(ref config, WEBP_DECODER_ABI_VERSION);

        }


        /// <summary>
        /// Initialize the structure as empty. Must be called before any other use. Returns false in case of version mismatch
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static int WebPInitDecBuffer(ref WebPDecBuffer buffer) {
            return WebPInitDecBufferInternal(ref buffer, WEBP_DECODER_ABI_VERSION);
        }



        //    // Deprecated alpha-less version of WebPIDecGetYUVA(): it will ignore the

        //// alpha information (if present). Kept for backward compatibility.

        //public IntPtr WebPIDecGetYUV(IntPtr decoder, int* last_y, uint8_t** u, uint8_t** v,

        //    int* width, int* height, int* stride, int* uv_stride) {

        //  return WebPIDecGetYUVA(idec, last_y, u, v, NULL, width, height,

        //                         stride, uv_stride, NULL);

    }




}

