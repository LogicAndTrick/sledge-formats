using System.IO;

namespace Sledge.Formats.Texture.Wad.Lumps
{
    public class MipTextureLump : RawTextureLump
    {
        public override LumpType Type => LumpType.MipTexture;

        public MipTextureLump(BinaryReader br) : base(br, true)
        {
            //
        }

        public override int Write(BinaryWriter bw)
        {
            var pos = bw.BaseStream.Position;
            Write(bw, true, this);
            return (int)(bw.BaseStream.Position - pos);
        }
    }
}