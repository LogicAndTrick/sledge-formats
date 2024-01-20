using System.Numerics;
using Sledge.Formats.Bsp.Objects;
using Plane = System.Numerics.Plane;

namespace Sledge.Formats.Bsp
{
    public class TraceLine
    {
        public bool Success { get; set; }

        public Contents StartContents { get; set; }
        public Contents EndContents { get; set; }
        public bool PassedThroughNonSolid { get; set; }

        public Vector3 StartPoint { get; set; }
        public Vector3 EndPoint { get; set; }

        public Vector3 TargetPoint { get; set; }

        public float EndFraction { get; set; } = 1;
        public Plane EndPlane { get; set; }
    }
}
