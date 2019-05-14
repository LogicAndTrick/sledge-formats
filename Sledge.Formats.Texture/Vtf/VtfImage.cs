namespace Sledge.Formats.Texture.Vtf
{
    public class VtfImage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Mipmap { get; set; }
        public int Frame { get; set; }
        public int Face { get; set; }
        public int Slice { get; set; }
        public byte[] Data { get; set; }
    }
}