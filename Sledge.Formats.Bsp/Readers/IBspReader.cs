using System.IO;
using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Readers
{
    public interface IBspReader
    {
        Version SupportedVersion { get; }
        int NumLumps { get; }

        void StartHeader(BspFile file, BinaryReader br);
        Blob ReadBlob(BinaryReader br);
        void EndHeader(BspFile file, BinaryReader br);

        ILump GetLump(Blob blob);
    }
}