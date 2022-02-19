using System;
using System.IO;
using System.Linq;
using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Readers
{
    public class GoldsourceBspReader : IBspReader
    {
        public Version SupportedVersion => Version.Goldsource;
        public int NumLumps => (int) Lump.NumLumps;

        public void StartHeader(BspFile file, BinaryReader br, BspFileOptions options)
        {
            // 
        }

        public Blob ReadBlob(BinaryReader br, BspFileOptions options)
        {
            return new Blob
            {
                Offset = br.ReadInt32(),
                Length = br.ReadInt32()
            };
        }

        public void EndHeader(BspFile file, BinaryReader br, BspFileOptions options)
        {
            var bshiftFormat = options.UseBlueShiftFormat;
            if (options.AutodetectBlueShiftFormat)
            {
                bshiftFormat = false;

                // The first two lumps are switched in the blue shift format
                // Since the first lump is entities and the second is planes,
                // use some basic heuristics to try and detect if they are swapped.
                // If unsure, assume it's not the blue shift format.
                var entBlob = file.Blobs.FirstOrDefault(x => (Lump) x.Index == Lump.Entities);
                var plnBlob = file.Blobs.FirstOrDefault(x => (Lump) x.Index == Lump.Planes);
                if (entBlob.Offset > 0 && plnBlob.Offset > 0)
                {
                    var pos = br.BaseStream.Position;

                    // make sure we hit at least 2 of the 3 heuristics:
                    // - entity blob is a multiple of the plane struct size
                    // - plane blob is NOT a multiple of the plane struct size
                    // - entity blob starts with '{'
                    br.BaseStream.Seek(entBlob.Offset, SeekOrigin.Begin);
                    var entBlobStart = br.ReadByte();
                    const int planeStructSize = (3 * 4) + 4 + 4; // float(3), float, int32

                    var confidence = 0;
                    if (entBlob.Length % planeStructSize == 0) confidence++;
                    if (plnBlob.Length % planeStructSize != 0) confidence++;
                    if (entBlobStart == '{') confidence++;

                    bshiftFormat = confidence >= 2;

                    br.BaseStream.Seek(pos, SeekOrigin.Begin);
                }
            }

            if (bshiftFormat)
            {
                // Switch the entity and planes blobs around
                var entBlob = file.Blobs.FirstOrDefault(x => (Lump) x.Index == Lump.Entities);
                var plnBlob = file.Blobs.FirstOrDefault(x => (Lump) x.Index == Lump.Planes);
                if (entBlob.Offset > 0 && plnBlob.Offset > 0)
                {
                    // Blob is a struct so we need to go through this... not sure why I made that decision
                    for (var i = 0; i < file.Blobs.Count; i++)
                    {
                        var idx = (Lump)file.Blobs[i].Index;
                        if (idx == Lump.Entities)
                        {
                            file.Blobs[i] = new Blob
                            {
                                Index = entBlob.Index,
                                Offset = plnBlob.Offset,
                                Length = plnBlob.Length
                            };
                        }
                        else if (idx == Lump.Planes)
                        {
                            file.Blobs[i] = new Blob
                            {
                                Index = plnBlob.Index,
                                Offset = entBlob.Offset,
                                Length = entBlob.Length
                            };
                        }
                    }
                }
            }

            options.UseBlueShiftFormat = bshiftFormat;
        }

        public ILump GetLump(Blob blob, BspFileOptions options)
        {
            switch ((Lump) blob.Index)
            {
                case Lump.Entities:
                    return new Entities();
                case Lump.Planes:
                    return new Planes();
                case Lump.Textures:
                    return new Textures();
                case Lump.Vertices:
                    return new Vertices();
                case Lump.Visibility:
                    return new Visibility();
                case Lump.Nodes:
                    return new Nodes();
                case Lump.Texinfo:
                    return new Texinfo();
                case Lump.Faces:
                    return new Faces();
                case Lump.Lighting:
                    return new Lightmaps();
                case Lump.Clipnodes:
                    return new Clipnodes();
                case Lump.Leaves:
                    return new Leaves();
                case Lump.Marksurfaces:
                    return new LeafFaces();
                case Lump.Edges:
                    return new Edges();
                case Lump.Surfedges:
                    return new Surfedges();
                case Lump.Models:
                    return new Models();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum Lump : int
        {
            Entities = 0,
            Planes = 1,
            Textures = 2,
            Vertices = 3,
            Visibility = 4,
            Nodes = 5,
            Texinfo = 6,
            Faces = 7,
            Lighting = 8,
            Clipnodes = 9,
            Leaves = 10,
            Marksurfaces = 11,
            Edges = 12,
            Surfedges = 13,
            Models = 14,

            NumLumps = 15
        }
    }
}
