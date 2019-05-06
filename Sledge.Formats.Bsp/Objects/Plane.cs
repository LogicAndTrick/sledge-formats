using System.Numerics;

namespace Sledge.Formats.Bsp.Objects
{
    public class Plane
    {
        public Vector3 Normal { get; set; }
        public float Distance { get; set; }
        public PlaneType Type { get; set; }
    }
}