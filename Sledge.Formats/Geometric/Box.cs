using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sledge.Formats.Geometric
{
    /// <summary>
    /// An axis-aligned bounding box
    /// </summary>
    public class Box : IEquatable<Box>
    {
        /// <summary>
        /// An empty box with both the start and end vectors being zero.
        /// </summary>
        public static readonly Box Empty = new Box(Vector3.Zero, Vector3.Zero);

        /// <summary>
        /// The minimum corner of the box
        /// </summary>
        public Vector3 Start { get; }

        /// <summary>
        /// The maximum corner of the box
        /// </summary>
        public Vector3 End { get; }

        /// <summary>
        /// The center of the box
        /// </summary>
        public Vector3 Center => (Start + End) / 2f;

        /// <summary>
        /// The X value difference of this box
        /// </summary>
        public float Width => End.X - Start.X;

        /// <summary>
        /// The Y value difference of this box
        /// </summary>
        public float Length => End.Y - Start.Y;

        /// <summary>
        /// The Z value difference of this box
        /// </summary>
        public float Height => End.Z - Start.Z;

        /// <summary>
        /// Get the smallest dimension of this box
        /// </summary>
        public float SmallestDimension => Math.Min(Width, Math.Min(Length, Height));

        /// <summary>
        /// Get the largest dimension of this box
        /// </summary>
        public float LargestDimension => Math.Max(Width, Math.Max(Length, Height));

        /// <summary>
        /// Get the width (X), length (Y), and height (Z) of this box as a vector.
        /// </summary>
        public Vector3 Dimensions => new Vector3(Width, Length, Height);

        /// <summary>
        /// Create a box from the given start and end points.
        /// The resulting box is not guaranteed to have identical start and end vectors as provided - the
        /// resulting box will have the start and end points set to the true minimum/maximum values of
        /// each dimension (X, Y, Z).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Box(Vector3 start, Vector3 end) : this(new[] { start, end })
        {
        }

        /// <summary>
        /// Create a box from the given list of vectors
        /// </summary>
        /// <param name="vectors">The list of vectors to create the box from. There must be at least one vector in the list.</param>
        /// <exception cref="InvalidOperationException">If the list of vectors is empty</exception>
        public Box(IEnumerable<Vector3> vectors)
        {
            var list = vectors.ToList();
            if (!list.Any())
            {
                throw new ArgumentException("Cannot create a bounding box out of zero Vectors.", nameof(list));
            }
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (var vertex in list)
            {
                min.X = Math.Min(vertex.X, min.X);
                min.Y = Math.Min(vertex.Y, min.Y);
                min.Z = Math.Min(vertex.Z, min.Z);
                max.X = Math.Max(vertex.X, max.X);
                max.Y = Math.Max(vertex.Y, max.Y);
                max.Z = Math.Max(vertex.Z, max.Z);
            }
            Start = min;
            End = max;
        }

        /// <summary>
        /// Create a box from the given list of boxes
        /// </summary>
        /// <param name="boxes">The list of boxes to create the box from. There must be at least one box in the list.</param>
        /// <exception cref="InvalidOperationException">If the list of boxes is empty</exception>
        public Box(IEnumerable<Box> boxes)
        {
            var list = boxes.ToList();
            if (!list.Any())
            {
                throw new ArgumentException("Cannot create a bounding box out of zero other boxes.", nameof(boxes));
            }
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (var box in list)
            {
                min.X = Math.Min(box.Start.X, min.X);
                min.Y = Math.Min(box.Start.Y, min.Y);
                min.Z = Math.Min(box.Start.Z, min.Z);
                max.X = Math.Max(box.End.X, max.X);
                max.Y = Math.Max(box.End.Y, max.Y);
                max.Z = Math.Max(box.End.Z, max.Z);
            }
            Start = min;
            End = max;
        }

        /// <summary>
        /// The box is considered empty if the width, height, and length are all less than the provided epsilon
        /// </summary>
        /// <returns>True if the box is empty</returns>
        public bool IsEmpty(float epsilon = 0.0001f)
        {
            return Math.Abs(Width) < epsilon && Math.Abs(Height) < epsilon && Math.Abs(Length) < epsilon;
        }

        /// <summary>
        /// Get the 8 corners of the box
        /// </summary>
        /// <returns>A list of 8 points</returns>
        public IEnumerable<Vector3> GetBoxPoints()
        {
            yield return new Vector3(Start.X, End.Y, End.Z);
            yield return End;
            yield return new Vector3(Start.X, Start.Y, End.Z);
            yield return new Vector3(End.X, Start.Y, End.Z);

            yield return new Vector3(Start.X, End.Y, Start.Z);
            yield return new Vector3(End.X, End.Y, Start.Z);
            yield return Start;
            yield return new Vector3(End.X, Start.Y, Start.Z);
        }

        /// <summary>
        /// Create a polyhedron from this box
        /// </summary>
        /// <returns>This box as a polyhedron</returns>
        public Polyhedron ToPolyhedron()
        {
            return new Polyhedron(GetBoxFaces());
        }

        /// <summary>
        /// Get the 6 planes representing the sides of this box
        /// </summary>
        /// <returns>A list of 6 planes</returns>
        public IEnumerable<Plane> GetBoxPlanes()
        {
            return GetBoxFaces().Select(x => x.Plane);
        }

        /// <summary>
        /// Get the 6 polygons representing the sides of this box
        /// </summary>
        /// <returns>A list of 6 polygons</returns>
        public IEnumerable<Polygon> GetBoxFaces()
        {
            var topLeftBack = new Vector3(Start.X, End.Y, End.Z);
            var topRightBack = End;
            var topLeftFront = new Vector3(Start.X, Start.Y, End.Z);
            var topRightFront = new Vector3(End.X, Start.Y, End.Z);

            var bottomLeftBack = new Vector3(Start.X, End.Y, Start.Z);
            var bottomRightBack = new Vector3(End.X, End.Y, Start.Z);
            var bottomLeftFront = Start;
            var bottomRightFront = new Vector3(End.X, Start.Y, Start.Z);

            return new[]
            {
                new Polygon(bottomLeftFront, bottomRightFront, topRightFront, topLeftFront),
                new Polygon(bottomRightBack, bottomLeftBack, topLeftBack, topRightBack),
                new Polygon(bottomLeftBack, bottomLeftFront, topLeftFront, topLeftBack),
                new Polygon(bottomRightFront, bottomRightBack, topRightBack, topRightFront),
                new Polygon(topLeftFront, topRightFront, topRightBack, topLeftBack),
                new Polygon(bottomLeftBack, bottomRightBack, bottomRightFront, bottomLeftFront),
            };
        }

        /// <summary>
        /// Get the 12 lines representing the edges of this box
        /// </summary>
        /// <returns>A list of 12 lines</returns>
        public IEnumerable<Line> GetBoxLines()
        {
            var topLeftBack = new Vector3(Start.X, End.Y, End.Z);
            var topRightBack = End;
            var topLeftFront = new Vector3(Start.X, Start.Y, End.Z);
            var topRightFront = new Vector3(End.X, Start.Y, End.Z);

            var bottomLeftBack = new Vector3(Start.X, End.Y, Start.Z);
            var bottomRightBack = new Vector3(End.X, End.Y, Start.Z);
            var bottomLeftFront = Start;
            var bottomRightFront = new Vector3(End.X, Start.Y, Start.Z);

            yield return new Line(topLeftBack, topRightBack);
            yield return new Line(topLeftFront, topRightFront);
            yield return new Line(topLeftBack, topLeftFront);
            yield return new Line(topRightBack, topRightFront);

            yield return new Line(topLeftBack, bottomLeftBack);
            yield return new Line(topLeftFront, bottomLeftFront);
            yield return new Line(topRightBack, bottomRightBack);
            yield return new Line(topRightFront, bottomRightFront);

            yield return new Line(bottomLeftBack, bottomRightBack);
            yield return new Line(bottomLeftFront, bottomRightFront);
            yield return new Line(bottomLeftBack, bottomLeftFront);
            yield return new Line(bottomRightBack, bottomRightFront);
        }

        /// <summary>
        /// Returns true if this box overlaps the given box in any way
        /// </summary>
        public bool IntersectsWith(Box that)
        {
            if (Start.X >= that.End.X) return false;
            if (that.Start.X >= End.X) return false;

            if (Start.Y >= that.End.Y) return false;
            if (that.Start.Y >= End.Y) return false;

            if (Start.Z >= that.End.Z) return false;
            if (that.Start.Z >= End.Z) return false;

            return true;
        }

        /// <summary>
        /// Returns true if this box is completely inside the given box
        /// </summary>
        public bool ContainedWithin(Box that)
        {
            if (Start.X < that.Start.X) return false;
            if (Start.Y < that.Start.Y) return false;
            if (Start.Z < that.Start.Z) return false;

            if (End.X > that.End.X) return false;
            if (End.Y > that.End.Y) return false;
            if (End.Z > that.End.Z) return false;

            return true;
        }

        /* http://www.gamedev.net/community/forums/topic.asp?topic_id=338987 */
        /// <summary>
        /// Returns true if this box intersects the given line
        /// </summary>
        public bool IntersectsWith(Line that)
        {
            var start = that.Start;
            var finish = that.End;

            if (start.X < Start.X && finish.X < Start.X) return false;
            if (start.X > End.X && finish.X > End.X) return false;

            if (start.Y < Start.Y && finish.Y < Start.Y) return false;
            if (start.Y > End.Y && finish.Y > End.Y) return false;

            if (start.Z < Start.Z && finish.Z < Start.Z) return false;
            if (start.Z > End.Z && finish.Z > End.Z) return false;

            var d = (finish - start) / 2;
            var e = (End - Start) / 2;
            var c = start + d - ((Start + End) / 2);
            var ad = d.Absolute();

            if (Math.Abs(c.X) > e.X + ad.X) return false;
            if (Math.Abs(c.Y) > e.Y + ad.Y) return false;
            if (Math.Abs(c.Z) > e.Z + ad.Z) return false;

            var dca = d.Cross(c).Absolute();

            if (dca.X > e.Y * ad.Z + e.Z * ad.Y) return false;
            if (dca.Y > e.Z * ad.X + e.X * ad.Z) return false;
            if (dca.Z > e.X * ad.Y + e.Y * ad.X) return false;

            return true;
        }

        /// <summary>
        /// Returns true if the given Vector3 is inside this box.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool Vector3IsInside(Vector3 c)
        {
            return c.X >= Start.X && c.Y >= Start.Y && c.Z >= Start.Z
                   && c.X <= End.X && c.Y <= End.Y && c.Z <= End.Z;
        }

        /// <summary>
        /// Transform this box. Each corner of the box will be transformed, and then a new box will be created using those points.
        /// The dimensions of the resulting box may change if the transform isn't a simple translation.
        /// </summary>
        /// <param name="transform">The transformation to apply</param>
        /// <returns>A new box after the transformation has been applied</returns>
        public Box Transform(Matrix4x4 transform)
        {
            return new Box(GetBoxPoints().Select(x => Vector3.Transform(x, transform)));
        }

        /// <summary>
        /// Create a copy of this box.
        /// </summary>
        /// <returns>A copy of the box</returns>
        public Box Clone()
        {
            return new Box(Start, End);
        }

        public bool Equals(Box other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Start.Equals(other.Start) && End.Equals(other.End);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Box)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }

        public static bool operator ==(Box left, Box right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Box left, Box right)
        {
            return !Equals(left, right);
        }
    }
}
