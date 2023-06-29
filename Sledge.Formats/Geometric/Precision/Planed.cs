using System;
using System.Numerics;

namespace Sledge.Formats.Geometric.Precision
{
    /// <summary>
    /// Defines a plane in the form Ax + By + Cz + D = 0. Uses double precision floating points.
    /// </summary>
    public readonly struct Planed
    {
        public Vector3d Normal { get; }
        public double D { get; }

        public Planed(Vector3 normal, float d) : this(normal.ToVector3d(), d)
        {
            //
        }

        public Planed(Vector3d normal, double d)
        {
            Normal = normal.Normalise();
            D = d;
        }

        /// <summary>
        /// Gets an arbitrary point on this plane.
        /// </summary>
        public Vector3d GetPointOnPlane()
        {
            return Normal * -D;
        }

        public static Planed CreateFromVertices(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return Planed.CreateFromVertices(p1.ToVector3d(), p2.ToVector3d(), p3.ToVector3d());
        }

        /// <summary>
        /// Create a plane from 3 vertices. Assumes that the vertices are ordered counter-clockwise.
        /// </summary>
        public static Planed CreateFromVertices(Vector3d p1, Vector3d p2, Vector3d p3)
        {
            var a = p2 - p1;
            var b = p3 - p1;

            var normal = a.Cross(b).Normalise();
            var d = -normal.Dot(p1);

            return new Planed(normal, d);
        }

        /// <summary>Finds if the given point is above, below, or on the plane.</summary>
        /// <param name="co">The Vector3 to test</param>
        /// <param name="epsilon">Tolerance value</param>
        /// <returns>
        /// PlaneClassification.Back if Vector3 is below the plane<br />
        /// PlaneClassification.Front if Vector3 is above the plane<br />
        /// PlaneClassification.OnPlane if Vector3 is on the plane.
        /// </returns>
        public PlaneClassification OnPlane(Vector3d co, double epsilon = 0.0001d)
        {
            //eval (s = Ax + By + Cz + D) at point (x,y,z)
            //if s > 0 then point is "above" the plane (same side as normal)
            //if s < 0 then it lies on the opposite side
            //if s = 0 then the point (x,y,z) lies on the plane
            var res = DotCoordinate(co);
            if (Math.Abs(res) < epsilon) return PlaneClassification.OnPlane;
            if (res < 0) return PlaneClassification.Back;
            return PlaneClassification.Front;
        }

        /// <summary>
        /// Gets the point that the line intersects with this plane.
        /// </summary>
        /// <param name="start">The start of the line to intersect with</param>
        /// <param name="end">The end of the line to intersect with</param>
        /// <param name="ignoreDirection">Set to true to ignore the direction
        /// of the plane and line when intersecting. Defaults to false.</param>
        /// <param name="ignoreSegment">Set to true to ignore the start and
        /// end points of the line in the intersection. Defaults to false.</param>
        /// <returns>The point of intersection, or null if the line does not intersect</returns>
        public Vector3d? GetIntersectionPoint(Vector3d start, Vector3d end, bool ignoreDirection = false, bool ignoreSegment = false)
        {
            // http://softsurfer.com/Archive/algorithm_0104/algorithm_0104B.htm#Line%20Intersections
            // http://paulbourke.net/geometry/planeline/

            var dir = end - start;
            var denominator = Normal.Dot(dir);
            var numerator = Normal.Dot(GetPointOnPlane() - start);
            if (Math.Abs(denominator) < 0.00001d || !ignoreDirection && denominator < 0) return null;
            var u = numerator / denominator;
            if (!ignoreSegment && (u < 0 || u > 1)) return null;
            return start + u * dir;
        }

        /// <summary>
        /// Project a point into the space of this plane. I.e. Get the point closest
        /// to the provided point that is on this plane.
        /// </summary>
        /// <param name="point">The point to project</param>
        /// <returns>The point projected onto this plane</returns>
        public Vector3d Project(Vector3d point)
        {
            // http://www.gamedev.net/topic/262196-projecting-vector-onto-a-plane/
            // Projected = Point - ((Point - PointOnPlane) . Normal) * Normal
            return point - (point - GetPointOnPlane()).Dot(Normal) * Normal;
        }

        /// <summary>Evaluates the value of the plane formula at the given coordinate.</summary>
        /// <remarks>Returns the dot product of a specified three-dimensional vector and the normal vector of this plane plus the distance (<see cref="System.Numerics.Plane.D" />) value of the plane.</remarks>
        public double DotCoordinate(Vector3d co)
        {
            return Normal.Dot(co) + D;
        }

        /// <summary>
        /// Gets the axis closest to the normal of this plane
        /// </summary>
        /// <returns>Vector3.UnitX, Vector3.UnitY, or Vector3.UnitZ depending on the plane's normal</returns>
        public Vector3d GetClosestAxisToNormal()
        {
            // VHE prioritises the axes in order of X, Y, Z.
            var norm = Normal.Absolute();

            if (norm.X >= norm.Y && norm.X >= norm.Z) return Vector3d.UnitX;
            if (norm.Y >= norm.Z) return Vector3d.UnitY;
            return Vector3d.UnitZ;
        }

        public Planed Clone()
        {
            return new Planed(Normal, D);
        }

        /// <summary>
        /// Intersects three planes and gets the point of their intersection.
        /// </summary>
        /// <returns>The point that the planes intersect at, or null if they do not intersect at a point.</returns>
        public static Vector3d? Intersect(Planed p1, Planed p2, Planed p3)
        {
            // http://paulbourke.net/geometry/3planes/

            var c1 = p2.Normal.Cross(p3.Normal);
            var c2 = p3.Normal.Cross(p1.Normal);
            var c3 = p1.Normal.Cross(p2.Normal);

            var denom = p1.Normal.Dot(c1);
            if (denom < 0.00001d) return null; // No intersection, planes must be parallel

            var numer = -p1.D * c1 + -p2.D * c2 + -p3.D * c3;
            return numer / denom;
        }

        public bool EquivalentTo(Planed other, double delta = 0.0001d)
        {
            return Normal.EquivalentTo(other.Normal, delta)
                   && Math.Abs(D - other.D) < delta;
        }

        public Plane ToPlane()
        {
            return new Plane(Normal.ToVector3(), (float) D);
        }

        public Planed Flip()
        {
            return new Planed(-Normal, -D);
        }
    }
}
