using System.Numerics;

namespace Sledge.Formats.Bsp.Objects
{
    public class Plane
    {
        public Vector3 Normal { get; set; }
        public float Distance { get; set; }
        public PlaneType Type { get; set; }

        public float Evaluate(Vector3 point)
        {
            if (Type <= PlaneType.Z)
            {
                return BspFileExtensions.GetDimensionForSimplePlaneType(point, Type) - Distance;
            }
            else
            {
                return Vector3.Dot(Normal, point) - Distance;
            }
        }

        private bool Equals(Plane other)
        {
            return Normal.Equals(other.Normal) && Distance.Equals(other.Distance) && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Plane)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Normal.GetHashCode();
                hashCode = (hashCode * 397) ^ Distance.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Type;
                return hashCode;
            }
        }
    }
}