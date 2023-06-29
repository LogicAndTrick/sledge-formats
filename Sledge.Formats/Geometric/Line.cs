using System;
using System.Numerics;
using Sledge.Formats.Geometric.Precision;

namespace Sledge.Formats.Geometric
{
    public class Line
    {
        public Vector3 Start { get; }
        public Vector3 End { get; }

        public static readonly Line AxisX = new Line(Vector3.Zero, Vector3.UnitX);
        public static readonly Line AxisY = new Line(Vector3.Zero, Vector3.UnitY);
        public static readonly Line AxisZ = new Line(Vector3.Zero, Vector3.UnitZ);

        public Line(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }

        public Line Reverse()
        {
            return new Line(End, Start);
        }

        public Vector3 ClosestPoint(Vector3 point)
        {
            // http://paulbourke.net/geometry/pointline/

            var delta = End - Start;
            var den = delta.LengthSquared();
            if (Math.Abs(den) < 0.0001f) return Start; // Start and End are the same

            var numPoint = Vector3.Multiply(point - Start, delta);
            var num = numPoint.X + numPoint.Y + numPoint.Z;
            var u = num / den;

            if (u < 0) return Start; // Point is before the segment start
            if (u > 1) return End;   // Point is after the segment end
            return Start + u * delta;
        }

        public bool EquivalentTo(Line other, float delta = 0.0001f)
        {
            return Start.EquivalentTo(other.Start, delta) && End.EquivalentTo(other.End, delta)
                || End.EquivalentTo(other.Start, delta) && Start.EquivalentTo(other.End, delta);
        }

        public Lined ToLined()
        {
            return new Lined(Start.ToVector3d(), End.ToVector3d());
        }

        private bool Equals(Line other)
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
            if (obj.GetType() != typeof(Line)) return false;
            return Equals((Line)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }

        public static bool operator ==(Line left, Line right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Line left, Line right)
        {
            return !Equals(left, right);
        }
    }
}
