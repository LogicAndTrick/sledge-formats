using Sledge.Formats.Geometric;
using Sledge.Formats.Geometric.Precision;
using System;
using System.Drawing;
using System.Globalization;
using System.Numerics;

namespace Sledge.Formats
{
    public static class NumericsExtensions
    {
        public const float Epsilon = 0.0001f;

        // Vector2
        public static Vector3 ToVector3(this Vector2 self)
        {
            return new Vector3(self, 0);
        }

        // Vector3
        public static bool EquivalentTo(this Vector3 self, Vector3 test, float delta = Epsilon)
        {
            var xd = Math.Abs(self.X - test.X);
            var yd = Math.Abs(self.Y - test.Y);
            var zd = Math.Abs(self.Z - test.Z);
            return xd < delta && yd < delta && zd < delta;
        }

        public static bool EquivalentTo(this Vector3 self, Vector3d test, float delta = Epsilon)
        {
            var xd = Math.Abs(self.X - test.X);
            var yd = Math.Abs(self.Y - test.Y);
            var zd = Math.Abs(self.Z - test.Z);
            return xd < delta && yd < delta && zd < delta;
        }

        public static Vector3 Parse(string x, string y, string z, NumberStyles ns, IFormatProvider provider)
        {
            return new Vector3(float.Parse(x, ns, provider), float.Parse(y, ns, provider), float.Parse(z, ns, provider));
        }

        public static bool TryParse(string x, string y, string z, NumberStyles ns, IFormatProvider provider, out Vector3 vec)
        {
            if (float.TryParse(x, ns, provider, out var a) && float.TryParse(y, ns, provider, out var b) && float.TryParse(z, ns, provider, out var c))
            {
                vec = new Vector3(a, b, c);
                return true;
            }

            vec = Vector3.Zero;
            return false;
        }


        /// <inheritdoc cref="Vector3.Normalize"/>
        public static Vector3 Normalise(this Vector3 self) => Vector3.Normalize(self);

        /// <inheritdoc cref="Vector3.Abs"/>
        public static Vector3 Absolute(this Vector3 self) => Vector3.Abs(self);

        /// <inheritdoc cref="Vector3.Dot"/>
        public static float Dot(this Vector3 self, Vector3 other) => Vector3.Dot(self, other);

        /// <inheritdoc cref="Vector3.Cross"/>
        public static Vector3 Cross(this Vector3 self, Vector3 other) => Vector3.Cross(self, other);

        public static Vector3 Round(this Vector3 self, int num = 8) => new Vector3((float) Math.Round(self.X, num), (float) Math.Round(self.Y, num), (float) Math.Round(self.Z, num));

        /// <summary>
        /// Gets the axis closest to this vector
        /// </summary>
        /// <returns>Vector3.UnitX, Vector3.UnitY, or Vector3.UnitZ depending on the given vector</returns>
        public static Vector3 ClosestAxis(this Vector3 self)
        {
            // VHE prioritises the axes in order of X, Y, Z.
            var norm = Vector3.Abs(self);

            if (norm.X >= norm.Y && norm.X >= norm.Z) return Vector3.UnitX;
            if (norm.Y >= norm.Z) return Vector3.UnitY;
            return Vector3.UnitZ;
        }

        public static Vector3d ToVector3d(this Vector3 self)
        {
            return new Vector3d(self.X, self.Y, self.Z);
        }

        public static Vector2 ToVector2(this Vector3 self)
        {
            return new Vector2(self.X, self.Y);
        }

        // Vector4
        public static Vector4 ToVector4(this Color self)
        {
            return new Vector4(self.R, self.G, self.B, self.A) / 255f;
        }

        // Color
        public static Color ToColor(this Vector4 self)
        {
            var mul = self * 255;
            return Color.FromArgb((byte) mul.W, (byte) mul.X, (byte) mul.Y, (byte) mul.Z);
        }

        public static Color ToColor(this Vector3 self)
        {
            var mul = self * 255;
            return Color.FromArgb(255, (byte) mul.X, (byte) mul.Y, (byte) mul.Z);
        }

        // Plane
        
        /// <summary>
        /// Gets an arbitrary point on this plane.
        /// </summary>
        public static Vector3 GetPointOnPlane(this Plane plane)
        {
            return plane.Normal * -plane.D;
        }

        /// <summary>Finds if the given point is above, below, or on the plane.</summary>
        /// <param name="plane">The plane</param>
        /// <param name="co">The Vector3 to test</param>
        /// <param name="epsilon">Tolerance value</param>
        /// <returns>
        /// PlaneClassification.Back if Vector3 is below the plane<br />
        /// PlaneClassification.Front if Vector3 is above the plane<br />
        /// PlaneClassification.OnPlane if Vector3 is on the plane.
        /// </returns>
        public static PlaneClassification OnPlane(this Plane plane, Vector3 co, double epsilon = 0.0001d)
        {
            //eval (s = Ax + By + Cz + D) at point (x,y,z)
            //if s > 0 then point is "above" the plane (same side as normal)
            //if s < 0 then it lies on the opposite side
            //if s = 0 then the point (x,y,z) lies on the plane
            var res = DotCoordinate(plane, co);
            if (Math.Abs(res) < epsilon) return PlaneClassification.OnPlane;
            if (res < 0) return PlaneClassification.Back;
            return PlaneClassification.Front;
        }

