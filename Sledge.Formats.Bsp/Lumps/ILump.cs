using System.IO;

namespace Sledge.Formats.Bsp.Lumps
{
    public interface ILump
    {
        void Read(BinaryReader br, Blob blob, Version version);
        void PostReadProcess(BspFile bsp);

        void PreWriteProcess(BspFile bsp, Version version);
        int Write(BinaryWriter bw, Version version);
    }
}
