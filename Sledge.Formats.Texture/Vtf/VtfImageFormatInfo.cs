using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
// ReSharper disable MemberCanBePrivate.Global
namespace Sledge.Formats.Texture.Vtf
{
    // Uses logic from the excellent (LGPL-licensed) VtfLib, courtesy of Neil Jedrzejewski & Ryan Gregg
    public class VtfImageFormatInfo
    {
        private delegate void TransformPixel(byte[] data, int offset, int count);

        public VtfImageFormat Format { get; }

        public int BitsPerPixel { get; }
        public int BytesPerPixel { get; }

        public int RedBitsPerPixel { get; }
        public int GreenBitsPerPixel { get; }
        public int BlueBitsPerPixel { get; }
        public int AlphaBitsPerPixel { get; }

        public int RedIndex { get; }
        public int GreenIndex { get; }
        public int BlueIndex { get; }
        public int AlphaIndex { get; }

        public bool IsCompressed { get; }
        public bool IsSupported { get; }

        private readonly TransformPixel _pixelTransform;

        private readonly bool _is8Aligned;
        private readonly bool _is16Aligned;
        private readonly bool _is32Aligned;
        private readonly Mask[] _masks;

        public static VtfImageFormatInfo FromFormat(VtfImageFormat imageFormat) => ImageFormats[imageFormat];

        private VtfImageFormatInfo(
            VtfImageFormat format,
            int bitsPerPixel, int bytesPerPixel,
            int redBitsPerPixel, int greenBitsPerPixel, int blueBitsPerPixel, int alphaBitsPerPixel,
            int redIndex, int greenIndex, int blueIndex, int alphaIndex,
            bool isCompressed, bool isSupported,
            TransformPixel pixelTransform = null
            )
        {
            Format = format;

            BitsPerPixel = bitsPerPixel;
            BytesPerPixel = bytesPerPixel;

            RedBitsPerPixel = redBitsPerPixel;
            GreenBitsPerPixel = greenBitsPerPixel;
            BlueBitsPerPixel = blueBitsPerPixel;
            AlphaBitsPerPixel = alphaBitsPerPixel;

            RedIndex = redIndex;
            GreenIndex = greenIndex;
            BlueIndex = blueIndex;
            AlphaIndex = alphaIndex;

            IsCompressed = isCompressed;
            IsSupported = isSupported;

            _pixelTransform = pixelTransform;

            _is8Aligned = (redBitsPerPixel   == 0 || redBitsPerPixel   == 8) &&
                          (greenBitsPerPixel == 0 || greenBitsPerPixel == 8) &&
                          (blueBitsPerPixel  == 0 || blueBitsPerPixel  == 8) &&
                          (alphaBitsPerPixel == 0 || alphaBitsPerPixel == 8);

            _is16Aligned = (redBitsPerPixel   == 0 || redBitsPerPixel   == 16) &&
                           (greenBitsPerPixel == 0 || greenBitsPerPixel == 16) &&
                           (blueBitsPerPixel  == 0 || blueBitsPerPixel  == 16) &&
                           (alphaBitsPerPixel == 0 || alphaBitsPerPixel == 16);

            _is32Aligned = (redBitsPerPixel   == 0 || redBitsPerPixel   == 32) &&
                           (greenBitsPerPixel == 0 || greenBitsPerPixel == 32) &&
                           (blueBitsPerPixel  == 0 || blueBitsPerPixel  == 32) &&
                           (alphaBitsPerPixel == 0 || alphaBitsPerPixel == 32);

            if (!_is8Aligned && !_is16Aligned && !_is32Aligned)
            {
                var masks = new[] {
                    new Mask('r', redBitsPerPixel, redIndex),
                    new Mask('g', greenBitsPerPixel, greenIndex),
                    new Mask('b', blueBitsPerPixel, blueIndex),
                    new Mask('a', alphaBitsPerPixel, alphaIndex),
                }.OrderBy(x => x.Index).ToList();

                var offset = bitsPerPixel;
                foreach (var m in masks)
                {
                    offset -= m.Size;
                    m.Offset = offset;
                }

                var dict = masks.ToDictionary(x => x.Component, x => x);
                _masks = new[] { dict['b'], dict['g'], dict['r'], dict['a'] };
            }
        }

