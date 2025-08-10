using System.Numerics;

namespace Sledge.Formats.Map.Objects
{
    public class Surface
    {
        public string TextureName { get; set; }

        public Vector3 UAxis { get; set; } = Vector3.UnitX;
        public Vector3 VAxis { get; set; } = -Vector3.UnitZ;
        public float XScale { get; set; } = 1;
        public float YScale { get; set; } = 1;
        public float XShift { get; set; }
        public float YShift { get; set; }
        public float Rotation { get; set; }

        public int ContentFlags { get; set; }
        public int SurfaceFlags { get; set; }
        public float Value { get; set; }

        public float LightmapScale { get; set; }
        public string SmoothingGroups { get; set; }
    }
}