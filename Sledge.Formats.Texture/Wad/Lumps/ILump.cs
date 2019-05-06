using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sledge.Formats.Texture.Wad.Lumps
{
    public interface ILump
    {
        LumpType Type { get; }
        int Write(BinaryWriter bw);
    }
}