        /// <summary>
        /// Gets the size of the image data for this format in bytes
        /// </summary>
        /// <param name="width">The width of the image</param>
        /// <param name="height">The height of the image</param>
        /// <returns>The size of the image, in bytes</returns>
        public int GetSize(int width, int height)
        {
            switch (Format)
            {
                case VtfImageFormat.Dxt1:
                case VtfImageFormat.Dxt1Onebitalpha:
                    if (width < 4 && width > 0) width = 4;
                    if (height < 4 && height > 0) height = 4;
                    return (width + 3) / 4 * ((height + 3) / 4) * 8;
                case VtfImageFormat.Dxt3:
                case VtfImageFormat.Dxt5:
                    if (width < 4 && width > 0) width = 4;
                    if (height < 4 && height > 0) height = 4;
                    return (width + 3) / 4 * ((height + 3) / 4) * 16;
                default:
                    return width * height * BytesPerPixel;
            }
        }

        /// <summary>
        /// Convert an array of data in this format to a standard bgra8888 format.
        /// </summary>
        /// <param name="data">The data in this format</param>
        /// <param name="width">The width of the image</param>
        /// <param name="height">The height of the image</param>
        /// <returns>The data in bgra8888 format.</returns>
        public byte[] ConvertToBgra32(byte[] data, int width, int height)
        {
            var buffer = new byte[width * height * 4];

            // No format, return blank array
            if (Format == VtfImageFormat.None) return buffer;

            // This is the exact format we want, take the fast path
            else if (Format == VtfImageFormat.Bgra8888)
            {
                Array.Copy(data, buffer, buffer.Length);
                return buffer;
            }

            // Handle compressed formats
            else if (IsCompressed)
            {
                switch (Format)
                {
                    case VtfImageFormat.Dxt1:
                    case VtfImageFormat.Dxt1Onebitalpha:
                        DxtFormat.DecompressDxt1(buffer, data, width, height);
                        break;
                    case VtfImageFormat.Dxt3:
                        DxtFormat.DecompressDxt3(buffer, data, width, height);
                        break;
                    case VtfImageFormat.Dxt5:
                        DxtFormat.DecompressDxt5(buffer, data, width, height);
                        break;
                    default:
                        throw new NotImplementedException($"Unsupported format: {Format}");
                }
            }

            // Handle simple byte-aligned data
            else if (_is8Aligned)
            {
                for (int i = 0, j = 0; i < data.Length; i += BytesPerPixel, j += 4)
                {
                    buffer[j + 0] = BlueIndex  >= 0 ? data[i + BlueIndex ] : (byte) 0  ; // b
                    buffer[j + 1] = GreenIndex >= 0 ? data[i + GreenIndex] : (byte) 0  ; // g
                    buffer[j + 2] = RedIndex   >= 0 ? data[i + RedIndex  ] : (byte) 0  ; // r
                    buffer[j + 3] = AlphaIndex >= 0 ? data[i + AlphaIndex] : (byte) 255; // a
                    _pixelTransform?.Invoke(buffer, j, 4);
                }
            }

            // Special logic for half-precision HDR format
            else if (Format == VtfImageFormat.Rgba16161616F)
            {
                var logAverageLuminance = 0.0f;

                var shorts = new ushort[data.Length / 2];
                for (int i = 0, j = 0; i < data.Length; i += BytesPerPixel, j += 4)
                {
                    for (var k = 0; k < 4; k++)
                    {
                        shorts[j + k] = BitConverter.ToUInt16(data, i + k * 2);
                    }

                    var lum = shorts[j + 0] * 0.299f + shorts[j + 1] * 0.587f + shorts[j + 2] * 0.114f;
                    logAverageLuminance += (float) Math.Log(0.0000000001d + lum);
                }

                logAverageLuminance = (float) Math.Exp(logAverageLuminance / (width * height));

                for (var i = 0; i < shorts.Length; i += 4)
                {
                    TransformFp16(shorts, i, logAverageLuminance);

                    buffer[i + 2] = (byte)(shorts[i + 0] >> 8);
                    buffer[i + 1] = (byte)(shorts[i + 1] >> 8);
                    buffer[i + 0] = (byte)(shorts[i + 2] >> 8);
                    buffer[i + 3] = (byte)(shorts[i + 3] >> 8);
                }
            }

            // Handle short-aligned data
            else if (_is16Aligned)
            {
                for (int i = 0, j = 0; i < data.Length; i += BytesPerPixel, j += 4)
                {
                    var b = BlueIndex  >= 0 ? BitConverter.ToUInt16(data, i + BlueIndex  * 2) : UInt16.MinValue;
                    var g = GreenIndex >= 0 ? BitConverter.ToUInt16(data, i + GreenIndex * 2) : UInt16.MinValue;
                    var r = RedIndex   >= 0 ? BitConverter.ToUInt16(data, i + RedIndex   * 2) : UInt16.MinValue;
                    var a = AlphaIndex >= 0 ? BitConverter.ToUInt16(data, i + AlphaIndex * 2) : UInt16.MaxValue;

                    buffer[j + 0] = (byte) (b >> 8);
                    buffer[j + 1] = (byte) (g >> 8);
                    buffer[j + 2] = (byte) (r >> 8);
                    buffer[j + 3] = (byte) (a >> 8);

                    _pixelTransform?.Invoke(buffer, j, 4);
                }
            }

            // Handle custom-aligned data that fits into a uint
            else if (BitsPerPixel <= 32)
            {
                for (int i = 0, j = 0; i < data.Length; i += BytesPerPixel, j += 4)
                {
                    var val = 0u;
                    for (var k = BytesPerPixel - 1; k >= 0; k--)
                    {
                        val = val << 8;
                        val |= data[i + k];
                    }
                    buffer[j + 0] = _masks[0].Apply(val, BitsPerPixel);
                    buffer[j + 1] = _masks[1].Apply(val, BitsPerPixel);
                    buffer[j + 2] = _masks[2].Apply(val, BitsPerPixel);
                    buffer[j + 3] = _masks[3].Apply(val, BitsPerPixel);
                }
            }

            // Format not supported yet
            else
            {
                throw new NotImplementedException($"Unsupported format: {Format}");
            }

            return buffer;
        }

