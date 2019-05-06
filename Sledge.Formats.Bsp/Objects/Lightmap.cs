namespace Sledge.Formats.Bsp.Objects
{
    public class Lightmap
    {
        public int Offset { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BitsPerPixel { get; set; }
        public byte[] Data { get; set; }
    }
}