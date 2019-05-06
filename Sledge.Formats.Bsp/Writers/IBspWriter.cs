using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Writers
{
    public interface IBspWriter
    {
        Version SupportedVersion { get; }

        void SeekToFirstLump(BinaryWriter bw);
        void WriteHeader(BspFile file, IEnumerable<Blob> blobs, BinaryWriter bw);

        IEnumerable<ILump> GetLumps(BspFile bsp);
    }
}
