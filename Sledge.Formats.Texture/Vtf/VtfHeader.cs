using System.Numerics;

namespace Sledge.Formats.Texture.Vtf
{
    public class VtfHeader
    {
        public decimal Version { get; set; }
        public VtfImageFlag Flags { get; set; }
        public Vector3 Reflectivity { get; set; }
        public float BumpmapScale { get; set; }
    }
}