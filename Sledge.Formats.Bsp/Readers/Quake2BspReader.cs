using System;
using System.IO;
using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Readers
{
    public class Quake2BspReader : IBspReader
    {
        public Version SupportedVersion => Version.Quake2;
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
                case Lump.Leaves:
                    return new Leaves();
                case Lump.LeafFaces:
                    return new LeafFaces();
                case Lump.LeafBrushes:
                    return new LeafBrushes();
                case Lump.Edges:
                    return new Edges();
                case Lump.Surfedges:
                    return new Surfedges();
                case Lump.Models:
                    return new Models();
                case Lump.Brushes:
                    return new Brushes();
                case Lump.BrushSides:
                    return new BrushSides();
                case Lump.Pop:
                    return new Pop();
                case Lump.Areas:
                    return new Areas();
                case Lump.AreaPortals:
                    return new AreaPortals();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum Lump : int
        {
            Entities = 0,
            Planes = 1,
            Vertices = 2,
            Visibility = 3,
            Nodes = 4,
            Texinfo = 5,
            Faces = 6,
            Lighting = 7,
            Leaves = 8,
            LeafFaces = 9,
            LeafBrushes = 10,
            Edges = 11,
            Surfedges = 12,
            Models = 13,
            Brushes = 14,
            BrushSides = 15,
            Pop = 16,
            Areas = 17,
            AreaPortals = 18,

            NumLumps = 19
        }
    }
}
