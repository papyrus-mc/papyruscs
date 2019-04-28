using System;
using System.Drawing;
using System.IO;

/*

Decoder for Targa (.TGA) images.
Supports pretty much the full Targa specification (all bit
depths, etc).  At the very least, it decodes all TGA images that
I've found in the wild.  If you find one that it fails to decode,
let me know!

Copyright 2013-2016 Dmitry Brant
http://dmitrybrant.com

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

*/

/* File Modified by MJungnickel */

namespace DmitryBrant.ImageFormats
{
    /// <summary>
    /// Handles reading Targa (.TGA) images.
    /// </summary>
    public static class TgaReader
    {

        /// <summary>
        /// Reads a Targa (.TGA) image from a file.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <returns>Bitmap that contains the image that was read.</returns>
        public static Bitmap Load(string fileName)
        {
            Bitmap bmp = null;
            using (var f = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bmp = Load(f);
            }
            return bmp;
        }

        /// <summary>
        /// Reads a Targa (.TGA) image from a stream.
        /// </summary>
        /// <param name="stream">Stream from which to read the image.</param>
        /// <returns>Bitmap that contains the image that was read.</returns>
        /// 
        public static Bitmap Load(Stream stream)
        {
            Bitmap theBitmap = null;
            BinaryReader reader = new BinaryReader(stream);

            UInt32[] palette = null;
            byte[] scanline = null;

            byte idFieldLength = (byte)stream.ReadByte();
            byte colorMap = (byte)stream.ReadByte();
            byte imageType = (byte)stream.ReadByte();
            UInt16 colorMapOffset = LittleEndian(reader.ReadUInt16());
            UInt16 colorsUsed = LittleEndian(reader.ReadUInt16());
            byte bitsPerColorMap = (byte)stream.ReadByte();
            UInt16 xCoord = LittleEndian(reader.ReadUInt16());
            UInt16 yCoord = LittleEndian(reader.ReadUInt16());
            UInt16 imgWidth = LittleEndian(reader.ReadUInt16());
            UInt16 imgHeight = LittleEndian(reader.ReadUInt16());
            byte bitsPerPixel = (byte)stream.ReadByte();
            byte imgFlags = (byte)stream.ReadByte();

            if (colorMap > 1)
                throw new ApplicationException("This is not a valid TGA file.");

            if (idFieldLength > 0)
            {
                byte[] idBytes = new byte[idFieldLength];
                stream.Read(idBytes, 0, idFieldLength);
                string idStr = System.Text.Encoding.ASCII.GetString(idBytes);

                //do something with the ID string...
                System.Diagnostics.Debug.WriteLine("Targa image ID: " + idStr);
            }

            //image types:
            //0 - No Image Data Available
            //1 - Uncompressed Color Image
            //2 - Uncompressed RGB Image
            //3 - Uncompressed Black & White Image
            //9 - Compressed Color Image
            //10 - Compressed RGB Image
            //11 - Compressed Black & White Image

            if ((imageType > 11) || ((imageType > 3) && (imageType < 9)))
            {
                throw new ApplicationException("This image type (" + imageType + ") is not supported.");
            }
            else if (bitsPerPixel != 8 && bitsPerPixel != 15 && bitsPerPixel != 16 && bitsPerPixel != 24 && bitsPerPixel != 32)
            {
                throw new ApplicationException("Number of bits per pixel (" + bitsPerPixel + ") is not supported.");
            }
            if (colorMap > 0)
            {
                if (bitsPerColorMap != 15 && bitsPerColorMap != 16 && bitsPerColorMap != 24 && bitsPerColorMap != 32)
                {
                    throw new ApplicationException("Number of bits per color map (" + bitsPerPixel + ") is not supported.");
                }
            }

            byte[] bmpData = new byte[imgWidth * 4 * imgHeight];

            try
            {

                if (colorMap > 0)
                {
                    int paletteEntries = colorMapOffset + colorsUsed;
                    palette = new UInt32[paletteEntries];

                    if (bitsPerColorMap == 24)
                    {
                        for (int i = colorMapOffset; i < paletteEntries; i++)
                        {
                            palette[i] = 0xFF000000;
                            palette[i] |= (UInt32)(stream.ReadByte() << 16);
                            palette[i] |= (UInt32)(stream.ReadByte() << 8);
                            palette[i] |= (UInt32)(stream.ReadByte());
                        }
                    }
                    else if (bitsPerColorMap == 32)
                    {
                        for (int i = colorMapOffset; i < paletteEntries; i++)
                        {
                            palette[i] = 0xFF000000;
                            palette[i] |= (UInt32)(stream.ReadByte() << 16);
                            palette[i] |= (UInt32)(stream.ReadByte() << 8);
                            palette[i] |= (UInt32)(stream.ReadByte());
                            palette[i] |= (UInt32)(stream.ReadByte() << 24);
                        }
                    }
                    else if ((bitsPerColorMap == 15) || (bitsPerColorMap == 16))
                    {
                        int hi, lo;
                        for (int i = colorMapOffset; i < paletteEntries; i++)
                        {
                            hi = stream.ReadByte();
                            lo = stream.ReadByte();
                            palette[i] = 0xFF000000;
                            palette[i] |= (UInt32)((hi & 0x1F) << 3) << 16;
                            palette[i] |= (UInt32)((((lo & 0x3) << 3) + ((hi & 0xE0) >> 5)) << 3) << 8;
                            palette[i] |= (UInt32)(((lo & 0x7F) >> 2) << 3);
                        }
                    }
                }

                if (imageType == 1 || imageType == 2 || imageType == 3)
                {
                    scanline = new byte[imgWidth * (bitsPerPixel / 8)];
                    for (int y = imgHeight - 1; y >= 0; y--)
                    {
                        switch (bitsPerPixel)
                        {
                            case 8:
                                stream.Read(scanline, 0, scanline.Length);
                                if (imageType == 1)
                                {
                                    for (int x = 0; x < imgWidth; x++)
                                    {
                                        bmpData[4 * (y * imgWidth + x)] = (byte)((palette[scanline[x]] >> 16) & 0XFF);
                                        bmpData[4 * (y * imgWidth + x) + 1] = (byte)((palette[scanline[x]] >> 8) & 0XFF);
                                        bmpData[4 * (y * imgWidth + x) + 2] = (byte)((palette[scanline[x]]) & 0XFF);
                                        bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                                    }
                                }
                                else if (imageType == 3)
                                {
                                    for (int x = 0; x < imgWidth; x++)
                                    {
                                        bmpData[4 * (y * imgWidth + x)] = scanline[x];
                                        bmpData[4 * (y * imgWidth + x) + 1] = scanline[x];
                                        bmpData[4 * (y * imgWidth + x) + 2] = scanline[x];
                                        bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                                    }
                                }
                                break;
                            case 15:
                            case 16:
                                int hi, lo;
                                for (int x = 0; x < imgWidth; x++)
                                {
                                    hi = stream.ReadByte();
                                    lo = stream.ReadByte();

                                    bmpData[4 * (y * imgWidth + x)] = (byte)((hi & 0x1F) << 3);
                                    bmpData[4 * (y * imgWidth + x) + 1] = (byte)((((lo & 0x3) << 3) + ((hi & 0xE0) >> 5)) << 3);
                                    bmpData[4 * (y * imgWidth + x) + 2] = (byte)(((lo & 0x7F) >> 2) << 3);
                                    bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                                }
                                break;
                            case 24:
                                stream.Read(scanline, 0, scanline.Length);
                                for (int x = 0; x < imgWidth; x++)
                                {
                                    bmpData[4 * (y * imgWidth + x)] = scanline[x * 3];
                                    bmpData[4 * (y * imgWidth + x) + 1] = scanline[x * 3 + 1];
                                    bmpData[4 * (y * imgWidth + x) + 2] = scanline[x * 3 + 2];
                                    bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                                }
                                break;
                            case 32:
                                stream.Read(scanline, 0, scanline.Length);
                                for (int x = 0; x < imgWidth; x++)
                                {
                                    bmpData[4 * (y * imgWidth + x)] = scanline[x * 4];
                                    bmpData[4 * (y * imgWidth + x) + 1] = scanline[x * 4 + 1];
                                    bmpData[4 * (y * imgWidth + x) + 2] = scanline[x * 4 + 2];
                                    bmpData[4 * (y * imgWidth + x) + 3] = scanline[x * 4 + 3];
                                }
                                break;
                        }
                    }

                }
                else if (imageType == 9 || imageType == 10 || imageType == 11)
                {
                    int y = imgHeight - 1, x = 0, i;
                    int bytesPerPixel = bitsPerPixel / 8;
                    scanline = new byte[imgWidth * 4];

                    while (y >= 0 && stream.Position < stream.Length)
                    {
                        i = stream.ReadByte();
                        if (i < 128)
                        {
                            i++;
                            switch (bitsPerPixel)
                            {
                                case 8:
                                    stream.Read(scanline, 0, i * bytesPerPixel);
                                    if (imageType == 9)
                                    {
                                        for (int j = 0; j < i; j++)
                                        {
                                            bmpData[4 * (y * imgWidth + x)] = (byte)((palette[scanline[j]] >> 16) & 0XFF);
                                            bmpData[4 * (y * imgWidth + x) + 1] = (byte)((palette[scanline[j]] >> 8) & 0XFF);
                                            bmpData[4 * (y * imgWidth + x) + 2] = (byte)((palette[scanline[j]]) & 0XFF);
                                            bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                                            x++;
                                            if (x >= imgWidth) { x = 0; y--; }
                                        }
                                    }
                                    else if (imageType == 11)
                                    {
                                        for (int j = 0; j < i; j++)
                                        {
                                            bmpData[4 * (y * imgWidth + x)] = scanline[j];
                                            bmpData[4 * (y * imgWidth + x) + 1] = scanline[j];
                                            bmpData[4 * (y * imgWidth + x) + 2] = scanline[j];
                                            bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                                            x++;
                                            if (x >= imgWidth) { x = 0; y--; }
                                        }
                                    }
                                    break;
                                case 15:
                                case 16:
                                    int hi, lo;
                                    for (int j = 0; j < i; j++)
                                    {
                                        hi = stream.ReadByte();
                                        lo = stream.ReadByte();

                                        bmpData[4 * (y * imgWidth + x)] = (byte)((hi & 0x1F) << 3);
                                        bmpData[4 * (y * imgWidth + x) + 1] = (byte)((((lo & 0x3) << 3) + ((hi & 0xE0) >> 5)) << 3);
                                        bmpData[4 * (y * imgWidth + x) + 2] = (byte)(((lo & 0x7F) >> 2) << 3);
                                        bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                                        x++;
                                        if (x >= imgWidth) { x = 0; y--; }
                                    }
                                    break;
                                case 24:
                                    stream.Read(scanline, 0, i * bytesPerPixel);
                                    for (int j = 0; j < i; j++)
                                    {
                                        bmpData[4 * (y * imgWidth + x)] = scanline[j * 3];
                                        bmpData[4 * (y * imgWidth + x) + 1] = scanline[j * 3 + 1];
                                        bmpData[4 * (y * imgWidth + x) + 2] = scanline[j * 3 + 2];
                                        bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                                        x++;
                                        if (x >= imgWidth) { x = 0; y--; }
                                    }
                                    break;
                                case 32:
                                    stream.Read(scanline, 0, i * bytesPerPixel);
                                    for (int j = 0; j < i; j++)
                                    {
                                        bmpData[4 * (y * imgWidth + x)] = scanline[j * 4];
                                        bmpData[4 * (y * imgWidth + x) + 1] = scanline[j * 4 + 1];
                                        bmpData[4 * (y * imgWidth + x) + 2] = scanline[j * 4 + 2];
                                        bmpData[4 * (y * imgWidth + x) + 3] = scanline[j * 4 + 3];
                                        x++;
                                        if (x >= imgWidth) { x = 0; y--; }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            i = (i & 0x7F) + 1;
                            int r, g, b, a;

                            switch (bitsPerPixel)
                            {
                                case 8:
                                    int p = stream.ReadByte();
                                    if (imageType == 9)
                                    {
                                        for (int j = 0; j < i; j++)
                                        {
                                            bmpData[4 * (y * imgWidth + x)] = (byte)((palette[p] >> 16) & 0XFF);
                                            bmpData[4 * (y * imgWidth + x) + 1] = (byte)((palette[p] >> 8) & 0XFF);
                                            bmpData[4 * (y * imgWidth + x) + 2] = (byte)((palette[p]) & 0XFF);
                                            bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                                            x++;
                                            if (x >= imgWidth) { x = 0; y--; }
                                        }
                                    }
                                    else if (imageType == 11)
                                    {
                                        for (int j = 0; j < i; j++)
                                        {
                                            bmpData[4 * (y * imgWidth + x)] = (byte)p;
                                            bmpData[4 * (y * imgWidth + x) + 1] = (byte)p;
                                            bmpData[4 * (y * imgWidth + x) + 2] = (byte)p;
                                            bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                                            x++;
                                            if (x >= imgWidth) { x = 0; y--; }
                                        }
                                    }
                                    break;
                                case 15:
                                case 16:
                                    int hi = stream.ReadByte();
                                    int lo = stream.ReadByte();
                                    for (int j = 0; j < i; j++)
                                    {
                                        bmpData[4 * (y * imgWidth + x)] = (byte)((hi & 0x1F) << 3);
                                        bmpData[4 * (y * imgWidth + x) + 1] = (byte)((((lo & 0x3) << 3) + ((hi & 0xE0) >> 5)) << 3);
                                        bmpData[4 * (y * imgWidth + x) + 2] = (byte)(((lo & 0x7F) >> 2) << 3);
                                        bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                                        x++;
                                        if (x >= imgWidth) { x = 0; y--; }
                                    }
                                    break;
                                case 24:
                                    r = stream.ReadByte();
                                    g = stream.ReadByte();
                                    b = stream.ReadByte();
                                    for (int j = 0; j < i; j++)
                                    {
                                        bmpData[4 * (y * imgWidth + x)] = (byte)r;
                                        bmpData[4 * (y * imgWidth + x) + 1] = (byte)g;
                                        bmpData[4 * (y * imgWidth + x) + 2] = (byte)b;
                                        bmpData[4 * (y * imgWidth + x) + 3] = 0xFF;
                                        x++;
                                        if (x >= imgWidth) { x = 0; y--; }
                                    }
                                    break;
                                case 32:
                                    r = stream.ReadByte();
                                    g = stream.ReadByte();
                                    b = stream.ReadByte();
                                    a = stream.ReadByte();
                                    for (int j = 0; j < i; j++)
                                    {
                                        bmpData[4 * (y * imgWidth + x)] = (byte)r;
                                        bmpData[4 * (y * imgWidth + x) + 1] = (byte)g;
                                        bmpData[4 * (y * imgWidth + x) + 2] = (byte)b;
                                        bmpData[4 * (y * imgWidth + x) + 3] = (byte)a;
                                        x++;
                                        if (x >= imgWidth) { x = 0; y--; }
                                    }
                                    break;
                            }
                        }

                    }
                }

            }
            catch (Exception e)
            {
                //give a partial image in case of unexpected end-of-file

                System.Diagnostics.Debug.WriteLine("Error while processing TGA file: " + e.Message);
            }

            theBitmap = new Bitmap((int)imgWidth, (int)imgHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Imaging.BitmapData bmpBits = theBitmap.LockBits(new Rectangle(0, 0, theBitmap.Width, theBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Runtime.InteropServices.Marshal.Copy(bmpData, 0, bmpBits.Scan0, imgWidth * 4 * imgHeight);
            theBitmap.UnlockBits(bmpBits);

            int imgOrientation = (imgFlags >> 4) & 0x3;
            if (imgOrientation == 1)
                theBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            else if (imgOrientation == 2)
                theBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            else if(imgOrientation == 3)
                theBitmap.RotateFlip(RotateFlipType.RotateNoneFlipXY);

            return theBitmap;
        }


        private static UInt16 LittleEndian(UInt16 val)
        {
            if (BitConverter.IsLittleEndian) return val;
            return conv_endian(val);
        }
        private static UInt32 LittleEndian(UInt32 val)
        {
            if (BitConverter.IsLittleEndian) return val;
            return conv_endian(val);
        }

        private static UInt16 conv_endian(UInt16 val)
        {
            UInt16 temp;
            temp = (UInt16)(val << 8); temp &= 0xFF00; temp |= (UInt16)((val >> 8) & 0xFF);
            return temp;
        }
        private static UInt32 conv_endian(UInt32 val)
        {
            UInt32 temp = (val & 0x000000FF) << 24;
            temp |= (val & 0x0000FF00) << 8;
            temp |= (val & 0x00FF0000) >> 8;
            temp |= (val & 0xFF000000) >> 24;
            return (temp);
        }

    }
}
