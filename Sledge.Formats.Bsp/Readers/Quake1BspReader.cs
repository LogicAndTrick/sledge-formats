using System;
using System.IO;
using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Readers
{
    public class Quake1BspReader : IBspReader
    {
        public Version SupportedVersion => Version.Quake1;
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
            //
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