        private static void TransformFp16(ushort[] shorts, int offset, float logAverageLuminance)
        {
            const float fp16HdrKey = 4.0f;
            const float fp16HdrShift = 0.0f;
            const float fp16HdrGamma = 2.25f;

            float sR = shorts[offset + 0], sG = shorts[offset + 1], sB = shorts[offset + 2];

            var sY = sR * 0.299f + sG * 0.587f + sB * 0.114f;

            var sU = (sB - sY) * 0.565f;
            var sV = (sR - sY) * 0.713f;

            var sTemp = sY;

            sTemp = fp16HdrKey * sTemp / logAverageLuminance;
            sTemp = sTemp / (1.0f + sTemp);
            sTemp = sTemp / sY;

            shorts[offset + 0] = Clamp(Math.Pow((sY + 1.403f * sV) * sTemp + fp16HdrShift, fp16HdrGamma) * 65535.0f);
            shorts[offset + 1] = Clamp(Math.Pow((sY - 0.344f * sU - 0.714f * sV) * sTemp + fp16HdrShift, fp16HdrGamma) * 65535.0f);
            shorts[offset + 2] = Clamp(Math.Pow((sY + 1.770f * sU) * sTemp + fp16HdrShift, fp16HdrGamma) * 65535.0f);

            ushort Clamp(double sValue)
            {
                if (sValue < UInt16.MinValue) return UInt16.MinValue;
                if (sValue > UInt16.MaxValue) return UInt16.MaxValue;
                return (ushort) sValue;
            }
        }

