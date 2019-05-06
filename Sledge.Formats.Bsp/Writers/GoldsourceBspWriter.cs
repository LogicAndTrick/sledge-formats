using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Writers
{
    public class GoldsourceBspWriter : IBspWriter
    {
        public Version SupportedVersion => Version.Goldsource;

        public void SeekToFirstLump(BinaryWriter bw)
        {
            const int headerSize = 4 + 15 * (4 + 4);
            bw.Seek(headerSize, SeekOrigin.Current);
        }

        public void WriteHeader(BspFile file, IEnumerable<Blob> blobs, BinaryWriter bw)
        {
            bw.Write((int) Version.Goldsource);
            foreach (var blob in blobs)
            {
                bw.Write(blob.Offset);
                bw.Write(blob.Length);
            }
        }

        public IEnumerable<ILump> GetLumps(BspFile bsp)
        {
            var types = new[]
            {
                typeof(Entities),
                typeof(Planes),
                typeof(Textures),
                typeof(Vertices),
                typeof(Visibility),
                typeof(Nodes),
                typeof(Texinfo),
                typeof(Faces),
                typeof(Lightmaps),
                typeof(Clipnodes),
                typeof(Leaves),
                typeof(LeafFaces),
                typeof(Edges),
                typeof(Surfedges),
                typeof(Models),
            };

            return types
                .Select(x => bsp.Lumps.FirstOrDefault(l => l.GetType() == x) ?? Activator.CreateInstance(x))
                .OfType<ILump>();
        }
    }
}
