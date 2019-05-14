using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Sledge.Formats.Texture.Vtf
{
    public class VtfImageFormatInfo
    {
        public delegate void TransformPixel(byte[] data, uint offset, uint count);

        public VtfImageFormat Format { get; }

        public uint BitsPerPixel { get; }
        public uint BytesPerPixel { get; }

        public uint RedBitsPerPixel { get; }
        public uint GreenBitsPerPixel { get; }
        public uint BlueBitsPerPixel { get; }
        public uint AlphaBitsPerPixel { get; }

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

        private VtfImageFormatInfo(
            VtfImageFormat format,
            uint bitsPerPixel, uint bytesPerPixel,
            uint redBitsPerPixel, uint greenBitsPerPixel, uint blueBitsPerPixel, uint alphaBitsPerPixel,
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
                var masks = new Mask[] {
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

        private static int GetMipSize(int input, int level)
        {
            var res = input >> level;
            if (res < 1) res = 1;
            return res;
        }

        public uint ComputeMipmapSize(int width, int height, int depth, int mipLevel)
        {
            var w = GetMipSize(width, mipLevel);
            var h = GetMipSize(height, mipLevel);
            var d = GetMipSize(depth, mipLevel);
            return ComputeImageSize((uint)w, (uint)h, (uint)d);
        }

        public uint ComputeImageSize(uint width, uint height, uint depth)
        {
            switch (Format)
            {
                case VtfImageFormat.Dxt1:
                case VtfImageFormat.Dxt1Onebitalpha:
                    if (width < 4 && width > 0) width = 4;
                    if (height < 4 && height > 0) height = 4;
                    return ((width + 3) / 4) * ((height + 3) / 4) * 8 * depth;
                case VtfImageFormat.Dxt3:
                case VtfImageFormat.Dxt5:
                    if (width < 4 && width > 0) width = 4;
                    if (height < 4 && height > 0) height = 4;
                    return ((width + 3) / 4) * ((height + 3) / 4) * 16 * depth;
                default:
                    return width * height * depth * BytesPerPixel;
            }
        }

        public uint ComputeImageSize(uint width, uint height, uint depth, uint numMipmaps)
        {
            uint uiImageSize = 0;

            for (var i = 0; i < numMipmaps; i++)
            {
                uiImageSize += ComputeImageSize(width, height, depth);

                width >>= 1;
                height >>= 1;
                depth >>= 1;

                if (width < 1) width = 1;
                if (height < 1) height = 1;
                if (depth < 1) depth = 1;
            }

            return uiImageSize;
        }

        public byte[] Read(BinaryReader br, uint width, uint height)
        {
            var buffer = new byte[width * height * 4];

            // No format, return blank array
            if (Format == VtfImageFormat.None) return buffer;

            // This is the exact format we want, take the fast path
            else if (Format == VtfImageFormat.Bgra8888)
            {
                br.Read(buffer, 0, buffer.Length);
                return buffer;
            }

            // Handle compressed formats
            else if (IsCompressed)
            {
                switch (Format)
                {
                    case VtfImageFormat.Dxt1:
                    case VtfImageFormat.Dxt1Onebitalpha:
                        DxtFormat.DecompressDxt1(buffer, br, width, height);
                        break;
                    case VtfImageFormat.Dxt3:
                        DxtFormat.DecompressDxt3(buffer, br, width, height);
                        break;
                    case VtfImageFormat.Dxt5:
                        DxtFormat.DecompressDxt5(buffer, br, width, height);
                        break;
                    default:
                        throw new NotImplementedException($"Unsupported format: {Format}");
                }
            }

            // Handle simple byte-aligned data
            else if (_is8Aligned)
            {
                var bytes = br.ReadBytes((int)(width * height * BytesPerPixel));
                for (uint i = 0, j = 0; i < bytes.Length; i += BytesPerPixel, j += 4)
                {
                    buffer[j + 0] = BlueIndex >= 0 ? bytes[i + BlueIndex] : (byte)0; // b
                    buffer[j + 1] = GreenIndex >= 0 ? bytes[i + GreenIndex] : (byte)0; // g
                    buffer[j + 2] = RedIndex >= 0 ? bytes[i + RedIndex] : (byte)0; // r
                    buffer[j + 3] = AlphaIndex >= 0 ? bytes[i + AlphaIndex] : (byte)255; // a
                    _pixelTransform?.Invoke(buffer, j, 4);
                }
            }

            // Special logic for half-precision HDR format
            else if (Format == VtfImageFormat.Rgba16161616F)
            {
                var logAverageLuminance = 0.0f;

                var bytes = br.ReadBytes((int)(width * height * BytesPerPixel));
                var shorts = new ushort[bytes.Length / 2];
                for (uint i = 0, j = 0; i < bytes.Length; i += BytesPerPixel, j += 4)
                {
                    for (var k = 0; k < 4; k++)
                    {
                        shorts[j + k] = BitConverter.ToUInt16(bytes, (int) (i + k * 2));
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
                var bytes = br.ReadBytes((int)(width * height * BytesPerPixel));
                for (int i = 0, j = 0; i < bytes.Length; i += (int) BytesPerPixel, j += 4)
                {
                    var b = BlueIndex  >= 0 ? BitConverter.ToUInt16(bytes, i + BlueIndex  * 2) : UInt16.MinValue;
                    var g = GreenIndex >= 0 ? BitConverter.ToUInt16(bytes, i + GreenIndex * 2) : UInt16.MinValue;
                    var r = RedIndex   >= 0 ? BitConverter.ToUInt16(bytes, i + RedIndex   * 2) : UInt16.MinValue;
                    var a = AlphaIndex >= 0 ? BitConverter.ToUInt16(bytes, i + AlphaIndex * 2) : UInt16.MaxValue;

                    buffer[j + 0] = (byte) (b >> 8);
                    buffer[j + 1] = (byte) (g >> 8);
                    buffer[j + 2] = (byte) (r >> 8);
                    buffer[j + 3] = (byte) (a >> 8);

                    _pixelTransform?.Invoke(buffer, (uint) j, 4);
                }
            }

            // Handle custom-aligned data that fits into a uint
            else if (BitsPerPixel <= 32)
            {
                var bytes = br.ReadBytes((int)(width * height * BytesPerPixel));
                for (uint i = 0, j = 0; i < bytes.Length; i += BytesPerPixel, j += 4)
                {
                    var val = 0u;
                    for (var k = (int) BytesPerPixel - 1; k >= 0; k--)
                    {
                        val = val << 8;
                        val |= bytes[i + k];
                    }
                    buffer[j + 0] = _masks[0].Apply(val, BitsPerPixel);
                    buffer[j + 1] = _masks[1].Apply(val, BitsPerPixel);
                    buffer[j + 2] = _masks[2].Apply(val, BitsPerPixel);
                    buffer[j + 3] = _masks[3].Apply(val, BitsPerPixel);
                }
            }
            switch (Format)
            {
                case VtfImageFormat.R32F:
                    break;
                case VtfImageFormat.Rgb323232F:
                    break;
                case VtfImageFormat.Rgba32323232F:
                    break;
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
                return (ushort)sValue;
            }
        }

        public static VtfImageFormatInfo FromFormat(VtfImageFormat imageFormat)
        {
            if (imageFormat == VtfImageFormat.None) return null;
            return ImageFormats[imageFormat];
        }

        private static readonly Dictionary<VtfImageFormat, VtfImageFormatInfo> ImageFormats = new Dictionary<VtfImageFormat, VtfImageFormatInfo>
        {
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

        private static void TransformBluescreen(byte[] bytes, uint index, uint count)
        {
            for (var i = index; i < index + count; i += 4)
            {
                if (bytes[i + 0] == byte.MaxValue && bytes[i + 1] == 0 && bytes[i + 2] == 0)
                {
                    bytes[i + 3] = 0;
                }
            }
        }

        private static void TransformLuminance(byte[] bytes, uint index, uint count)
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
                partial >>= (bits - dest);
                b <<= dest;
                b |= partial;
            }
            return b;
        }

        private class Mask
        {
            public char Component { get; set; }
            public uint Size { get; set; }
            public int Index { get; set; }
            public uint Offset { get; set; }
            private uint Bitmask => ~0u >> (32 - (int) Size);

            public Mask(char component, uint size, int index)
            {
                Component = component;
                Size = size;
                Index = index;
            }

            public byte Apply(uint value, uint bitsPerPixel)
            {
                if (Index < 0) return Component == 'a' ? Byte.MaxValue : Byte.MinValue;
                var im = value >> (int) (bitsPerPixel - Offset - Size);
                im &= Bitmask;
                return PartialToByte((byte) im, (int) Size);
            }
        }
    }
}