        private static readonly Dictionary<VtfImageFormat, VtfImageFormatInfo> ImageFormats = new Dictionary<VtfImageFormat, VtfImageFormatInfo>
        {
            {VtfImageFormat.None, null},
            {VtfImageFormat.Rgba8888, new VtfImageFormatInfo(VtfImageFormat.Rgba8888, 32, 4, 8, 8, 8, 8, 0, 1, 2, 3, false, true)},
            {VtfImageFormat.Abgr8888, new VtfImageFormatInfo(VtfImageFormat.Abgr8888, 32, 4, 8, 8, 8, 8, 3, 2, 1, 0, false, true)},
            {VtfImageFormat.Rgb888, new VtfImageFormatInfo(VtfImageFormat.Rgb888, 24, 3, 8, 8, 8, 0, 0, 1, 2, -1, false, true)},
            {VtfImageFormat.Bgr888, new VtfImageFormatInfo(VtfImageFormat.Bgr888, 24, 3, 8, 8, 8, 0, 2, 1, 0, -1, false, true)},
            {VtfImageFormat.Rgb565, new VtfImageFormatInfo(VtfImageFormat.Rgb565, 16, 2, 5, 6, 5, 0, 0, 1, 2, -1, false, true)},
            {VtfImageFormat.I8, new VtfImageFormatInfo(VtfImageFormat.I8, 8, 1, 8, 8, 8, 0, 0, -1, -1, -1, false, true, TransformLuminance)},
            {VtfImageFormat.Ia88, new VtfImageFormatInfo(VtfImageFormat.Ia88, 16, 2, 8, 8, 8, 8, 0, -1, -1, 1, false, true, TransformLuminance)},
            {VtfImageFormat.P8, new VtfImageFormatInfo(VtfImageFormat.P8, 8, 1, 0, 0, 0, 0, -1, -1, -1, -1, false, false)},
            {VtfImageFormat.A8, new VtfImageFormatInfo(VtfImageFormat.A8, 8, 1, 0, 0, 0, 8, -1, -1, -1, 0, false, true)},
            {VtfImageFormat.Rgb888Bluescreen, new VtfImageFormatInfo(VtfImageFormat.Rgb888Bluescreen, 24, 3, 8, 8, 8, 8, 0, 1, 2, -1, false, true, TransformBluescreen)},
            {VtfImageFormat.Bgr888Bluescreen, new VtfImageFormatInfo(VtfImageFormat.Bgr888Bluescreen, 24, 3, 8, 8, 8, 8, 2, 1, 0, -1, false, true, TransformBluescreen)},
            {VtfImageFormat.Argb8888, new VtfImageFormatInfo(VtfImageFormat.Argb8888, 32, 4, 8, 8, 8, 8, 3, 0, 1, 2, false, true)},
            {VtfImageFormat.Bgra8888, new VtfImageFormatInfo(VtfImageFormat.Bgra8888, 32, 4, 8, 8, 8, 8, 2, 1, 0, 3, false, true)},
            {VtfImageFormat.Dxt1, new VtfImageFormatInfo(VtfImageFormat.Dxt1, 4, 0, 0, 0, 0, 0, -1, -1, -1, -1, true, true)},
            {VtfImageFormat.Dxt3, new VtfImageFormatInfo(VtfImageFormat.Dxt3, 8, 0, 0, 0, 0, 8, -1, -1, -1, -1, true, true)},
            {VtfImageFormat.Dxt5, new VtfImageFormatInfo(VtfImageFormat.Dxt5, 8, 0, 0, 0, 0, 8, -1, -1, -1, -1, true, true)},
            {VtfImageFormat.Bgrx8888, new VtfImageFormatInfo(VtfImageFormat.Bgrx8888, 32, 4, 8, 8, 8, 0, 2, 1, 0, -1, false, true)},
            {VtfImageFormat.Bgr565, new VtfImageFormatInfo(VtfImageFormat.Bgr565, 16, 2, 5, 6, 5, 0, 2, 1, 0, -1, false, true)},
            {VtfImageFormat.Bgrx5551, new VtfImageFormatInfo(VtfImageFormat.Bgrx5551, 16, 2, 5, 5, 5, 0, 2, 1, 0, -1, false, true)},
            {VtfImageFormat.Bgra4444, new VtfImageFormatInfo(VtfImageFormat.Bgra4444, 16, 2, 4, 4, 4, 4, 2, 1, 0, 3, false, true)},
            {VtfImageFormat.Dxt1Onebitalpha, new VtfImageFormatInfo(VtfImageFormat.Dxt1Onebitalpha, 4, 0, 0, 0, 0, 1, -1, -1, -1, -1, true, true)},
            {VtfImageFormat.Bgra5551, new VtfImageFormatInfo(VtfImageFormat.Bgra5551, 16, 2, 5, 5, 5, 1, 2, 1, 0, 3, false, true)},
            {VtfImageFormat.Uv88, new VtfImageFormatInfo(VtfImageFormat.Uv88, 16, 2, 8, 8, 0, 0, 0, 1, -1, -1, false, true)},
            {VtfImageFormat.Uvwq8888, new VtfImageFormatInfo(VtfImageFormat.Uvwq8888, 32, 4, 8, 8, 8, 8, 0, 1, 2, 3, false, true)},
            {VtfImageFormat.Rgba16161616F, new VtfImageFormatInfo(VtfImageFormat.Rgba16161616F, 64, 8, 16, 16, 16, 16, 0, 1, 2, 3, false, true)},
            {VtfImageFormat.Rgba16161616, new VtfImageFormatInfo(VtfImageFormat.Rgba16161616, 64, 8, 16, 16, 16, 16, 0, 1, 2, 3, false, true)},
            {VtfImageFormat.Uvlx8888, new VtfImageFormatInfo(VtfImageFormat.Uvlx8888, 32, 4, 8, 8, 8, 8, 0, 1, 2, 3, false, true)},
            {VtfImageFormat.R32F, new VtfImageFormatInfo(VtfImageFormat.R32F, 32, 4, 32, 0, 0, 0, 0, -1, -1, -1, false, false)},
            {VtfImageFormat.Rgb323232F, new VtfImageFormatInfo(VtfImageFormat.Rgb323232F, 96, 12, 32, 32, 32, 0, 0, 1, 2, -1, false, false)},
            {VtfImageFormat.Rgba32323232F, new VtfImageFormatInfo(VtfImageFormat.Rgba32323232F, 128, 16, 32, 32, 32, 32, 0, 1, 2, 3, false, false)},
            {VtfImageFormat.NvDst16, new VtfImageFormatInfo(VtfImageFormat.NvDst16, 16, 2, 16, 0, 0, 0, 0, -1, -1, -1, false, true)},
            {VtfImageFormat.NvDst24, new VtfImageFormatInfo(VtfImageFormat.NvDst24, 24, 3, 24, 0, 0, 0, 0, -1, -1, -1, false, true)},
            {VtfImageFormat.NvIntz, new VtfImageFormatInfo(VtfImageFormat.NvIntz, 32, 4, 0, 0, 0, 0, -1, -1, -1, -1, false, false)},
            {VtfImageFormat.NvRawz, new VtfImageFormatInfo(VtfImageFormat.NvRawz, 24, 3, 0, 0, 0, 0, -1, -1, -1, -1, false, false)},
            {VtfImageFormat.AtiDst16, new VtfImageFormatInfo(VtfImageFormat.AtiDst16, 16, 2, 16, 0, 0, 0, 0, -1, -1, -1, false, true)},
            {VtfImageFormat.AtiDst24, new VtfImageFormatInfo(VtfImageFormat.AtiDst24, 24, 3, 24, 0, 0, 0, 0, -1, -1, -1, false, true)},
            {VtfImageFormat.NvNull, new VtfImageFormatInfo(VtfImageFormat.NvNull, 32, 4, 0, 0, 0, 0, -1, -1, -1, -1, false, false)},
            {VtfImageFormat.Ati1N, new VtfImageFormatInfo(VtfImageFormat.Ati1N, 4, 0, 0, 0, 0, 0, -1, -1, -1, -1, true, false)},
            {VtfImageFormat.Ati2N, new VtfImageFormatInfo(VtfImageFormat.Ati2N, 8, 0, 0, 0, 0, 0, -1, -1, -1, -1, true, false)},
        };

