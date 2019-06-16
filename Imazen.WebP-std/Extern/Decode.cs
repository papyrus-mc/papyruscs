using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
#pragma warning disable 1591

namespace Imazen.WebP.Extern
{


    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPIDecoder { }





    public enum WEBP_CSP_MODE
    {

        /// MODE_RGB -> 0
        MODE_RGB = 0,

        /// MODE_RGBA -> 1
        MODE_RGBA = 1,

        /// MODE_BGR -> 2
        MODE_BGR = 2,

        /// MODE_BGRA -> 3
        MODE_BGRA = 3,

        /// MODE_ARGB -> 4
        MODE_ARGB = 4,

        /// MODE_RGBA_4444 -> 5
        MODE_RGBA_4444 = 5,

        /// MODE_RGB_565 -> 6
        MODE_RGB_565 = 6,

        /// MODE_rgbA -> 7
        MODE_rgbA = 7,

        /// MODE_bgrA -> 8
        MODE_bgrA = 8,

        /// MODE_Argb -> 9
        MODE_Argb = 9,

        /// MODE_rgbA_4444 -> 10
        MODE_rgbA_4444 = 10,

        /// MODE_YUV -> 11
        MODE_YUV = 11,

        /// MODE_YUVA -> 12
        MODE_YUVA = 12,

        /// MODE_LAST -> 13
        MODE_LAST = 13,
    }






    //------------------------------------------------------------------------------
    // WebPDecBuffer: Generic structure for describing the output sample buffer.

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPRGBABuffer
    {

        /// uint8_t*
        public IntPtr rgba;

        /// int
        public int stride;

        /// size_t->unsigned int
        public UIntPtr size;
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPYUVABuffer
    {

        /// uint8_t*
        public IntPtr y;

        /// uint8_t*
        public IntPtr u;

        /// uint8_t*
        public IntPtr v;

        /// uint8_t*
        public IntPtr a;

        /// int
        public int y_stride;

        /// int
        public int u_stride;

        /// int
        public int v_stride;

        /// int
        public int a_stride;

        /// size_t->unsigned int
        public UIntPtr y_size;

        /// size_t->unsigned int
        public UIntPtr u_size;

        /// size_t->unsigned int
        public UIntPtr v_size;

        /// size_t->unsigned int
        public UIntPtr a_size;
    }

    [StructLayoutAttribute(LayoutKind.Explicit)]
    public struct Anonymous_690ed5ec_4c3d_40c6_9bd0_0747b5a28b54
    {

        /// WebPRGBABuffer->Anonymous_47cdec86_3c1a_4b39_ab93_76bc7499076a
        [FieldOffsetAttribute(0)]
        public WebPRGBABuffer RGBA;

        /// WebPYUVABuffer->Anonymous_70de6e8e_c3ae_4506_bef0_c17f17a7e678
        [FieldOffsetAttribute(0)]
        public WebPYUVABuffer YUVA;
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPDecBuffer
    {

        /// WEBP_CSP_MODE->Anonymous_cb136f5b_1d5d_49a0_aca4_656a79e9d159
        public WEBP_CSP_MODE colorspace;

        /// int
        public int width;

        /// int
        public int height;

        /// int
        public int is_external_memory;

        /// Anonymous_690ed5ec_4c3d_40c6_9bd0_0747b5a28b54
        public Anonymous_690ed5ec_4c3d_40c6_9bd0_0747b5a28b54 u;

        /// uint32_t[4]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U4)]
        public uint[] pad;

        /// uint8_t*
        public IntPtr private_memory;
    }


    //------------------------------------------------------------------------------
    // Enumeration of the status codes

    public enum VP8StatusCode
    {

        /// VP8_STATUS_OK -> 0
        VP8_STATUS_OK = 0,

        VP8_STATUS_OUT_OF_MEMORY,

        VP8_STATUS_INVALID_PARAM,

        VP8_STATUS_BITSTREAM_ERROR,

        VP8_STATUS_UNSUPPORTED_FEATURE,

        VP8_STATUS_SUSPENDED,

        VP8_STATUS_USER_ABORT,

        VP8_STATUS_NOT_ENOUGH_DATA,
    }




















    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPBitstreamFeatures
    {

        /// <summary>
        /// Width in pixels, as read from the bitstream
        /// </summary>
        public int width;

        /// <summary>
        /// Height in pixels, as read from the bitstream.
        /// </summary>
        public int height;

        /// <summary>
        /// // True if the bitstream contains an alpha channel.
        /// </summary>
        public int has_alpha;

        /// <summary>
        /// True if the bitstream contains an animation
        /// </summary>
        public int has_animation;

        /// <summary>
        /// 0 = undefined (/mixed), 1 = lossy, 2 = lossless
        /// </summary>
        public int format;


        /// <summary>
        /// Padding for later use
        /// </summary>
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U4)]
        public uint[] pad;
    }




