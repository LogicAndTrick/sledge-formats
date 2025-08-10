using Sledge.Formats.Map.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sledge.Formats.Map.Formats
{
    public class JackhammerPrefabLibrary
    {
        private const int Version = 1;

        public string Description { get; set; }
        public List<Prefab> Prefabs { get; set; }

        public static JackhammerPrefabLibrary FromFile(string file)
        {
            using (var stream = File.OpenRead(file))
            {
                return new JackhammerPrefabLibrary(stream);
            }
        }

        public JackhammerPrefabLibrary()
        {
            Description = "";
            Prefabs = new List<Prefab>();
        }

        public JackhammerPrefabLibrary(Stream stream)
        {
            Prefabs = new List<Prefab>();
            using (var br = new BinaryReader(stream))
            {
                var header = br.ReadFixedLengthString(Encoding.ASCII, 4);
                Util.Assert(header == "JHOL", $"Incorrect prefab library header. Expected 'JHOL', got '{header}'.");

                var version = br.ReadInt32();
                Util.Assert(version == Version, $"Unsupported prefab library version number. Expected {Version}, got {version}.");

                // read header data
                var descriptionStringNum = br.ReadInt32();
                _ = br.ReadInt32(); // unknown
                _ = br.ReadInt32(); // unknown
                var singleEntryLength = br.ReadInt32();
                var entryDataOffset = br.ReadInt32();
                var entryDataLength = br.ReadInt32();
                var mapDataOffset = br.ReadInt32();
                var mapDataLength = br.ReadInt32();
                var imageDataOffset = br.ReadInt32();
                var imageDataLength = br.ReadInt32();
                var stringDataOffset = br.ReadInt32();
                var stringDataLength = br.ReadInt32();

                // read strings
                br.BaseStream.Seek(stringDataOffset, SeekOrigin.Begin);

                var stringCount = br.ReadInt32();

                // for whatever reason the offset to each string is stored next, we don't need those
                br.BaseStream.Seek(stringCount * 4, SeekOrigin.Current);

                var strings = new List<string>
                {
                    "" // string numbers seem to be indexed from 1 so add a blank string for index 0
                };
                for (var i = 0; i < stringCount; i++)
                {
                    var len = br.ReadInt32();
                    strings.Add(br.ReadFixedLengthString(Encoding.ASCII, len));
                }

                Description = strings[descriptionStringNum];

                // read entries
                br.BaseStream.Seek(entryDataOffset, SeekOrigin.Begin);

                var entries = new List<Entry>();

                var numEntries = entryDataLength / singleEntryLength;
                for (var i = 0; i < numEntries; i++)
                {
                    var name = strings[br.ReadInt32()];
                    var desc = strings[br.ReadInt32()];
                    _ = br.ReadInt32(); // unknown
                    _ = br.ReadInt32(); // unknown
                    var entryMapOffset = br.ReadInt32();
                    var entryMapLength = br.ReadInt32();
                    var entryMapType = br.ReadFixedLengthString(Encoding.ASCII, 4);
                    var entryImageOffset = br.ReadInt32();
                    var entryImageLength = br.ReadInt32();
                    var entryImageType = br.ReadFixedLengthString(Encoding.ASCII, 4);

                    if (entryMapType != "JHMF")
                    {
                        throw new NotSupportedException("Unexpected non-JMF format in Jackhammer prefab library.");
                    }

                    // i suspect this is an origin or bounding box, but not sure
                    (_, _, _) = (br.ReadDouble(), br.ReadDouble(), br.ReadDouble());

                    var numWorldspawns = br.ReadInt32(); // ?? no idea, always seems to be 1
                    var numSolids = br.ReadInt32();
                    var numPointEnts = br.ReadInt32();
                    var numSolidEnts = br.ReadInt32();
                    _ = br.ReadInt32(); // unknown
                    _ = br.ReadInt32(); // unknown
                    _ = br.ReadInt32(); // unknown
                    var numUniqueTextures = br.ReadInt32();

                    entries.Add(new Entry
                    {
                        Name = name,
                        Description = desc,
                        MapOffset = entryMapOffset,
                        MapLength = entryMapLength,
                        MapType = entryMapType,
                        ImageOffset = entryImageOffset,
                        ImageLength = entryImageLength,
                        ImageType = entryImageType
                    });
                }

                // read maps and preview images
                var jmf = new JackhammerJmfFormat();

                foreach (var entry in entries)
                {
                    br.BaseStream.Seek(imageDataOffset + entry.ImageOffset, SeekOrigin.Begin);
                    var img = br.ReadBytes(entry.ImageLength);
                    using (var substream = new SubStream(br.BaseStream, mapDataOffset + entry.MapOffset, entry.MapLength))
                    {
                        Prefabs.Add(new Prefab(entry.Name, entry.Description, jmf.Read(substream))
                        {
                            PreviewImage = img
                        });
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
            throw new NotImplementedException();
        }

        // record class to hold an entry temporarily
        private class Entry
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int MapOffset { get; set; }
            public int MapLength { get; set; }
            public string MapType { get; set; }
            public int ImageOffset { get; set; }
            public int ImageLength { get; set; }
            public string ImageType { get; set; }
        }
    }
}
