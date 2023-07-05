using System.Collections.Generic;
using System.IO;
using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Writers
{
    public interface IBspWriter
    {
        Version SupportedVersion { get; }

        void SeekToFirstLump(BinaryWriter bw);
        void WriteHeader(BspFile file, IEnumerable<Blob> blobs, BinaryWriter bw, BspFileOptions options);

        IEnumerable<ILump> GetLumps(BspFile bsp, BspFileOptions options);
    }
}
