using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace Imazen.WebP.Extern
{

    public enum WebPImageHint
    {

        /// WEBP_HINT_DEFAULT -> 0
        WEBP_HINT_DEFAULT = 0,

        WEBP_HINT_PICTURE,

        WEBP_HINT_PHOTO,

        WEBP_HINT_GRAPH,

        WEBP_HINT_LAST,
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPConfig
    {

        /// Lossless encoding (0=lossy(default), 1=lossless).
        public int lossless;

        /// between 0 (smallest file) and 100 (biggest)
        public float quality;

        /// quality/speed trade-off (0=fast, 6=slower-better)
        public int method;

        /// WebPImageHint->Anonymous_838f22f5_6f57_48a0_9ecb_8eec917009f9
        public WebPImageHint image_hint;

        // Parameters related to lossy compression only:

        /// if non-zero, set the desired target size in bytes. Takes precedence over the 'compression' parameter.
        public int target_size;

        /// if non-zero, specifies the minimal distortion to try to achieve. Takes precedence over target_size.
        public float target_PSNR;

        /// maximum number of segments to use, in [1..4]
        public int segments;

        /// Spatial Noise Shaping. 0=off, 100=maximum.
        public int sns_strength;

        /// range: [0 = off .. 100 = strongest]
        public int filter_strength;

        /// range: [0 = off .. 7 = least sharp]
        public int filter_sharpness;

        /// filtering type: 0 = simple, 1 = strong (only used
        /// if filter_strength > 0 or autofilter > 0)
        public int filter_type;

        ///  Auto adjust filter's strength [0 = off, 1 = on]
        public int autofilter;

        /// Algorithm for encoding the alpha plane (0 = none,
        /// 1 = compressed with WebP lossless). Default is 1.
        public int alpha_compression;

        /// Predictive filtering method for alpha plane.
        ///  0: none, 1: fast, 2: best. Default if 1.
        public int alpha_filtering;

        /// Between 0 (smallest size) and 100 (lossless).
                           // Default is 100.
        public int alpha_quality;

        /// number of entropy-analysis passes (in [1..10]).
        public int pass;

        /// if true, export the compressed picture back.
        /// In-loop filtering is not applied.
        public int show_compressed;

        /// preprocessing filter:
        /// 0=none, 1=segment-smooth, 2=pseudo-random dithering
        public int preprocessing;

        /// log2(number of token partitions) in [0..3]. Default
        /// is set to 0 for easier progressive decoding.
        public int partitions;

        /// quality degradation allowed to fit the 512k limit
        /// on prediction modes coding (0: no degradation,
        /// 100: maximum possible degradation).
        public int partition_limit;

        /// <summary>
        /// If true, compression parameters will be remapped
        /// to better match the expected output size from
        /// JPEG compression. Generally, the output size will
        /// be similar but the degradation will be lower.
        /// </summary>
        public int emulate_jpeg_size;

        /// If non-zero, try and use multi-threaded encoding.
        public int thread_level;

        /// <summary>
        /// If set, reduce memory usage (but increase CPU use).
        /// </summary>
        public int low_memory;

        /// <summary>
        ///  Near lossless encoding [0 = max loss .. 100 = off (default)].
        /// </summary>
        public int near_lossless; 
           
        /// if non-zero, preserve the exact RGB values under
        /// transparent area. Otherwise, discard this invisible
        /// RGB information for better compression. The default
        /// value is 0. 
        public int exact;             

        /// uint32_t[3]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.U4)]
        public uint[] pad;
    }

    public enum WebPPreset
    {

        /// WEBP_PRESET_DEFAULT -> 0
        WEBP_PRESET_DEFAULT = 0,

        /// digital picture, like portrait, inner shot
        WEBP_PRESET_PICTURE,

        /// outdoor photograph, with natural lighting
        WEBP_PRESET_PHOTO,

        /// hand or line drawing, with high-contrast details
        WEBP_PRESET_DRAWING,

        /// small-sized colorful images
        WEBP_PRESET_ICON,

        /// text-like
        WEBP_PRESET_TEXT,
    }






    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPAuxStats
    {

        /// int
        public int coded_size;

        /// float[5]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.R4)]
        public float[] PSNR;

        /// int[3]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.I4)]
        public int[] block_count;

        /// int[2]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.I4)]
        public int[] header_bytes;

        /// int[12]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 12, ArraySubType = UnmanagedType.I4)]
        public int[] residual_bytes;

        /// int[4]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I4)]
        public int[] segment_size;

        /// int[4]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I4)]
        public int[] segment_quant;

        /// int[4]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I4)]
        public int[] segment_level;

        /// int
        public int alpha_data_size;

        /// int
        public int layer_data_size;

        /// uint32_t->unsigned int
        public uint lossless_features;

        /// int
        public int histogram_bits;

        /// int
        public int transform_bits;

        /// int
        public int cache_bits;

        /// int
        public int palette_size;

        /// int
        public int lossless_size;

        /// lossless header (transform, huffman etc) size
        public  int lossless_hdr_size;

        /// lossless image data size
        public int lossless_data_size;      
        /// uint32_t[2]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
        public uint[] pad;
    }


    /// Return Type: int
    ///data: uint8_t*
    ///data_size: size_t->unsigned int
    ///picture: WebPPicture*
    public delegate int WebPWriterFunction([InAttribute()] IntPtr data, UIntPtr data_size, ref WebPPicture picture);

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPMemoryWriter
    {

        /// uint8_t*
        public IntPtr mem;

        /// size_t->unsigned int
        public UIntPtr size;

        /// size_t->unsigned int
        public UIntPtr max_size;

        /// uint32_t[1]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.U4)]
        public uint[] pad;
    }



    /// Return Type: int
    ///percent: int
    ///picture: WebPPicture*
    public delegate int WebPProgressHook(int percent, ref WebPPicture picture);

    public enum WebPEncCSP
    {

        /// 4:2:0 (half-res chroma x and y)
        WEBP_YUV420 = 0,

        /// bit-mask to get the UV sampling factors
        WEBP_CSP_UV_MASK = 3,

        /// 4:2:0 with alpha
        WEBP_YUV420A = 4,

        /// Bit mask to set alpha
        WEBP_CSP_ALPHA_BIT = 4,
    }


    public enum WebPEncodingError
    {

        /// VP8_ENC_OK -> 0
        VP8_ENC_OK = 0,

        VP8_ENC_ERROR_OUT_OF_MEMORY,

        VP8_ENC_ERROR_BITSTREAM_OUT_OF_MEMORY,

        VP8_ENC_ERROR_NULL_PARAMETER,

        VP8_ENC_ERROR_INVALID_CONFIGURATION,

        VP8_ENC_ERROR_BAD_DIMENSION,

        VP8_ENC_ERROR_PARTITION0_OVERFLOW,

        VP8_ENC_ERROR_PARTITION_OVERFLOW,

        VP8_ENC_ERROR_BAD_WRITE,

        VP8_ENC_ERROR_FILE_TOO_BIG,

        VP8_ENC_ERROR_USER_ABORT,

        VP8_ENC_ERROR_LAST,
    }


    /// <summary>
    ///  Main exchange structure (input samples, output bytes, statistics)
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPPicture
    {

        //   INPUT
        //////////////
        // Main flag for encoder selecting between ARGB or YUV input.
        // It is recommended to use ARGB input (*argb, argb_stride) for lossless
        // compression, and YUV input (*y, *u, *v, etc.) for lossy compression
        // since these are the respective native colorspace for these formats.
        public int use_argb;

        // YUV input (mostly used for input to lossy compression)

        /// colorspace: should be YUV420 for now (=Y'CbCr). WebPEncCSP->Anonymous_84ce7065_fe91_48b4_93d8_1f0e84319dba
        public WebPEncCSP colorspace;

        /// int
        public int width;

        /// int
        public int height;

        /// uint8_t* pointers to luma/chroma planes.
        public IntPtr y;

        /// uint8_t*
        public IntPtr u;

        /// uint8_t*
        public IntPtr v;

        /// int
        public int y_stride;

        /// int
        public int uv_stride;

        /// uint8_t*
        public IntPtr a;

        /// int
        public int a_stride;

        /// uint32_t[2]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
        public uint[] pad1;

        /// uint32_t*
        public IntPtr argb;

        /// int
        public int argb_stride;

        /// uint32_t[3]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.U4)]
        public uint[] pad2;

        // OUTPUT 


        /// WebPWriterFunction
        public WebPWriterFunction writer;

        /// void*
        public IntPtr custom_ptr;

        /// int
        public int extra_info_type;

        /// uint8_t*
        public IntPtr extra_info;

        /// WebPAuxStats*
        public IntPtr stats;

        /// WebPEncodingError->Anonymous_8b714d63_f91b_46af_b0d0_667c703ed356
        public WebPEncodingError error_code;

        /// WebPProgressHook
        public WebPProgressHook progress_hook;

        /// void*
        public IntPtr user_data;

        /// uint32_t[3]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.U4)]
        public uint[] pad3;

        /// uint8_t*
        public IntPtr pad4;

        /// uint8_t*
        public IntPtr pad5;
        

        /// uint32_t[8]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U4)]
        public uint[] pad6;

        /// void*
        public IntPtr memory_;

        /// void*
        public IntPtr memory_argb_;

        /// void*[2]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.SysUInt)]
        public IntPtr[] pad7;
    }


    public partial class NativeMethods
    {

        /// Return Type: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPGetEncoderVersion", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPGetEncoderVersion();


        /// Return Type: size_t->unsigned int
        ///rgb: uint8_t*
        ///width: int
        ///height: int
        ///stride: int
        ///quality_factor: float
        ///output: uint8_t**
        [DllImportAttribute("libwebp", EntryPoint = "WebPEncodeRGB", CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr WebPEncodeRGB([InAttribute()] IntPtr rgb, int width, int height, int stride, float quality_factor, ref IntPtr output);


        /// Return Type: size_t->unsigned int
        ///bgr: uint8_t*
        ///width: int
        ///height: int
        ///stride: int
        ///quality_factor: float
        ///output: uint8_t**
        [DllImportAttribute("libwebp", EntryPoint = "WebPEncodeBGR", CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr WebPEncodeBGR([InAttribute()] IntPtr bgr, int width, int height, int stride, float quality_factor, ref IntPtr output);


        /// Return Type: size_t->unsigned int
        ///rgba: uint8_t*
        ///width: int
        ///height: int
        ///stride: int
        ///quality_factor: float
        ///output: uint8_t**
        [DllImportAttribute("libwebp", EntryPoint = "WebPEncodeRGBA", CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr WebPEncodeRGBA([InAttribute()] IntPtr rgba, int width, int height, int stride, float quality_factor, ref IntPtr output);


        /// Return Type: size_t->unsigned int
        ///bgra: uint8_t*
        ///width: int
        ///height: int
        ///stride: int
        ///quality_factor: float
        ///output: uint8_t**
        [DllImportAttribute("libwebp", EntryPoint = "WebPEncodeBGRA", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPEncodeBGRA([InAttribute()] IntPtr bgra, int width, int height, int stride, float quality_factor, ref IntPtr output);


        /// Return Type: size_t->unsigned int
        ///rgb: uint8_t*
        ///width: int
        ///height: int
        ///stride: int
        ///output: uint8_t**
        [DllImportAttribute("libwebp", EntryPoint = "WebPEncodeLosslessRGB", CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr WebPEncodeLosslessRGB([InAttribute()] IntPtr rgb, int width, int height, int stride, ref IntPtr output);


        /// Return Type: size_t->unsigned int
        ///bgr: uint8_t*
        ///width: int
        ///height: int
        ///stride: int
        ///output: uint8_t**
        [DllImportAttribute("libwebp", EntryPoint = "WebPEncodeLosslessBGR", CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr WebPEncodeLosslessBGR([InAttribute()] IntPtr bgr, int width, int height, int stride, ref IntPtr output);


        /// Return Type: size_t->unsigned int
        ///rgba: uint8_t*
        ///width: int
        ///height: int
        ///stride: int
        ///output: uint8_t**
        [DllImportAttribute("libwebp", EntryPoint = "WebPEncodeLosslessRGBA", CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr WebPEncodeLosslessRGBA([InAttribute()] IntPtr rgba, int width, int height, int stride, ref IntPtr output);


        /// Return Type: size_t->unsigned int
        ///bgra: uint8_t*
        ///width: int
        ///height: int
        ///stride: int
        ///output: uint8_t**
        [DllImportAttribute("libwebp", EntryPoint = "WebPEncodeLosslessBGRA", CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr WebPEncodeLosslessBGRA([InAttribute()] IntPtr bgra, int width, int height, int stride, ref IntPtr output);


        /// Return Type: int
        ///param0: WebPConfig*
        ///param1: WebPPreset->Anonymous_017d4167_f53e_4b3d_b029_592ff5c3f80b
        ///param2: float
        ///param3: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPConfigInitInternal", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPConfigInitInternal(ref WebPConfig param0, WebPPreset param1, float param2, int param3);


        /// Return Type: int
        ///config: WebPConfig*
        [DllImportAttribute("libwebp", EntryPoint = "WebPConfigLosslessPreset", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPConfigLosslessPreset(ref WebPConfig config);

        /// Return Type: int
        ///config: WebPConfig*
        [DllImportAttribute("libwebp", EntryPoint = "WebPValidateConfig", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPValidateConfig(ref WebPConfig config);


        /// Return Type: void
        ///writer: WebPMemoryWriter*
        [DllImportAttribute("libwebp", EntryPoint = "WebPMemoryWriterInit", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WebPMemoryWriterInit(ref WebPMemoryWriter writer);

        /// Return Type: void
        ///writer: WebPMemoryWriter*
        [DllImportAttribute("libwebp", EntryPoint = "WebPMemoryWriterClear", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WebPMemoryWriterClear(ref WebPMemoryWriter writer);


        /// Return Type: int
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///picture: WebPPicture*
        [DllImportAttribute("libwebp", EntryPoint = "WebPMemoryWrite", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPMemoryWrite([InAttribute()] IntPtr data, UIntPtr data_size, ref WebPPicture picture);


        /// Return Type: int
        ///param0: WebPPicture*
        ///param1: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureInitInternal", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureInitInternal(ref WebPPicture param0, int param1);


        /// Return Type: int
        ///picture: WebPPicture*
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureAlloc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureAlloc(ref WebPPicture picture);


        /// Return Type: void
        ///picture: WebPPicture*
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureFree", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WebPPictureFree(ref WebPPicture picture);


        /// Return Type: int
        ///src: WebPPicture*
        ///dst: WebPPicture*
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureCopy", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureCopy(ref WebPPicture src, ref WebPPicture dst);


        /// Return Type: int
        ///pic1: WebPPicture*
        ///pic2: WebPPicture*
        ///metric_type: int
        ///result: float* result[5]
        ///

        /// <summary>
        /// Compute PSNR, SSIM or LSIM distortion metric between two pictures.
        /// Result is in dB, stores in result[] in the  Y/U/V/Alpha/All or B/G/R/A/All order.
        /// Returns false in case of error (src and ref don't have same dimension, ...)
        /// Warning: this function is rather CPU-intensive.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="reference"></param>
        /// <param name="metric_type">0 = PSNR, 1 = SSIM, 2 = LSIM</param>
        /// <param name="result"></param>
        /// <returns></returns>
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureDistortion", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureDistortion(ref WebPPicture src, ref WebPPicture reference, int metric_type, ref float result);




        /// Return Type: int
        ///picture: WebPPicture*
        ///left: int
        ///top: int
        ///width: int
        ///height: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureCrop", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureCrop(ref WebPPicture picture, int left, int top, int width, int height);


        /// Return Type: int
        ///src: WebPPicture*
        ///left: int
        ///top: int
        ///width: int
        ///height: int
        ///dst: WebPPicture*
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureView", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureView(ref WebPPicture src, int left, int top, int width, int height, ref WebPPicture dst);


        /// Return Type: int
        ///picture: WebPPicture*
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureIsView", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureIsView(ref WebPPicture picture);


        /// Rescale a picture to new dimension width x height.
        /// ow gamma correction is applied.
        /// 
        /// Return Type: int
        ///pic: WebPPicture*
        ///width: int
        ///height: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureRescale", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureRescale(ref WebPPicture pic, int width, int height);


        /// Return Type: int
        ///picture: WebPPicture*
        ///rgb: uint8_t*
        ///rgb_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureImportRGB", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureImportRGB(ref WebPPicture picture, [InAttribute()] IntPtr rgb, int rgb_stride);


        /// Return Type: int
        ///picture: WebPPicture*
        ///rgba: uint8_t*
        ///rgba_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureImportRGBA", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureImportRGBA(ref WebPPicture picture, [InAttribute()] IntPtr rgba, int rgba_stride);


        /// Performs 'smart' RGBA->YUVA420 downsampling and colorspace conversion.
        /// Downsampling is handled with extra care in case of color clipping. This
        /// method is roughly 2x slower than WebPPictureARGBToYUVA() but produces better
        /// YUV representation.
        /// Returns false in case of error.
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureSmartARGBToYUVA", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureSmartARGBToYUVA(ref WebPPicture picture);

        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureImportRGBX", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureImportRGBX(ref WebPPicture picture, [InAttribute()] IntPtr rgbx, int rgbx_stride);



        /// Return Type: int
        ///picture: WebPPicture*
        ///bgr: uint8_t*
        ///bgr_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureImportBGR", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureImportBGR(ref WebPPicture picture, [InAttribute()] IntPtr bgr, int bgr_stride);


        /// Return Type: int
        ///picture: WebPPicture*
        ///bgra: uint8_t*
        ///bgra_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureImportBGRA", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureImportBGRA(ref WebPPicture picture, [InAttribute()] IntPtr bgra, int bgra_stride);


        /// Return Type: int
        ///picture: WebPPicture*
        ///bgrx: uint8_t*
        ///bgrx_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureImportBGRX", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureImportBGRX(ref WebPPicture picture, [InAttribute()] IntPtr bgrx, int bgrx_stride);


        /// Return Type: int
        ///picture: WebPPicture*
        ///colorspace: WebPEncCSP->Anonymous_84ce7065_fe91_48b4_93d8_1f0e84319dba
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureARGBToYUVA", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureARGBToYUVA(ref WebPPicture picture, WebPEncCSP colorspace);


        /// Return Type: int
        ///picture: WebPPicture*
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureYUVAToARGB", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureYUVAToARGB(ref WebPPicture picture);


        /// Return Type: void
        ///picture: WebPPicture*
        [DllImportAttribute("libwebp", EntryPoint = "WebPCleanupTransparentArea", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WebPCleanupTransparentArea(ref WebPPicture picture);


        /// Return Type: int
        ///picture: WebPPicture*
        [DllImportAttribute("libwebp", EntryPoint = "WebPPictureHasTransparency", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPPictureHasTransparency(ref WebPPicture picture);


        /// Return Type: int
        ///config: WebPConfig*
        ///picture: WebPPicture*
        [DllImportAttribute("libwebp", EntryPoint = "WebPEncode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPEncode(ref WebPConfig config, ref WebPPicture picture);

    }
}

#pragma warning restore 1591