using System.Numerics;

namespace Sledge.Formats.Bsp.Objects
{
    public class Model
    {
        public Vector3 Mins { get; set; }
        public Vector3 Maxs { get; set; }
        public Vector3 Origin { get; set; }
        public int[] HeadNodes { get; set; }
        public int VisLeaves { get; set; }
        public int FirstFace { get; set; }
        public int NumFaces { get; set; }
    }
}