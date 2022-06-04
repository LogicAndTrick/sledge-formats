using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Sledge.Formats.Bsp.Lumps;
using Sledge.Formats.Bsp.Readers;
using Sledge.Formats.Bsp.Writers;

namespace Sledge.Formats.Bsp
{
    public class BspFile
    {
        public Version Version { get; set; }
        public List<Blob> Blobs { get; set; }
        public List<ILump> Lumps { get; set; }
        public BspFileOptions Options { get; set; }

        /// <summary>
        /// Create an empty BSP file
        /// </summary>
        /// <param name="version">The version of the file</param>
        /// <param name="options">Options for the file</param>
        public BspFile(Version version, BspFileOptions options = null)
        {
            Version = version;
            Blobs = new List<Blob>();
            Lumps = new List<ILump>();
            Options = options ?? new BspFileOptions();
        }

        public BspFile(Stream stream, BspFileOptions options = null)
        {
            Options = (options ?? BspFileOptions.Default).Copy();
            using (var br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                // Read the version number
                // More recent formats have a "magic" number when different engines forked
                var magic = (Magic) br.ReadUInt32();
                switch (magic)
                {
                    case Magic.Ibsp:
                    case Magic.Vbsp:
                        Version = (Version) ((br.ReadUInt32() << 32) + magic);
                        break;
                    default:
                        Version = (Version) magic;
                        break;
                }

                // Initialise the reader
                var reader = _readers.First(x => x.SupportedVersion == Version);

                reader.StartHeader(this, br, Options);

                // Read the blobs
                Blobs = new List<Blob>();
                for (var i = 0; i < reader.NumLumps; i++)
                {
                    var blob = reader.ReadBlob(br, Options);
                    blob.Index = i;
                    Blobs.Add(blob);
                }

                reader.EndHeader(this, br, Options);

                Lumps = new List<ILump>();
                foreach (var blob in Blobs)
                {
                    var lump = reader.GetLump(blob, Options);
                    if (lump == null) continue;

                    var pos = br.BaseStream.Position;
                    br.BaseStream.Seek(blob.Offset, SeekOrigin.Begin);

                    lump.Read(br, blob, Version);
                    Lumps.Add(lump);

                    br.BaseStream.Seek(pos, SeekOrigin.Begin);
                }

                foreach (var lump in Lumps)
                {
                    lump.PostReadProcess(this);
                }
            }
        }

        public void WriteToStream(Stream s, Version version)
        {
            var writer = _writers.First(x => x.SupportedVersion == version);

            foreach (var lump in Lumps)
            {
                lump.PreWriteProcess(this, version);
            }

            using (var bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                writer.SeekToFirstLump(bw);
                var lumps = writer.GetLumps(this, Options)
                    .Select((x, i) => new Blob
                    {
                        Offset = (int) bw.BaseStream.Position,
                        Length = x.Write(bw, version),
                        Index = i
                    })
                    .ToList();
                bw.Seek(0, SeekOrigin.Begin);
                writer.WriteHeader(this, lumps, bw, Options);
            }
        }

        public T GetLump<T>() where T : ILump
        {
            return (T) Lumps.FirstOrDefault(x => x is T);
        }

        private readonly IBspReader[] _readers =
        {
            new GoldsourceBspReader(),
            new Quake1BspReader(),
            new Quake2BspReader(), 
        };

        private readonly IBspWriter[] _writers =
        {
            new GoldsourceBspWriter(),
            new Quake1BspWriter(), 
        };

        public AreaPortals AreaPortals => GetLump<AreaPortals>();
        public Areas Areas => GetLump<Areas>();
        public Brushes Brushes => GetLump<Brushes>();
        public BrushSides BrushSides => GetLump<BrushSides>();
        public Clipnodes Clipnodes => GetLump<Clipnodes>();
        public Edges Edges => GetLump<Edges>();
        public Entities Entities => GetLump<Entities>();
        public Faces Faces => GetLump<Faces>();
        public LeafBrushes LeafBrushes => GetLump<LeafBrushes>();
        public LeafFaces LeafFaces => GetLump<LeafFaces>();
        public Leaves Leaves => GetLump<Leaves>();
        public Lightmaps Lightmaps => GetLump<Lightmaps>();
        public Models Models => GetLump<Models>();
        public Nodes Nodes => GetLump<Nodes>();
        public Planes Planes => GetLump<Planes>();
        public Pop Pop => GetLump<Pop>();
        public Surfedges Surfedges => GetLump<Surfedges>();
        public Texinfo Texinfo => GetLump<Texinfo>();
        public Textures Textures => GetLump<Textures>();
        public Vertices Vertices => GetLump<Vertices>();
        public Visibility Visibility => GetLump<Visibility>();
    }
}