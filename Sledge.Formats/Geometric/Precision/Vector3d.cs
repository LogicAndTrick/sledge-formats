using System;
using System.Globalization;
using System.Numerics;

namespace Sledge.Formats.Geometric.Precision
{
    /// <summary>
    /// A 3-dimensional immutable vector that uses double precision floating points.
    /// </summary>
    public struct Vector3d
    {
        public static readonly Vector3d MaxValue = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
        public static readonly Vector3d MinValue = new Vector3d(double.MinValue, double.MinValue, double.MinValue);
        public static readonly Vector3d Zero = new Vector3d(0, 0, 0);
        public static readonly Vector3d One = new Vector3d(1, 1, 1);
        public static readonly Vector3d UnitX = new Vector3d(1, 0, 0);
        public static readonly Vector3d UnitY = new Vector3d(0, 1, 0);
        public static readonly Vector3d UnitZ = new Vector3d(0, 0, 1);

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool EquivalentTo(Vector3d test, double delta = 0.0001d)
        {
            var xd = Math.Abs(X - test.X);
            var yd = Math.Abs(Y - test.Y);
            var zd = Math.Abs(Z - test.Z);
            return xd < delta && yd < delta && zd < delta;
        }

        public bool EquivalentTo(Vector3 test, double delta = 0.0001d)
        {
            var xd = Math.Abs(X - test.X);
            var yd = Math.Abs(Y - test.Y);
            var zd = Math.Abs(Z - test.Z);
            return xd < delta && yd < delta && zd < delta;
        }

        public double Dot(Vector3d c)
        {
            return X * c.X + Y * c.Y + Z * c.Z;
        }

        public Vector3d Cross(Vector3d that)
        {
            var xv = Y * that.Z - Z * that.Y;
            var yv = Z * that.X - X * that.Z;
            var zv = X * that.Y - Y * that.X;
            return new Vector3d(xv, yv, zv);
        }

        public Vector3d Round(int num = 8)
        {
            return new Vector3d(Math.Round(X, num), Math.Round(Y, num), Math.Round(Z, num));
        }

        public Vector3d Snap(double snapTo)
        {
            return new Vector3d(
                Math.Round(X / snapTo) * snapTo,
                Math.Round(Y / snapTo) * snapTo,
                Math.Round(Z / snapTo) * snapTo
            );
        }

        public double Length()
        {
            return Math.Sqrt(LengthSquared());
        }

        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public Vector3d Normalise()
        {
            var len = Length();
            return Math.Abs(len) < 0.0001 ? new Vector3d(0, 0, 0) : new Vector3d(X / len, Y / len, Z / len);
        }

        public Vector3d Absolute()
        {
            return new Vector3d(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
        }

        public static Vector3d operator +(Vector3d c1, Vector3d c2)
        {
            return new Vector3d(c1.X + c2.X, c1.Y + c2.Y, c1.Z + c2.Z);
        }

        public static Vector3d operator -(Vector3d c1, Vector3d c2)
        {
            return new Vector3d(c1.X - c2.X, c1.Y - c2.Y, c1.Z - c2.Z);
        }

        public static Vector3d operator -(Vector3d c1)
        {
            return new Vector3d(-c1.X, -c1.Y, -c1.Z);
        }

        public static Vector3d operator /(Vector3d c, double f)
        {
            return Math.Abs(f) < 0.0001 ? new Vector3d(0, 0, 0) : new Vector3d(c.X / f, c.Y / f, c.Z / f);
        }

        public static Vector3d operator *(Vector3d c, double f)
        {
            return new Vector3d(c.X * f, c.Y * f, c.Z * f);
        }

        public static Vector3d operator *(Vector3d c, Vector3d f)
        {
            return new Vector3d(c.X * f.X, c.Y * f.Y, c.Z * f.Z);
        }

        public static Vector3d operator /(Vector3d c, Vector3d f)
        {
            return new Vector3d(c.X / f.X, c.Y / f.Y, c.Z / f.Z);
        }

        public static Vector3d operator *(double f, Vector3d c)
        {
            return c * f;
        }

        public bool Equals(Vector3d other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vector3d other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = hashCode * 397 ^ Y.GetHashCode();
                hashCode = hashCode * 397 ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Vector3d left, Vector3d right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3d left, Vector3d right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return "(" + X.ToString("0.0000", CultureInfo.InvariantCulture) + " " + Y.ToString("0.0000", CultureInfo.InvariantCulture) + " " + Z.ToString("0.0000", CultureInfo.InvariantCulture) + ")";
        }

        public Vector3d Clone()
        {
            return new Vector3d(X, Y, Z);
        }

        public static Vector3d Parse(string x, string y, string z)
        {
            const NumberStyles ns = NumberStyles.Float;
            return new Vector3d(double.Parse(x, ns, CultureInfo.InvariantCulture), double.Parse(y, ns, CultureInfo.InvariantCulture), double.Parse(z, ns, CultureInfo.InvariantCulture));
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)X, (float)Y, (float)Z);
        }

        public static Vector3d Transform(Vector3d vec, Matrix4x4 transform)
        {
            return Vector3.Transform(vec.ToVector3(), transform).ToVector3d();
        }
    }
}