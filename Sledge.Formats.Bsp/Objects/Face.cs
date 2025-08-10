namespace Sledge.Formats.Bsp.Objects
{
    public class Face
    {
        public const int MaxLightmaps = 4;

        public uint ID { get; set; }
        public ushort Plane { get; set; }
        public ushort Side { get; set; }
        public int FirstEdge { get; set; }
        public ushort NumEdges { get; set; }
        public ushort TextureInfo { get; set; }
        public byte[] Styles { get; set; }
        public int LightmapOffset { get; set; }
    }
}