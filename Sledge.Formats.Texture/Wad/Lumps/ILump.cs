using System.IO;

namespace Sledge.Formats.Texture.Wad.Lumps
{
    public interface ILump
    {
        LumpType Type { get; }
        int Write(BinaryWriter bw);
    }
}
