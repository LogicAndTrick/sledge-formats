using System.Numerics;

namespace Sledge.Formats.Bsp.Objects
{
    public class TextureInfo
    {
        public Vector4 S { get; set; }
        public Vector4 T { get; set; }
        public int MipTexture { get; set; }
        public int Flags { get; set; }
        public int Value { get; set; }
        public string TextureName { get; set; }
        public int NextTextureInfo { get; set; }
    }
}