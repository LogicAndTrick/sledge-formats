using System.Numerics;

namespace Sledge.Formats.Model.Goldsource
{
    public class Header
    {
        public ID ID { get; set; } = ID.Idst;
        public Version Version { get; set; } = Version.Goldsource;
        public string Name { get; set; } = "";
        public int Size { get; set; }

        public Vector3 EyePosition { get; set; }
        public Vector3 HullMin { get; set; }
        public Vector3 HullMax { get; set; }
        public Vector3 BoundingBoxMin { get; set; }
        public Vector3 BoundingBoxMax { get; set; }

        public int Flags { get; set; }
    }
}