    // Decoding options
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPDecoderOptions
    {
        public int bypass_filtering;               // if true, skip the in-loop filtering
        public int no_fancy_upsampling;            // if true, use faster pointwise upsampler
        public int use_cropping;                   // if true, cropping is applied _first_
        public int crop_left, crop_top;            // top-left position for cropping.
        // Will be snapped to even values.
        public int crop_width, crop_height;        // dimension of the cropping area
        public int use_scaling;                    // if true, scaling is applied _afterward_
        public int scaled_width, scaled_height;    // final resolution
        public int use_threads;                    // if true, use multi-threaded decoding
        public int dithering_strength;             // dithering strength (0=Off, 100=full)

        public int flip;                           // flip output vertically
        public int alpha_dithering_strength;       // alpha dithering strength in [0..100]

        /// uint32_t[5]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U4)]
        public uint[] pad;
    };

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPDecoderConfig
    {

        /// WebPBitstreamFeatures->Anonymous_c6b01f0b_3e38_4731_b2d6_9c0e3bdb71aa
        public WebPBitstreamFeatures input;

        /// WebPDecBuffer->Anonymous_5c438b36_7de6_498e_934a_d3613b37f5fc
        public WebPDecBuffer output;

        /// WebPDecoderOptions->Anonymous_78066979_3e1e_4d74_aee5_f316b20e3385
        public WebPDecoderOptions options;
    }


    public partial class NativeMethods
    {

        /// Return Type: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPGetDecoderVersion", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPGetDecoderVersion();




        /// <summary>
        /// Retrieve basic header information: width, height.
        /// This function will also validate the header and return 0 in
        /// case of formatting error.
        /// Pointers 'width' and 'height' can be passed NULL if deemed irrelevant.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="data_size"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        [DllImportAttribute("libwebp", EntryPoint = "WebPGetInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPGetInfo([InAttribute()] IntPtr data, UIntPtr data_size, ref int width, ref int height);


        /// Return Type: uint8_t*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///width: int*
        ///height: int*
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecodeRGBA", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPDecodeRGBA([InAttribute()] IntPtr data, UIntPtr data_size, ref int width, ref int height);


        /// Return Type: uint8_t*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///width: int*
        ///height: int*
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecodeARGB", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPDecodeARGB([InAttribute()] IntPtr data, UIntPtr data_size, ref int width, ref int height);


        /// Return Type: uint8_t*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///width: int*
        ///height: int*
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecodeBGRA", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPDecodeBGRA([InAttribute()] IntPtr data, UIntPtr data_size, ref int width, ref int height);


        /// Return Type: uint8_t*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///width: int*
        ///height: int*
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecodeRGB", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPDecodeRGB([InAttribute()] IntPtr data, UIntPtr data_size, ref int width, ref int height);


        /// Return Type: uint8_t*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///width: int*
        ///height: int*
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecodeBGR", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPDecodeBGR([InAttribute()] IntPtr data, UIntPtr data_size, ref int width, ref int height);


        /// Return Type: uint8_t*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///width: int*
        ///height: int*
        ///u: uint8_t**
        ///v: uint8_t**
        ///stride: int*
        ///uv_stride: int*
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecodeYUV", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPDecodeYUV([InAttribute()] IntPtr data, UIntPtr data_size, ref int width, ref int height, ref IntPtr u, ref IntPtr v, ref int stride, ref int uv_stride);


