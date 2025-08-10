using System.Numerics;

namespace Sledge.Formats.Bsp.Objects
{
    /// <summary>
    /// A clipping hull for a simple bounding box.
    /// </summary>
    public class BoxHull : Hull
    {
        private readonly float[] _distances;

        public BoxHull(Vector3 mins, Vector3 maxs)
        {
            _distances = new[]
            {
                maxs.X,
                mins.X,
                maxs.Y,
                mins.Y,
                maxs.Z,
                mins.Z,
            };
        }

        public override int GetRoot()
        {
            return 0;
        }

        public override Plane GetPlane(int nodeNum)
        {
            return new Plane
            {
                Distance = _distances[nodeNum],
                Normal = Planes[nodeNum].Normal,
                Type = Planes[nodeNum].Type
            };
        }

        public override int GetChild(int nodeNum, int side)
        {
            return Children[nodeNum][side];
        }
        
        private static readonly int[][] Children;
        private static readonly Plane[] Planes;

        static BoxHull()
        {
            const int empty = (int)Contents.Empty;
            const int solid = (int)Contents.Solid;

            Children = new[]
            {
                new[] { empty, 1 },
                new[] { 2, empty },
                new[] { empty, 3 },
                new[] { 4, empty },
                new[] { empty, 5 },
                new[] { solid, empty },
            };
            Planes = new[]
            {
                new Plane { Normal = Vector3.UnitX, Type = PlaneType.X },
                new Plane { Normal = Vector3.UnitX, Type = PlaneType.X },
                new Plane { Normal = Vector3.UnitY, Type = PlaneType.Y },
                new Plane { Normal = Vector3.UnitY, Type = PlaneType.Y },
                new Plane { Normal = Vector3.UnitZ, Type = PlaneType.Z },
                new Plane { Normal = Vector3.UnitZ, Type = PlaneType.Z },
            };
        }
    }
}