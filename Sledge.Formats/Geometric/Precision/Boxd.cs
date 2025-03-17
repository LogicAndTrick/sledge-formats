using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sledge.Formats.Geometric.Precision
{
    /// <summary>
    /// An axis-aligned bounding box. Uses double precision floating points.
    /// </summary>
    public class Boxd : IEquatable<Boxd>
    {
        /// <summary>
        /// An empty box with both the start and end vectors being zero.
        /// </summary>
        public static readonly Boxd Empty = new Boxd(Vector3d.Zero, Vector3d.Zero);

        /// <summary>
        /// The minimum corner of the box
        /// </summary>
        public Vector3d Start { get; }

        /// <summary>
        /// The maximum corner of the box
        /// </summary>
        public Vector3d End { get; }

        /// <summary>
        /// The center of the box
        /// </summary>
        public Vector3d Center => (Start + End) / 2f;

        /// <summary>
        /// The X value difference of this box
        /// </summary>
        public double Width => End.X - Start.X;

        /// <summary>
        /// The Y value difference of this box
        /// </summary>
        public double Length => End.Y - Start.Y;

        /// <summary>
        /// The Z value difference of this box
        /// </summary>
        public double Height => End.Z - Start.Z;

        /// <summary>
        /// Get the smallest dimension of this box
        /// </summary>
        public double SmallestDimension => Math.Min(Width, Math.Min(Length, Height));

        /// <summary>
        /// Get the largest dimension of this box
        /// </summary>
        public double LargestDimension => Math.Max(Width, Math.Max(Length, Height));

        /// <summary>
        /// Get the width (X), length (Y), and height (Z) of this box as a vector.
        /// </summary>
        public Vector3d Dimensions => new Vector3d(Width, Length, Height);

        /// <summary>
        /// Create a box from the given start and end points.
        /// The resulting box is not guaranteed to have identical start and end vectors as provided - the
        /// resulting box will have the start and end points set to the true minimum/maximum values of
        /// each dimension (X, Y, Z).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Boxd(Vector3d start, Vector3d end) : this(new[] { start, end })
        {
        }

        /// <summary>
        /// Create a box from the given list of vectors
        /// </summary>
        /// <param name="vectors">The list of vectors to create the box from. There must be at least one vector in the list.</param>
        /// <exception cref="InvalidOperationException">If the list of vectors is empty</exception>
        public Boxd(IEnumerable<Vector3d> vectors)
        {
            var list = vectors.ToList();
            if (!list.Any())
            {
                throw new ArgumentException("Cannot create a bounding box out of zero Vectors.", nameof(list));
            }
            var min = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
            var max = new Vector3d(double.MinValue, double.MinValue, double.MinValue);
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
        public Boxd(IEnumerable<Boxd> boxes)
        {
            var list = boxes.ToList();
            if (!list.Any())
            {
                throw new ArgumentException("Cannot create a bounding box out of zero other boxes.", nameof(boxes));
            }
            var min = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
            var max = new Vector3d(double.MinValue, double.MinValue, double.MinValue);
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
        public bool IsEmpty(double epsilon = 0.0001f)
        {
            return Math.Abs(Width) < epsilon && Math.Abs(Height) < epsilon && Math.Abs(Length) < epsilon;
        }

        /// <summary>
        /// Get the 8 corners of the box
        /// </summary>
        /// <returns>A list of 8 points</returns>
        public IEnumerable<Vector3d> GetBoxPoints()
        {
            yield return new Vector3d(Start.X, End.Y, End.Z);
            yield return End;
            yield return new Vector3d(Start.X, Start.Y, End.Z);
            yield return new Vector3d(End.X, Start.Y, End.Z);

            yield return new Vector3d(Start.X, End.Y, Start.Z);
            yield return new Vector3d(End.X, End.Y, Start.Z);
            yield return Start;
            yield return new Vector3d(End.X, Start.Y, Start.Z);
        }

        /// <summary>
        /// Create a polyhedron from this box
        /// </summary>
        /// <returns>This box as a polyhedron</returns>
        public Polyhedrond ToPolyhedrond()
        {
            return new Polyhedrond(GetBoxFaces());
        }

        /// <summary>
        /// Get the 6 planes representing the sides of this box
        /// </summary>
        /// <returns>A list of 6 planes</returns>
        public IEnumerable<Planed> GetBoxPlanes()
        {
            return GetBoxFaces().Select(x => x.Plane);
        }

        /// <summary>
        /// Get the 6 polygons representing the sides of this box
        /// </summary>
        /// <returns>A list of 6 polygons</returns>
        public IEnumerable<Polygond> GetBoxFaces()
        {
            var topLeftBack = new Vector3d(Start.X, End.Y, End.Z);
            var topRightBack = End;
            var topLeftFront = new Vector3d(Start.X, Start.Y, End.Z);
            var topRightFront = new Vector3d(End.X, Start.Y, End.Z);

            var bottomLeftBack = new Vector3d(Start.X, End.Y, Start.Z);
            var bottomRightBack = new Vector3d(End.X, End.Y, Start.Z);
            var bottomLeftFront = Start;
            var bottomRightFront = new Vector3d(End.X, Start.Y, Start.Z);

            return new[]
            {
                new Polygond(topLeftFront, topRightFront, bottomRightFront, bottomLeftFront),
                new Polygond(topRightBack, topLeftBack, bottomLeftBack, bottomRightBack),
                new Polygond(topLeftBack, topLeftFront, bottomLeftFront, bottomLeftBack),
                new Polygond(topRightFront, topRightBack, bottomRightBack, bottomRightFront),
                new Polygond(topLeftBack, topRightBack, topRightFront, topLeftFront),
                new Polygond(bottomLeftFront, bottomRightFront, bottomRightBack, bottomLeftBack)
            };
        }

        /// <summary>
        /// Get the 12 lines representing the edges of this box
        /// </summary>
        /// <returns>A list of 12 lines</returns>
        public IEnumerable<Lined> GetBoxLines()
        {
            var topLeftBack = new Vector3d(Start.X, End.Y, End.Z);
            var topRightBack = End;
            var topLeftFront = new Vector3d(Start.X, Start.Y, End.Z);
            var topRightFront = new Vector3d(End.X, Start.Y, End.Z);

            var bottomLeftBack = new Vector3d(Start.X, End.Y, Start.Z);
            var bottomRightBack = new Vector3d(End.X, End.Y, Start.Z);
            var bottomLeftFront = Start;
            var bottomRightFront = new Vector3d(End.X, Start.Y, Start.Z);

            yield return new Lined(topLeftBack, topRightBack);
            yield return new Lined(topLeftFront, topRightFront);
            yield return new Lined(topLeftBack, topLeftFront);
            yield return new Lined(topRightBack, topRightFront);

            yield return new Lined(topLeftBack, bottomLeftBack);
            yield return new Lined(topLeftFront, bottomLeftFront);
            yield return new Lined(topRightBack, bottomRightBack);
            yield return new Lined(topRightFront, bottomRightFront);

            yield return new Lined(bottomLeftBack, bottomRightBack);
            yield return new Lined(bottomLeftFront, bottomRightFront);
            yield return new Lined(bottomLeftBack, bottomLeftFront);
            yield return new Lined(bottomRightBack, bottomRightFront);
        }

        /// <summary>
        /// Returns true if this box overlaps the given box in any way
        /// </summary>
        public bool IntersectsWith(Boxd that)
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
        public bool ContainedWithin(Boxd that)
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
        public bool IntersectsWith(Lined that)
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
        /// Returns true if the given Vector3d is inside this box.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool Vector3IsInside(Vector3d c)
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
        public Boxd Transform(Matrix4x4 transform)
        {
            return new Boxd(GetBoxPoints().Select(x => Vector3d.Transform(x, transform)));
        }

        /// <summary>
        /// Create a copy of this box.
        /// </summary>
        /// <returns>A copy of the box</returns>
        public Boxd Clone()
        {
            return new Boxd(Start, End);
        }

        public bool Equals(Boxd other)
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
            return Equals((Boxd)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }

        public static bool operator ==(Boxd left, Boxd right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Boxd left, Boxd right)
        {
            return !Equals(left, right);
        }
    }
}
