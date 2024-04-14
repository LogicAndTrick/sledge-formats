using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Sledge.Formats.Texture.Vtf.Resources;
using Sledge.Formats.Valve;

namespace Sledge.Formats.Texture.Vtf
{
    public class VtfFile
    {
        private const string VtfHeader = "VTF";
        private const int MaxNumberOfResources = 32;

        public VtfHeader Header { get; set; }

        private readonly List<VtfResource> _resources;
        public IReadOnlyList<VtfResource> Resources => _resources;

        public VtfImage LowResImage { get; set; }

        private readonly List<VtfImage> _images;
        public IReadOnlyList<VtfImage> Images => _images;

        /// <summary>
        /// Create a blank VTF file
        /// </summary>
        /// <param name="version">The version to use. The version will be upgraded if you add features that require newer versions. The default version is 7.2, which is the lowest public version, and compatible with all Source engine versions.</param>
        public VtfFile(decimal version = 7.2m)
        {
            Header = new VtfHeader
            {
                Version = version,
                BumpmapScale = 1,
                Flags = VtfImageFlag.None,
                Reflectivity = Vector3.Zero
            };
            _resources = new List<VtfResource>();
            _images = new List<VtfImage>();
            LowResImage = null;
        }

        /// <summary>
        /// Load a vtf file from a stream.
        /// </summary>
        public VtfFile(Stream stream)
        {
            Header = new VtfHeader();
            _resources = new List<VtfResource>();
            _images = new List<VtfImage>();

            using (var br = new BinaryReader(stream))
            {
                var header = br.ReadFixedLengthString(Encoding.ASCII, 4);
                if (header != VtfHeader) throw new Exception("Invalid VTF header. Expected '" + VtfHeader + "', got '" + header + "'.");

                var v1 = br.ReadUInt32();
                var v2 = br.ReadUInt32();
                var version = v1 + (v2 / 10m); // e.g. 7.3

                if (version < 7.0m || version > 7.5m) throw new NotSupportedException($"Unsupported VTF version. Expected 7.0-7.5, got {version}.");

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
                }

                // align to multiple of 16
                if (br.BaseStream.Position % 16 != 0) br.BaseStream.Seek(16 - (br.BaseStream.Position % 16), SeekOrigin.Current);

                var faces = 1;
                if (Header.Flags.HasFlag(VtfImageFlag.EnvMap))
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
                        case VtfResourceType.Crc:
                        case VtfResourceType.TextureLodSettings:
                        case VtfResourceType.TextureSettingsEx:
                            _resources.Add(new VtfValueResource
                            {
                                Type = type,
                                Value = data
                            });
                            break;
                        case VtfResourceType.KeyValueData:
                            br.BaseStream.Position = data;
                            var kvLength = br.ReadInt32();
                            var kvString = br.ReadFixedLengthString(Encoding.ASCII, kvLength);

                            List<SerialisedObject> kvObjects;
                            using (var reader = new StringReader(kvString))
                            {
                                kvObjects = SerialisedObjectFormatter.Parse(reader).ToList();
                            }
                            if (kvObjects.Count != 1) throw new NotSupportedException("More than one object found in the keyvalue resource. This is not supported.");

                            _resources.Add(new VtfKeyValueResource
                            {
                                Type = type,
                                KeyValues = kvObjects.First()
                            });
                            break;
                        case VtfResourceType.Sheet:
                            // todo
                            _resources.Add(new VtfValueResource()
                            {
                                Type = type,
                                Value = data // actually a data pointer
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

                                _images.Add(new VtfImage
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

        /// <summary>
        /// Perform some basic validation checks on the file and return any issues that exist.
        /// </summary>
        public IEnumerable<string> Validate()
        {
            var largestImage = _images.OrderByDescending(x => x.Width * (long)x.Height).FirstOrDefault();
            foreach (var image in _images)
            {
                // check width & height for power of 2
                if (image.Width <= 0 || (image.Width & (image.Width - 1)) != 0) yield return $"Error: image width of {image.Width} is not a power of 2.";
                if (image.Height <= 0 || (image.Height & (image.Height - 1)) != 0) yield return $"Error: image height of {image.Height} is not a power of 2.";
                if (largestImage != null && image.Format != largestImage.Format) yield return $"Error: expected image format is {largestImage.Format}, but instead got {image.Format}.";
                if (image.Format == VtfImageFormat.None) yield return "Error: `None` is not a valid image format.";
            }

            var numMipmaps = _images.Select(x => x.Mipmap).Distinct().Count();
            var numFrames = _images.Select(x => x.Frame).Distinct().Count();
            var numFaces = _images.Select(x => x.Face).Distinct().Count();
            var numSlices = _images.Select(x => x.Slice).Distinct().Count();

            if (numFaces > 1)
            {
                if (!Header.Flags.HasFlag(VtfImageFlag.EnvMap)) yield return "Error: only environment maps support multiple faces, but the `EnvMap` flag is not set on the texture.";
                if (numFaces != 6 && numFaces != 7) yield return "Error: when using multiple faces, there must be exactly 6 or 7 of them.";
            }

            for (var mi = 0; mi < numMipmaps; mi++)
            {
                for (var fr = 0; fr < numFrames; fr++)
                {
                    for (var fa = 0; fa < numFaces; fa++)
                    {
                        for (var sl = 0; sl < numSlices; sl++)
                        {
                            var img = _images.FirstOrDefault(x => x.Mipmap == mi && x.Frame == fr && x.Face == fa && x.Slice == sl);
                            if (img == null) yield return $"Error: missing image data for mipmap = {mi}, frame = {fr}, face = {fa}, slice = {sl}.";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Write the file to a stream. Validation will be performed first to ensure that the file is valid.
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        public void Write(Stream stream)
        {
            var results = Validate().ToList();
            if (results.Any()) throw new VtfValidationException(results);

            using (var bw = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                var numResources = _resources.Count + 1 + (LowResImage == null ? 0 : 1);
                var headerSize = 64;
                if (Header.Version >= 7.2m) headerSize = 80;
                if (Header.Version >= 7.3m) headerSize += numResources * 8;
                if (headerSize % 16 != 0) headerSize += 16 - (headerSize % 16); // align to 16

                var largestImage = _images.OrderByDescending(x => x.Width * (long)x.Height).FirstOrDefault();
                var numFrames = _images.Select(x => x.Frame).Distinct().Count();
                var numMipmaps = _images.Select(x => x.Mipmap).Distinct().Count();
                var numSlices = _images.Select(x => x.Slice).Distinct().Count();
                var firstFrame = _images.OrderBy(x => x.Frame).FirstOrDefault()?.Frame ?? 0;

                // write header
                bw.WriteFixedLengthString(Encoding.ASCII, 4, VtfHeader);
                bw.Write((uint)Math.Floor(Header.Version));
                bw.Write((uint)Math.Floor((Header.Version - Math.Floor(Header.Version)) * 10m));
                bw.Write(headerSize);
                bw.Write((ushort) (largestImage?.Width ?? 0));
                bw.Write((ushort) (largestImage?.Height ?? 0));
                bw.Write((uint) Header.Flags);
                bw.Write((ushort) numFrames);
                bw.Write((ushort) firstFrame);
                bw.Write(0); // padding
                bw.WriteVector3(Header.Reflectivity);
                bw.Write(0); // padding
                bw.Write(Header.BumpmapScale);
                bw.Write((uint) (largestImage?.Format ?? VtfImageFormat.None));
                bw.Write((byte) numMipmaps);
                bw.Write((uint) (LowResImage?.Format ?? VtfImageFormat.None));
                bw.Write((byte) (LowResImage?.Width ?? 0));
                bw.Write((byte) (LowResImage?.Height ?? 0));
                if (Header.Version >= 7.2m)
                {
                    bw.Write((ushort) numSlices);
                }
                if (Header.Version >= 7.3m)
                {
                    bw.Write(new byte[] { 0, 0, 0 }); // padding
                    bw.Write((uint) numResources);
                }

                // align to multiple of 16
                if (bw.BaseStream.Position % 16 != 0) bw.Write(new byte[16 - (bw.BaseStream.Position % 16)]);

                // zero out the resources for now
                var resourcesStartPos = bw.BaseStream.Position;
                if (Header.Version >= 7.3m)
                {
                    for (var i = 0; i < numResources; i++)
                    {
                        bw.Write(0L);
                    }
                }

                // align to multiple of 16 again
                if (bw.BaseStream.Position % 16 != 0) bw.Write(new byte[16 - (bw.BaseStream.Position % 16)]);

                var resourceData = new List<(VtfResourceType type, uint value)>();

                // write images
                if (LowResImage != null)
                {
                    resourceData.Add((VtfResourceType.LowResImage, (uint) bw.BaseStream.Position));
                    bw.Write(LowResImage.Data);
                }

                resourceData.Add((VtfResourceType.Image, (uint)bw.BaseStream.Position));
                foreach (var mipGroup in _images.GroupBy(x => x.Mipmap).OrderByDescending(x => x.Key))
                {
                    foreach (var frameGroup in mipGroup.GroupBy(x => x.Frame).OrderBy(x => x.Key))
                    {
                        foreach (var faceGroup in frameGroup.GroupBy(x => x.Face).OrderBy(x => x.Key))
                        {
                            foreach (var image in faceGroup.OrderBy(x => x.Slice))
                            {
                                bw.Write(image.Data);
                            }
                        }
                    }
                }

                // write resources
                foreach (var res in _resources)
                {
                    uint value;
                    switch (res)
                    {
                        case VtfKeyValueResource kvr:
                            value = (uint)bw.BaseStream.Position;
                            using (var ms = new MemoryStream())
                            {
                                using (var tw = new StreamWriter(ms))
                                {
                                    SerialisedObjectFormatter.Print(kvr.KeyValues, tw);
                                }

                                ms.Position = 0;
                                var arr = ms.ToArray();
                                bw.Write(arr.Length);
                                bw.Write(arr);
                            }
                            break;
                        case VtfValueResource vvr:
                            value = vvr.Value;
                            break;
                        case VtfUnknownResource vur:
                            value = (uint)bw.BaseStream.Position;
                            bw.Write(vur.Data);
                            break;
                        default:
                            throw new NotImplementedException($"Unknown resource type: {res.GetType().Name}");
                    }
                    resourceData.Add((res.Type, value));
                }

                // now go back and write the resource data to the header
                if (Header.Version >= 7.3m)
                {
                    var end = bw.BaseStream.Position;
                    bw.BaseStream.Position = resourcesStartPos;
                    foreach (var (type, value) in resourceData)
                    {
                        bw.Write((uint)type);
                        bw.Write(value);
                    }
                    bw.BaseStream.Position = end;
                }
            }
        }

        private static int GetMipSize(int input, int level)
        {
            var res = input >> level;
            if (res < 1) res = 1;
            return res;
        }

        /// <summary>
        /// Add an image to the file. If an image with the same properties exists, it will be replaced.
        /// </summary>
        /// <param name="image">The image to add.</param>
        public void AddImage(VtfImage image)
        {
            var matching = _images.FirstOrDefault(x => x.Face == image.Face && x.Frame == image.Frame && x.Mipmap == image.Mipmap && x.Slice == image.Slice);
            var existing = _images.FirstOrDefault(x => x != matching);
            if (existing != null && existing.Format != image.Format)
            {
                throw new Exception($"All images must have the same format. Expected {existing.Format}, but got {image.Format}");
            }

            if (image.Slice != 1 && Header.Version < 7.2m) Header.Version = 7.2m;

            _images.Add(image);
        }

        /// <summary>
        /// Remove an image from the file.
        /// </summary>
        public void RemoveImage(VtfImage image)
        {
            _images.Remove(image);
        }

        /// <summary>
        /// Add a resource to the file. The file will be upgraded to version 7.3, if required.
        /// </summary>
        /// <param name="resource">The resource to add. If the resource is of type sheet, crc, texture lod settings, or texture settings ex, the existing resource will be removed.</param>
        public void AddResource(VtfResource resource)
        {
            switch (resource.Type)
            {
                case VtfResourceType.LowResImage:
                case VtfResourceType.Image:
                    throw new InvalidOperationException("Images cannot be set by adding a resource. Use Images/LowResImage instead.");
                case VtfResourceType.Sheet:
                case VtfResourceType.Crc:
                case VtfResourceType.TextureLodSettings:
                case VtfResourceType.TextureSettingsEx:
                    // only allow one of these resource types
                    _resources.RemoveAll(x => x.Type == resource.Type);
                    break;
                case VtfResourceType.KeyValueData:
                    // can have as many of these as we want
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resource.Type));
            }

            if (_resources.Count > MaxNumberOfResources) throw new InvalidOperationException($"Cannot add more than {MaxNumberOfResources} resources to a file.");

            if (Header.Version < 7.3m) Header.Version = 7.3m;
            _resources.Add(resource);
        }

        /// <summary>
        /// Remove a resource f rom the file.
        /// </summary>
        public void RemoveResource(VtfResource resource)
        {
            _resources.Remove(resource);
        }
    }
}