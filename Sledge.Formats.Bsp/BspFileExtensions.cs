using Sledge.Formats.Bsp.Objects;
using System;
using System.Numerics;

namespace Sledge.Formats.Bsp
{
    public static class BspFileExtensions
    {
        public static PlaneType GetBspPlaneTypeForNormal(this Vector3 normal)
        {
            const float epsilon = 0.0001f;

            var ax = Math.Abs(normal.X);
            var ay = Math.Abs(normal.Y);
            var az = Math.Abs(normal.Z);

            if (ax > 1.0 - epsilon && ay < epsilon && az < epsilon) return PlaneType.X;
            if (ay > 1.0 - epsilon && az < epsilon && ax < epsilon) return PlaneType.Y;
            if (az > 1.0 - epsilon && ax < epsilon && ay < epsilon) return PlaneType.Z;
            if (ax >= ay && ax >= az) return PlaneType.AnyX;
            if (ay >= ax && ay >= az) return PlaneType.AnyY;
            return PlaneType.AnyZ;
        }
    }
}