        /// Return Type: uint8_t*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///output_buffer: uint8_t*
        ///output_buffer_size: size_t->unsigned int
        ///output_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecodeRGBAInto", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPDecodeRGBAInto([InAttribute()] IntPtr data, UIntPtr data_size, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride);


        /// Return Type: uint8_t*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///output_buffer: uint8_t*
        ///output_buffer_size: size_t->unsigned int
        ///output_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecodeARGBInto", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPDecodeARGBInto([InAttribute()] IntPtr data, UIntPtr data_size, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride);


        /// Return Type: uint8_t*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///output_buffer: uint8_t*
        ///output_buffer_size: size_t->unsigned int
        ///output_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecodeBGRAInto", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPDecodeBGRAInto([InAttribute()] IntPtr data, UIntPtr data_size, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride);


        /// Return Type: uint8_t*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///output_buffer: uint8_t*
        ///output_buffer_size: size_t->unsigned int
        ///output_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecodeRGBInto", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPDecodeRGBInto([InAttribute()] IntPtr data, UIntPtr data_size, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride);


        /// Return Type: uint8_t*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///output_buffer: uint8_t*
        ///output_buffer_size: size_t->unsigned int
        ///output_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecodeBGRInto", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPDecodeBGRInto([InAttribute()] IntPtr data, UIntPtr data_size, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride);


        /// Return Type: uint8_t*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///luma: uint8_t*
        ///luma_size: size_t->unsigned int
        ///luma_stride: int
        ///u: uint8_t*
        ///u_size: size_t->unsigned int
        ///u_stride: int
        ///v: uint8_t*
        ///v_size: size_t->unsigned int
        ///v_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecodeYUVInto", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPDecodeYUVInto([InAttribute()] IntPtr data, UIntPtr data_size, IntPtr luma, UIntPtr luma_size, int luma_stride, IntPtr u, UIntPtr u_size, int u_stride, IntPtr v, UIntPtr v_size, int v_stride);


        /// Return Type: int
        ///param0: WebPDecBuffer*
        ///param1: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPInitDecBufferInternal", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPInitDecBufferInternal(ref WebPDecBuffer param0, int param1);


