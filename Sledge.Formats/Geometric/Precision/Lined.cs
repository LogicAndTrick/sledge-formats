using System;
using System.Numerics;

namespace Sledge.Formats.Geometric.Precision
{
    public class Lined
    {
        public Vector3d Start { get; }
        public Vector3d End { get; }

        public static readonly Lined AxisX = new Lined(Vector3d.Zero, Vector3d.UnitX);
        public static readonly Lined AxisY = new Lined(Vector3d.Zero, Vector3d.UnitY);
        public static readonly Lined AxisZ = new Lined(Vector3d.Zero, Vector3d.UnitZ);

        public Lined(Vector3 start, Vector3 end) : this(start.ToVector3d(), end.ToVector3d())
        {
            //
        }

        public Lined(Vector3d start, Vector3d end)
        {
            Start = start;
            End = end;
        }

        public Lined Reverse()
        {
            return new Lined(End, Start);
        }

        public Vector3d ClosestPoint(Vector3d point)
        {
            // http://paulbourke.net/geometry/pointline/

            var delta = End - Start;
            var den = delta.LengthSquared();
            if (Math.Abs(den) < 0.0001f) return Start; // Start and End are the same

            var numPoint = (point - Start) * delta;
            var num = numPoint.X + numPoint.Y + numPoint.Z;
            var u = num / den;

            if (u < 0) return Start; // Point is before the segment start
            if (u > 1) return End;   // Point is after the segment end
            return Start + u * delta;
        }

        public bool EquivalentTo(Lined other, float delta = 0.0001f)
        {
            return Start.EquivalentTo(other.Start, delta) && End.EquivalentTo(other.End, delta)
                || End.EquivalentTo(other.Start, delta) && Start.EquivalentTo(other.End, delta);
        }

        public Line ToLine()
        {
            return new Line(Start.ToVector3(), End.ToVector3());
        }

        private bool Equals(Lined other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Start, Start) && Equals(other.End, End)
                || Equals(other.End, Start) && Equals(other.Start, End);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Lined)) return false;
            return Equals((Lined)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }

        public static bool operator ==(Lined left, Lined right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Lined left, Lined right)
        {
            return !Equals(left, right);
        }
    }
}
