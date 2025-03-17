using Sledge.Formats.Bsp.Algorithms;
using System.Numerics;

namespace Sledge.Formats.Bsp.Objects
{
    public abstract class Hull
    {
        public abstract int GetRoot();
        public abstract Plane GetPlane(int nodeNum);
        public abstract int GetChild(int nodeNum, int side);

        public TraceLine TraceLine(Vector3 start, Vector3 end)
        {
            return Clipping.TraceLine(this, start, end);
        }

        public Contents GetContents(Vector3 point) => GetContents(GetRoot(), point);

        public Contents GetContents(int startNode, Vector3 point)
        {
            while (startNode >= 0)
            {
                var plane = GetPlane(startNode);
                var d = plane.Evaluate(point);
                startNode = d < 0 ? GetChild(startNode, 1) : GetChild(startNode, 0);
            }

            return (Contents)startNode;
        }
    }
}
