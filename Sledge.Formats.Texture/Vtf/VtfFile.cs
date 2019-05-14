using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sledge.Formats.Texture.Vtf
{
    public class VtfFile
    {
        private const string VtfHeader = "VTF";

        public VtfHeader Header { get; set; }
        public List<VtfResource> Resources { get; set; }
        public VtfImage LowResImage { get; set; }
        public List<VtfImage> Images { get; set; }

        public VtfFile(Stream stream)
        {
            Header = new VtfHeader();
            Resources = new List<VtfResource>();
            Images = new List<VtfImage>();

            using (var br = new BinaryReader(stream))
            {
                var header = br.ReadFixedLengthString(Encoding.ASCII, 4);
                if (header != VtfHeader) throw new Exception("Invalid VTF header. Expected '" + VtfHeader + "', got '" + header + "'.");

                var v1 = br.ReadUInt32();
                var v2 = br.ReadUInt32();
                var version = v1 + (v2 / 10m); // e.g. 7.3
                Header.Version = version;

                var headerSize = br.ReadUInt32();
                var width = br.ReadUInt16();
                var height = br.ReadUInt16();

                Header.Flags = (VtfImageFlag) br.ReadUInt32();

                var numFrames = br.ReadUInt16();
                var firstFrame = br.ReadUInt16();

                br.ReadBytes(4); // padding

                Header.Reflectivity = br.ReadVector3();

                br.ReadBytes(4); // padding

                Header.BumpmapScale = br.ReadSingle();

                var highResImageFormat = (VtfImageFormat) br.ReadUInt32();
                var mipmapCount = br.ReadByte();
                var lowResImageFormat = (VtfImageFormat) br.ReadUInt32();
                var lowResWidth = br.ReadByte();
                var lowResHeight = br.ReadByte();

                ushort depth = 1;
                uint numResources = 0;

                if (version >= 7.2m)
                {
                    depth = br.ReadUInt16();
                }
                if (version >= 7.3m)
                {
                    br.ReadBytes(3);
                    numResources = br.ReadUInt32();
                    br.ReadBytes(8);
                }

                var faces = 1;
                if (Header.Flags.HasFlag(VtfImageFlag.Envmap))
                {
                    faces = version < 7.5m && firstFrame != 0xFFFF ? 7 : 6;
                }

                var highResFormatInfo = VtfImageFormatInfo.FromFormat(highResImageFormat);
                var lowResFormatInfo = VtfImageFormatInfo.FromFormat(lowResImageFormat);

                var thumbnailSize = lowResImageFormat == VtfImageFormat.None
                    ? 0
                    : lowResFormatInfo.GetSize(lowResWidth, lowResHeight);

                var thumbnailPos = headerSize;
                var dataPos = headerSize + thumbnailSize;

                for (var i = 0; i < numResources; i++)
                {
                    var type = (VtfResourceType) br.ReadUInt32();
                    var data = br.ReadUInt32();
                    switch (type)
                    {
                        case VtfResourceType.LowResImage:
                            // Low res image
                            thumbnailPos = data;
                            break;
                        case VtfResourceType.Image:
                            // Regular image
                            dataPos = data;
                            break;
                        case VtfResourceType.Sheet:
                        case VtfResourceType.Crc:
                        case VtfResourceType.TextureLodSettings:
                        case VtfResourceType.TextureSettingsEx:
                        case VtfResourceType.KeyValueData:
                            // todo
                            Resources.Add(new VtfResource
                            {
                                Type = type,
                                Data = data
                            });
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), (uint) type, "Unknown resource type");
                    }
                }

                if (lowResImageFormat != VtfImageFormat.None)
                {
                    br.BaseStream.Position = thumbnailPos;
                    var thumbSize = lowResFormatInfo.GetSize(lowResWidth, lowResHeight);
                    LowResImage = new VtfImage
                    {
                        Format = lowResImageFormat,
                        Width = lowResWidth,
                        Height = lowResHeight,
                        Data = br.ReadBytes(thumbSize)
                    };
                }

                br.BaseStream.Position = dataPos;
                for (var mip = mipmapCount - 1; mip >= 0; mip--)
                {
                    for (var frame = 0; frame < numFrames; frame++)
                    {
                        for (var face = 0; face < faces; face++)
                        {
                            for (var slice = 0; slice < depth; slice++)
                            {
                                var wid = GetMipSize(width, mip);
                                var hei = GetMipSize(height, mip);
                                var size = highResFormatInfo.GetSize(wid, hei);

                                Images.Add(new VtfImage
                                {
                                    Format = highResImageFormat,
                                    Width = wid,
                                    Height = hei,
                                    Mipmap = mip,
                                    Frame = frame,
                                    Face = face,
                                    Slice = slice,
                                    Data = br.ReadBytes(size)
                                });
                            }
                        }
                    }
                }
            }
        }

        private static int GetMipSize(int input, int level)
        {
            var res = input >> level;
            if (res < 1) res = 1;
            return res;
        }
    }
}