        private static void TransformBluescreen(byte[] bytes, int index, int count)
        {
            for (var i = index; i < index + count; i += 4)
            {
                if (bytes[i + 0] == byte.MaxValue && bytes[i + 1] == 0 && bytes[i + 2] == 0)
                {
                    bytes[i + 3] = 0;
                }
            }
        }

        private static void TransformLuminance(byte[] bytes, int index, int count)
        {
            for (var i = index; i < index + count; i += 4)
            {
                bytes[i + 0] = bytes[i + 2];
                bytes[i + 1] = bytes[i + 2];
            }
        }

        private static byte PartialToByte(byte partial, int bits)
        {
            byte b = 0;
            var dest = 8;
            while (dest >= bits)
            {
                b <<= bits;
                b |= partial;
                dest -= bits;
            }
            if (dest != 0)
            {
                partial >>= bits - dest;
                b <<= dest;
                b |= partial;
            }
            return b;
        }

        private class Mask
        {
            public char Component { get; }
            public int Size { get; }
            public int Index { get; }
            public int Offset { get; set; }
            private uint Bitmask => ~0u >> (32 - Size);

            public Mask(char component, int size, int index)
            {
                Component = component;
                Size = size;
                Index = index;
            }

            public byte Apply(uint value, int bitsPerPixel)
            {
                if (Index < 0) return Component == 'a' ? Byte.MaxValue : Byte.MinValue;
                var im = value >> (bitsPerPixel - Offset - Size);
                im &= Bitmask;
                return PartialToByte((byte) im, Size);
            }
        }
    }
}