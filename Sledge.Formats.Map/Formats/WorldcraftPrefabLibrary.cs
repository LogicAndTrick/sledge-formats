using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Formats
{
    public class WorldcraftPrefabLibrary
    {
        private static readonly string PrefabLibraryHeader = "Worldcraft Prefab Library\r\n" + (char)0x1A;
        private const float Version = 0.1f;

        public string Description { get; set; }
        public List<Prefab> Prefabs { get; set; }

        public static WorldcraftPrefabLibrary FromFile(string file)
        {
            using (var stream = File.OpenRead(file))
            {
                return new WorldcraftPrefabLibrary(stream);
            }
        }

        public WorldcraftPrefabLibrary()
        {
            Description = "";
            Prefabs = new List<Prefab>();
        }

        public WorldcraftPrefabLibrary(Stream stream)
        {
            Prefabs = new List<Prefab>();
            using (var br = new BinaryReader(stream))
            {
                var header = br.ReadFixedLengthString(Encoding.ASCII, PrefabLibraryHeader.Length);
                Util.Assert(header == PrefabLibraryHeader, $"Incorrect prefab library header. Expected '{PrefabLibraryHeader}', got '{header}'.");

                var version = br.ReadSingle();
                Util.Assert(Math.Abs(version - Version) < 0.01, $"Unsupported prefab library version number. Expected {Version}, got {version}.");

                var rmf = new WorldcraftRmfFormat();

                var offset = br.ReadUInt32();
                var num = br.ReadUInt32();
                Description = br.ReadFixedLengthString(Encoding.ASCII, 500);

                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                for (var i = 0; i < num; i++)
                {
                    var objOffset = br.ReadUInt32();
                    var objLength = br.ReadUInt32();
                    var name = br.ReadFixedLengthString(Encoding.ASCII, 31);
                    var desc = br.ReadFixedLengthString(Encoding.ASCII, 500);
                    br.ReadBytes(5); // unknown/padding

                    using (var substream = new SubStream(br.BaseStream, objOffset, objLength))
                    {
                        Prefabs.Add(new Prefab(name, desc, rmf.Read(substream)));
                    }
                }
            }
        }

        public void WriteToFile(string file)
        {
            using (var stream = File.OpenWrite(file))
            {
                Write(stream);
            }
        }

        public void Write(Stream stream)
        {
            if (!stream.CanSeek) throw new ArgumentException("stream must be seekable.", nameof(stream));
            using (var bw = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                var rmf = new WorldcraftRmfFormat();

                var offsets = new List<(uint offs, uint len)>();

                bw.WriteFixedLengthString(Encoding.ASCII, PrefabLibraryHeader.Length, PrefabLibraryHeader);
                bw.Write(Version);
                var infoOffsetPosition = stream.Position;
                bw.Write(0); // offset, write this at the end
                bw.Write(Prefabs.Count);
                bw.WriteFixedLengthString(Encoding.ASCII, 500, Description);
                bw.Write(0); // unknown, worldcraft always has 4 extra bytes after the description

                // write all the map data
                foreach (var prefab in Prefabs)
                {
                    var pos = (uint) stream.Position;
                    rmf.Write(stream, prefab.Map, null);
                    var len = (uint) stream.Position - pos;
                    offsets.Add((pos, len));
                }

                // now go back and write the index offset to the header
                var startOffset = (uint) stream.Position;
                stream.Seek(infoOffsetPosition, SeekOrigin.Begin);
                bw.Write(startOffset);
                stream.Seek(startOffset, SeekOrigin.Begin);

                // now write the index
                for (var i = 0; i < Prefabs.Count; i++)
                {
                    var prefab = Prefabs[i];
                    var (offs, len) = offsets[i];

                    bw.Write(offs);
                    bw.Write(len);
                    bw.WriteFixedLengthString(Encoding.ASCII, 31, prefab.Name);
                    bw.WriteFixedLengthString(Encoding.ASCII, 500, prefab.Description);
                    bw.Write(new byte[5]); // unknown/padding
                }

            }
		}
	}
}