        /// <summary>
        /// Gets the point that the line intersects with this plane.
        /// </summary>
        /// <param name="plane">The plane</param>
        /// <param name="start">The start of the line to intersect with</param>
        /// <param name="end">The end of the line to intersect with</param>
        /// <param name="ignoreDirection">Set to true to ignore the direction
        /// of the plane and line when intersecting. Defaults to false.</param>
        /// <param name="ignoreSegment">Set to true to ignore the start and
        /// end points of the line in the intersection. Defaults to false.</param>
        /// <returns>The point of intersection, or null if the line does not intersect</returns>
        public static Vector3? GetIntersectionPoint(this Plane plane, Vector3 start, Vector3 end, bool ignoreDirection = false, bool ignoreSegment = false)
        {
            // http://softsurfer.com/Archive/algorithm_0104/algorithm_0104B.htm#Line%20Intersections
            // http://paulbourke.net/geometry/planeline/

            var dir = end - start;
            var denominator = plane.Normal.Dot(dir);
            var numerator = plane.Normal.Dot(GetPointOnPlane(plane) - start);
            if (Math.Abs(denominator) < 0.00001d || (!ignoreDirection && denominator < 0)) return null;
            var u = numerator / denominator;
            if (!ignoreSegment && (u < 0 || u > 1)) return null;
            return start + u * dir;
        }

        /// <summary>
        /// Project a point into the space of this plane. I.e. Get the point closest
        /// to the provided point that is on this plane.
        /// </summary>
        /// <param name="plane">The plane</param>
        /// <param name="point">The point to project</param>
        /// <returns>The point projected onto this plane</returns>
        public static Vector3 Project(this Plane plane, Vector3 point)
        {
            // http://www.gamedev.net/topic/262196-projecting-vector-onto-a-plane/
            // Projected = Point - ((Point - PointOnPlane) . Normal) * Normal
            return point - ((point - GetPointOnPlane(plane)).Dot(plane.Normal)) * plane.Normal;
        }

        /// <inheritdoc cref="Plane.DotCoordinate"/>
        public static float DotCoordinate(this Plane plane, Vector3 coordinate)
        {
            return Plane.DotCoordinate(plane, coordinate);
        }

        /// <summary>
        /// Gets the axis closest to the normal of this plane
        /// </summary>
        /// <returns>Vector3.UnitX, Vector3.UnitY, or Vector3.UnitZ depending on the plane's normal</returns>
        public static Vector3 GetClosestAxisToNormal(this Plane plane) => ClosestAxis(plane.Normal);

        /// <summary>
        /// Intersects three planes and gets the point of their intersection.
        /// </summary>
        /// <returns>The point that the planes intersect at, or null if they do not intersect at a point.</returns>
        public static Vector3? IntersectPlanes(Plane p1, Plane p2, Plane p3)
        {
            // http://paulbourke.net/geometry/3planes/

            var c1 = p2.Normal.Cross(p3.Normal);
            var c2 = p3.Normal.Cross(p1.Normal);
            var c3 = p1.Normal.Cross(p2.Normal);

            var denom = p1.Normal.Dot(c1);
            if (denom < 0.00001d) return null; // No intersection, planes must be parallel

            var numer = (-p1.D * c1) + (-p2.D * c2) + (-p3.D * c3);
            return numer / denom;
        }

        public static bool EquivalentTo(this Plane plane, Plane other, float delta = 0.0001f)
        {
            return plane.Normal.EquivalentTo(other.Normal, delta)
                   && Math.Abs(plane.D - other.D) < delta;
        }

        public static Planed ToPlaned(this Plane plane)
        {
            return new Planed(plane.Normal.ToVector3d(), plane.D);
        }

        public static Plane Flip(this Plane plane)
        {
            return new Plane(-plane.Normal, -plane.D);
        }

        // Matrix
        public static Vector3 Transform(this Matrix4x4 self, Vector3 vector) => Vector3.Transform(vector, self);

        // https://github.com/ericwa/ericw-tools/blob/master/qbsp/map.cc @TextureAxisFromPlane
        // ReSharper disable once UnusedTupleComponentInReturnValue
        public static (Vector3 uAxis, Vector3 vAxis, Vector3 snappedNormal) GetQuakeTextureAxes(this Plane plane)
        {
            var baseaxis = new[]
            {
                new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, -1, 0), // floor
                new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector3(0, -1, 0), // ceiling
                new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, -1), // west wall
                new Vector3(-1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, -1), // east wall
                new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, -1), // south wall
                new Vector3(0, -1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, -1) // north wall
            };

            var best = 0f;
            var bestaxis = 0;

            for (var i = 0; i < 6; i++)
            {
                var dot = plane.Normal.Dot(baseaxis[i * 3]);
                if (!(dot > best)) continue;

                best = dot;
                bestaxis = i;
            }

            return (baseaxis[bestaxis * 3 + 1], baseaxis[bestaxis * 3 + 2], baseaxis[bestaxis * 3]);
        }
    }
}