        /// Return Type: void
        ///buffer: WebPDecBuffer*
        [DllImportAttribute("libwebp", EntryPoint = "WebPFreeDecBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WebPFreeDecBuffer(ref WebPDecBuffer buffer);


        /// Return Type: WebPIDecoder*
        ///output_buffer: WebPDecBuffer*
        [DllImportAttribute("libwebp", EntryPoint = "WebPINewDecoder", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPINewDecoder(ref WebPDecBuffer output_buffer);


        /// Return Type: WebPIDecoder*
        ///csp: WEBP_CSP_MODE->Anonymous_cb136f5b_1d5d_49a0_aca4_656a79e9d159
        ///output_buffer: uint8_t*
        ///output_buffer_size: size_t->unsigned int
        ///output_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPINewRGB", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPINewRGB(WEBP_CSP_MODE csp, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride);


        /// Return Type: WebPIDecoder*
        ///luma: uint8_t*
        ///luma_size: size_t->unsigned int
        ///luma_stride: int
        ///u: uint8_t*
        ///u_size: size_t->unsigned int
        ///u_stride: int
        ///v: uint8_t*
        ///v_size: size_t->unsigned int
        ///v_stride: int
        ///a: uint8_t*
        ///a_size: size_t->unsigned int
        ///a_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPINewYUVA", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPINewYUVA(IntPtr luma, UIntPtr luma_size, int luma_stride, IntPtr u, UIntPtr u_size, int u_stride, IntPtr v, UIntPtr v_size, int v_stride, IntPtr a, UIntPtr a_size, int a_stride);


        /// Return Type: WebPIDecoder*
        ///luma: uint8_t*
        ///luma_size: size_t->unsigned int
        ///luma_stride: int
        ///u: uint8_t*
        ///u_size: size_t->unsigned int
        ///u_stride: int
        ///v: uint8_t*
        ///v_size: size_t->unsigned int
        ///v_stride: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPINewYUV", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPINewYUV(IntPtr luma, UIntPtr luma_size, int luma_stride, IntPtr u, UIntPtr u_size, int u_stride, IntPtr v, UIntPtr v_size, int v_stride);


        /// Return Type: void
        ///idec: WebPIDecoder*
        [DllImportAttribute("libwebp", EntryPoint = "WebPIDelete", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WebPIDelete(ref WebPIDecoder idec);


        /// Return Type: VP8StatusCode->Anonymous_b244cc15_fbc7_4c41_8884_71fe4f515cd6
        ///idec: WebPIDecoder*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        [DllImportAttribute("libwebp", EntryPoint = "WebPIAppend", CallingConvention = CallingConvention.Cdecl)]
        public static extern VP8StatusCode WebPIAppend(ref WebPIDecoder idec, [InAttribute()] IntPtr data, UIntPtr data_size);


        /// Return Type: VP8StatusCode->Anonymous_b244cc15_fbc7_4c41_8884_71fe4f515cd6
        ///idec: WebPIDecoder*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        [DllImportAttribute("libwebp", EntryPoint = "WebPIUpdate", CallingConvention = CallingConvention.Cdecl)]
        public static extern VP8StatusCode WebPIUpdate(ref WebPIDecoder idec, [InAttribute()] IntPtr data, UIntPtr data_size);


        /// Return Type: uint8_t*
        ///idec: WebPIDecoder*
        ///last_y: int*
        ///width: int*
        ///height: int*
        ///stride: int*
        [DllImportAttribute("libwebp", EntryPoint = "WebPIDecGetRGB", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPIDecGetRGB(ref WebPIDecoder idec, ref int last_y, ref int width, ref int height, ref int stride);


        /// Return Type: uint8_t*
        ///idec: WebPIDecoder*
        ///last_y: int*
        ///u: uint8_t**
        ///v: uint8_t**
        ///a: uint8_t**
        ///width: int*
        ///height: int*
        ///stride: int*
        ///uv_stride: int*
        ///a_stride: int*
        [DllImportAttribute("libwebp", EntryPoint = "WebPIDecGetYUVA", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPIDecGetYUVA(ref WebPIDecoder idec, ref int last_y, ref IntPtr u, ref IntPtr v, ref IntPtr a, ref int width, ref int height, ref int stride, ref int uv_stride, ref int a_stride);


        /// Return Type: WebPDecBuffer*
        ///idec: WebPIDecoder*
        ///left: int*
        ///top: int*
        ///width: int*
        ///height: int*
        [DllImportAttribute("libwebp", EntryPoint = "WebPIDecodedArea", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPIDecodedArea(ref WebPIDecoder idec, ref int left, ref int top, ref int width, ref int height);


        /// Return Type: VP8StatusCode->Anonymous_b244cc15_fbc7_4c41_8884_71fe4f515cd6
        ///param0: uint8_t*
        ///param1: size_t->unsigned int
        ///param2: WebPBitstreamFeatures*
        ///param3: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPGetFeaturesInternal", CallingConvention = CallingConvention.Cdecl)]
        public static extern VP8StatusCode WebPGetFeaturesInternal([InAttribute()] IntPtr param0, UIntPtr param1, ref WebPBitstreamFeatures param2, int param3);


        /// Return Type: int
        ///param0: WebPDecoderConfig*
        ///param1: int
        [DllImportAttribute("libwebp", EntryPoint = "WebPInitDecoderConfigInternal", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPInitDecoderConfigInternal(ref WebPDecoderConfig param0, int param1);


        /// Return Type: WebPIDecoder*
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///config: WebPDecoderConfig*
        [DllImportAttribute("libwebp", EntryPoint = "WebPIDecode", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebPIDecode([InAttribute()] IntPtr data, UIntPtr data_size, ref WebPDecoderConfig config);


        /// Return Type: VP8StatusCode->Anonymous_b244cc15_fbc7_4c41_8884_71fe4f515cd6
        ///data: uint8_t*
        ///data_size: size_t->unsigned int
        ///config: WebPDecoderConfig*
        [DllImportAttribute("libwebp", EntryPoint = "WebPDecode", CallingConvention = CallingConvention.Cdecl)]
        public static extern VP8StatusCode WebPDecode([InAttribute()] IntPtr data, UIntPtr data_size, ref WebPDecoderConfig config);

    }


}

#pragma warning restore 1591