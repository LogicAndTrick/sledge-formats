using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Sledge.Formats.Geometric.Precision;

namespace Sledge.Formats.Geometric
{
    /// <summary>
    /// Represents a coplanar, directed polygon with at least 3 vertices.
    /// </summary>
    public class Polygon
    {
        /// <summary>
        /// The vertices for the polygon, in counter-clockwise order when looking at the visible face of the polygon.
        /// </summary>
        public IReadOnlyList<Vector3> Vertices { get; }

        public Plane Plane => Plane.CreateFromVertices(Vertices[0], Vertices[1], Vertices[2]);
        public Vector3 Origin => Vertices.Aggregate(Vector3.Zero, (x, y) => x + y) / Vertices.Count;

        /// <summary>
        /// Creates a polygon from a list of points
        /// </summary>
        /// <param name="vertices">The vertices of the polygon</param>
        public Polygon(IEnumerable<Vector3> vertices)
        {
            Vertices = vertices.ToList();
        }

        /// <summary>
        /// Creates a polygon from a list of points
        /// </summary>
        /// <param name="vertices">The vertices of the polygon</param>
        public Polygon(params Vector3[] vertices)
        {
            Vertices = vertices.ToList();
        }

        /// <summary>
        /// Creates a polygon from a plane and a radius.
        /// Expands the plane to the radius size to create a large polygon with 4 vertices.
        /// </summary>
        /// <param name="plane">The polygon plane</param>
        /// <param name="radius">The polygon radius</param>
        public Polygon(Plane plane, float radius = 4096f)
        {
            // Get aligned up and right axes to the plane
            var direction = plane.GetClosestAxisToNormal();
            var up = direction == Vector3.UnitZ ? Vector3.UnitX : -Vector3.UnitZ;
            var v = up.Dot(plane.Normal);
            up = (up + -v * plane.Normal).Normalise();
            var right = up.Cross(plane.Normal);

            up *= radius;
            right *= radius;

            var origin = plane.GetPointOnPlane();
            var verts = new List<Vector3>
            {
                origin - right - up, // Bottom left
                origin + right - up, // Bottom right
                origin + right + up, // Top right
                origin - right + up, // Top left
            };
            Vertices = verts.ToList();
        }

        public PlaneClassification ClassifyAgainstPlane(Plane p)
        {
            var count = Vertices.Count;
            var front = 0;
            var back = 0;
            var onplane = 0;
            
            foreach (var t in Vertices)
            {
                var test = p.OnPlane(t);

                // Vertices on the plane are both in front and behind the plane in this context
                if (test == PlaneClassification.Back) back++;
                if (test == PlaneClassification.Front) front++;
                if (test == PlaneClassification.OnPlane) onplane++;
            }

            if (onplane == count) return PlaneClassification.OnPlane;
            if (front == count) return PlaneClassification.Front;
            if (back == count) return PlaneClassification.Back;
            return PlaneClassification.Spanning;
        }

        /// <summary>
        /// Splits this polygon by a clipping plane, returning the back and front planes.
        /// The original polygon is not modified.
        /// </summary>
        /// <param name="clip">The clipping plane</param>
        /// <param name="back">The back polygon</param>
        /// <param name="front">The front polygon</param>
        /// <returns>True if the split was successful</returns>
        public bool Split(Plane clip, out Polygon back, out Polygon front)
        {
            return Split(clip, out back, out front, out _, out _);
        }

        /// <summary>
        /// Splits this polygon by a clipping plane, returning the back and front planes.
        /// The original polygon is not modified.
        /// </summary>
        /// <param name="clip">The clipping plane</param>
        /// <param name="back">The back polygon</param>
        /// <param name="front">The front polygon</param>
        /// <param name="coplanarBack">If the polygon rests on the plane and points backward, this will not be null</param>
        /// <param name="coplanarFront">If the polygon rests on the plane and points forward, this will not be null</param>
        /// <returns>True if the split was successful</returns>
        public bool Split(Plane clip, out Polygon back, out Polygon front, out Polygon coplanarBack, out Polygon coplanarFront)
        {
            const float epsilon = NumericsExtensions.Epsilon;

            var clipd = new Planed(clip.Normal.ToVector3d(), clip.D);
            var distances = Vertices.Select(x => clipd.DotCoordinate(x.ToVector3d())).ToList();

            int cb = 0, cf = 0;
            for (var i = 0; i < distances.Count; i++)
            {
                if (distances[i] < -epsilon) cb++;
                else if (distances[i] > epsilon) cf++;
                else distances[i] = 0;
            }

            // Check non-spanning cases
            if (cb == 0 && cf == 0)
            {
                // Co-planar
                back = front = coplanarBack = coplanarFront = null;
                if (Plane.Normal.Dot(clip.Normal) > 0) coplanarFront = this;
                else coplanarBack = this;
                return false;
            }
            else if (cb == 0)
            {
                // All vertices in front
                back = coplanarBack = coplanarFront = null;
                front = this;
                return false;
            }
            else if (cf == 0)
            {
                // All vertices behind
                front = coplanarBack = coplanarFront = null;
                back = this;
                return false;
            }

            // Get the new front and back vertices
            var backVerts = new List<Vector3>();
            var frontVerts = new List<Vector3>();

            for (var i = 0; i < Vertices.Count; i++)
            {
                var j = (i + 1) % Vertices.Count;

                Vector3 s = Vertices[i], e = Vertices[j];
                double sd = distances[i], ed = distances[j]; // using doubles here intentionally, for precision

                if (sd <= 0) backVerts.Add(s);
                if (sd >= 0) frontVerts.Add(s);

                if ((sd < 0 && ed > 0) || (ed < 0 && sd > 0))
                {
                    // this is really the only calculation where we need higher precision, so temporarily switch to doubles for this and then switch back
                    var t = sd / (sd - ed);
                    var intersectd = s.ToVector3d() * (1 - t) + e.ToVector3d() * t;
                    var intersect = intersectd.ToVector3();

                    // avoid round off error when possible
                    // (if we know the clipping plane is a unit vector, then the intersection point on that axis will always be the plane's distance from the origin)
                    if (clip.Normal == Vector3.UnitX) intersect = new Vector3(-clip.D, intersect.Y, intersect.Z);
                    else if (clip.Normal == -Vector3.UnitX) intersect = new Vector3(clip.D, intersect.Y, intersect.Z);
                    else if (clip.Normal == Vector3.UnitY) intersect = new Vector3(intersect.X, -clip.D, intersect.Z);
                    else if (clip.Normal == -Vector3.UnitY) intersect = new Vector3(intersect.X, clip.D, intersect.Z);
                    else if (clip.Normal == Vector3.UnitZ) intersect = new Vector3(intersect.X, intersect.Y, -clip.D);
                    else if (clip.Normal == -Vector3.UnitZ) intersect = new Vector3(intersect.X, intersect.Y, clip.D);

                    backVerts.Add(intersect);
                    frontVerts.Add(intersect);
                }
            }

            back = new Polygon(backVerts.Select(x => new Vector3(x.X, x.Y, x.Z)));
            front = new Polygon(frontVerts.Select(x => new Vector3(x.X, x.Y, x.Z)));
            coplanarBack = coplanarFront = null;

            return true;
        }

        public Polygond ToPolygond()
        {
            return new Polygond(Vertices.Select(x => x.ToVector3d()));
        }

        public Polygon Rounded(int num)
        {
            return new Polygon(Vertices.Select(x => x.Round(num)));
        }
    }
}