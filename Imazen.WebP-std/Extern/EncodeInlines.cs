using System;
using System.Collections.Generic;
using System.Text;

namespace Imazen.WebP.Extern {
    public partial class NativeMethods {
        /// <summary>
        /// Should always be called, to initialize a fresh WebPConfig structure before
        /// modification. Returns false in case of version mismatch. WebPConfigInit()
        /// must have succeeded before using the 'config' object.
        /// Note that the default values are lossless=0 and quality=75.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static int WebPConfigInit(ref WebPConfig config) {
            return NativeMethods.WebPConfigInitInternal(ref config,WebPPreset.WEBP_PRESET_DEFAULT, 75.0f, WEBP_ENCODER_ABI_VERSION);
        }

        /// <summary>
        /// This function will initialize the configuration according to a predefined
        /// set of parameters (referred to by 'preset') and a given quality factor.
        /// This function can be called as a replacement to WebPConfigInit(). Will return false in case of error.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="preset"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static int WebPConfigPreset(ref WebPConfig config, WebPPreset preset, float quality) {
             return NativeMethods.WebPConfigInitInternal(ref config, preset, quality, WEBP_ENCODER_ABI_VERSION);
        }

        /// <summary>
        /// Should always be called, to initialize the structure. Returns false in case
        /// of version mismatch. WebPPictureInit() must have succeeded before using the
        /// 'picture' object.
        /// Note that, by default, use_argb is false and colorspace is WEBP_YUV420.
        /// </summary>
        /// <param name="picture"></param>
        /// <returns></returns>
        public static int WebPPictureInit(ref WebPPicture picture) {

            return NativeMethods.WebPPictureInitInternal(ref picture, WEBP_ENCODER_ABI_VERSION);

        }
    }
}
