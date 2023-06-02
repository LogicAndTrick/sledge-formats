using System.Numerics;

namespace Sledge.Formats.Model.Goldsource
{
    public struct Attachment
    {
        public string Name;
        public int Type;
        public int Bone;
        public Vector3 Origin;
        public Vector3[] Vectors;
    }
}