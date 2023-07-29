using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sledge.Formats.Geometric.Precision
{
    /// <summary>
    /// Represents a convex polyhedron with at least 4 sides. Uses double precision floating points.
    /// </summary>
    public class Polyhedrond
    {
        public IReadOnlyList<Polygond> Polygons { get; }

        public Vector3d Origin => Polygons.Aggregate(Vector3d.Zero, (x, y) => x + y.Origin) / Polygons.Count;

        public Polyhedrond(IEnumerable<Polygon> polygons) : this(polygons.Select(x => x.ToPolygond()))
        {
            //
        }

        /// <summary>
        /// Creates a polyhedron from a list of polygons which are assumed to be valid.
        /// </summary>
        public Polyhedrond(IEnumerable<Polygond> polygons)
        {
            Polygons = polygons.ToList();
        }

        public Polyhedrond(IEnumerable<Plane> planes) : this(planes.Select(x => x.ToPlaned()))
        {
            //
        }

        /// <summary>
        /// Creates a polyhedron by intersecting a set of at least 4 planes.
        /// </summary>
        public Polyhedrond(IEnumerable<Planed> planes)
        {
            var polygons = new List<Polygond>();

            var list = planes.ToList();
            for (var i = 0; i < list.Count; i++)
            {
                // Split the polygon by all the other planes
                var poly = new Polygond(list[i]);
                for (var j = 0; j < list.Count; j++)
                {
                    if (i != j && poly.Split(list[j], out var back, out _))
                    {
                        poly = back;
                    }
                }
                polygons.Add(poly);
            }

            // Ensure all the faces point outwards
            var origin = polygons.Aggregate(Vector3d.Zero, (x, y) => x + y.Origin) / polygons.Count;
            for (var i = 0; i < polygons.Count; i++)
            {
                var face = polygons[i];
                if (face.Plane.OnPlane(origin) >= 0) polygons[i] = new Polygond(face.Vertices.Reverse());
            }

            Polygons = polygons;
        }

        /// <summary>
        /// Splits this polyhedron into two polyhedron by intersecting against a plane.
        /// </summary>
        /// <param name="plane">The splitting plane</param>
        /// <param name="back">The back side of the polyhedron</param>
        /// <param name="front">The front side of the polyhedron</param>
        /// <returns>True if the plane splits the polyhedron, false if the plane doesn't intersect</returns>
        public bool Split(Planed plane, out Polyhedrond back, out Polyhedrond front)
        {
            back = front = null;

            // Check that this solid actually spans the plane
            var classify = Polygons.Select(x => x.ClassifyAgainstPlane(plane)).Distinct().ToList();
            if (classify.All(x => x != PlaneClassification.Spanning))
            {
                if (classify.Any(x => x == PlaneClassification.Back)) back = this;
                else if (classify.Any(x => x == PlaneClassification.Front)) front = this;
                return false;
            }

            var backPlanes = new List<Planed> { plane };
            var frontPlanes = new List<Planed> { new Planed(-plane.Normal, -plane.D) };

            foreach (var face in Polygons)
            {
                var classification = face.ClassifyAgainstPlane(plane);
                if (classification != PlaneClassification.Back) frontPlanes.Add(face.Plane);
                if (classification != PlaneClassification.Front) backPlanes.Add(face.Plane);
            }

            back = new Polyhedrond(backPlanes);
            front = new Polyhedrond(frontPlanes);

            return true;
        }

        public Polyhedron ToPolyhedron()
        {
            return new Polyhedron(Polygons.Select(x => x.ToPolygon()));
        }

        public Polyhedrond Rounded(int num)
        {
            return new Polyhedrond(Polygons.Select(x => x.Rounded(num)));
        }
    }
}
