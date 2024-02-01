using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Formats
{
    public class WorldcraftPrefabLibrary
    {
        public string LibraryDescription { get; set; }
        public List<Prefab> Prefabs { get; set; }

        public static WorldcraftPrefabLibrary FromFile(string file)
        {
            using (var stream = File.OpenRead(file))
            {
                return new WorldcraftPrefabLibrary(stream);
            }
        }

        public WorldcraftPrefabLibrary(Stream stream)
        {
            Prefabs = new List<Prefab>();
            using (var br = new BinaryReader(stream))
            {
                var header = br.ReadFixedLengthString(Encoding.ASCII, 28);
                var prefabLibraryHeader = "Worldcraft Prefab Library\r\n" + (char)0x1A;
                Util.Assert(header == prefabLibraryHeader, $"Incorrect prefab library header. Expected '{prefabLibraryHeader}', got '{header}'.");

                var version = br.ReadSingle();
                Util.Assert(Math.Abs(version - 0.1f) < 0.01, $"Unsupported prefab library version number. Expected 0.1, got {version}.");

                var rmf = new WorldcraftRmfFormat();

                var offset = br.ReadUInt32();
                var num = br.ReadUInt32();
                LibraryDescription = br.ReadFixedLengthString(Encoding.ASCII, 500);

                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                for (var i = 0; i < num; i++)
                {
                    var objOffset = br.ReadUInt32();
                    var objLength = br.ReadUInt32();
                    var name = br.ReadFixedLengthString(Encoding.ASCII, 31);
                    var desc = br.ReadFixedLengthString(Encoding.ASCII, 205);
                    var _ = br.ReadBytes(300);

                    using (var substream = new SubStream(br.BaseStream, objOffset, objLength))
                    {

                        Prefabs.Add(new Prefab
                        {
                            Name = name,
                            Description = desc,
                            Map = rmf.Read(substream)
                        });
                    }
                }
            }
        }

        public void Save(string file)
        {
            using (var stream = File.OpenWrite(file))
            {
                Save(stream);
            }
        }

        public void Save(Stream stream)
        {
            using (var bw = new BinaryWriter(stream))
            {
                var prefabLibraryHeader = "Worldcraft Prefab Library\r\n" + (char)0x1A;
                var version = 0.1f;
                var rmf = new WorldcraftRmfFormat();

                var objectOffset = 544;
                List<PrefabMeta> meta = new List<PrefabMeta>();

                bw.WriteFixedLengthString(Encoding.ASCII, 28, prefabLibraryHeader);
                bw.Write(version);

                foreach (var prefab in Prefabs)
                {
                    stream.Position = objectOffset;
                    using (var mapStream = new MemoryStream())
                    {
                        rmf.Write(mapStream, prefab.Map, "2.2");
                        var prefabMeta = new PrefabMeta()
                        {
                            StartOffset = (uint)objectOffset,
                            DataLenght =(uint) mapStream.Length,
                            Name = prefab.Name,
                            Description = prefab.Description,
                        };
                        meta.Add(prefabMeta);
                         
                        mapStream.Position = objectOffset;
                        mapStream.WriteTo(stream);


                        objectOffset+= (int)mapStream.Length;
                    }
                }

                bw.Seek(32, SeekOrigin.Begin);
                bw.Write(objectOffset);
                bw.Write(Prefabs.Count);
                bw.WriteFixedLengthString(Encoding.ASCII, 500, LibraryDescription); //PrefabLibraryDescription
                bw.Seek(objectOffset, SeekOrigin.Begin);



                foreach (var prefabMeta in meta)
                {
                    bw.Write(prefabMeta.StartOffset);
                    bw.Write(prefabMeta.DataLenght);
                    bw.WriteFixedLengthString(Encoding.ASCII, 31, prefabMeta.Name);
                    bw.WriteFixedLengthString(Encoding.ASCII, 205, prefabMeta.Description);
                    bw.WriteFixedLengthString(Encoding.ASCII, 300, "");
                }

            }
		}
        private struct PrefabMeta
        {
            public uint StartOffset;
            public uint DataLenght;
            public string Name;
            public string Description;
        }
	}
}
