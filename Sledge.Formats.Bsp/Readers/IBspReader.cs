using System.IO;
using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Readers
{
    public interface IBspReader
    {
        Version SupportedVersion { get; }
        int NumLumps { get; }

        void StartHeader(BspFile file, BinaryReader br, BspFileOptions options);
        Blob ReadBlob(BinaryReader br, BspFileOptions options);
        void EndHeader(BspFile file, BinaryReader br, BspFileOptions options);

        ILump GetLump(Blob blob, BspFileOptions options);
    }
}