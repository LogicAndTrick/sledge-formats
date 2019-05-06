namespace Sledge.Formats.Bsp.Objects
{
    public class Leaf
    {
        public const int MaxNumAmbientLevels = 4;

        public Contents Contents { get; set; }

        public int VisOffset { get; set; }
        public short Cluster { get; set; }
        public short Area { get; set; }

        public short[] Mins { get; set; }
        public short[] Maxs { get; set; }

        public ushort FirstLeafFace { get; set; }
        public ushort NumLeafFaces { get; set; }

        public ushort FirstLeafBrush { get; set; }
        public ushort NumLeafBrushes { get; set; }

        public byte[] AmbientLevels { get; set; }
    }
}