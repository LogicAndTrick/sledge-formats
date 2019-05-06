namespace Sledge.Formats.Bsp.Objects
{
    public class Node
    {
        public uint Plane { get; set; }
        public int[] Children { get; set; }
        public short[] Mins { get; set; }
        public short[] Maxs { get; set; }
        public ushort FirstFace { get; set; }
        public ushort NumFaces { get; set; }
    }
}