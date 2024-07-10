using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Sledge.Formats.Geometric;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Factories.Solids
{
    public class UVSphereSolidFactory : SolidFactoryBase
    {
        public override string Name => "UV Sphere";

        [DisplayName("Number of segments (vertical)")]
        [Description("The number of segments the sphere will have vertically")]
        [MapObjectFactoryPropertyData(MinValue = 3)]
        public int NumberOfSegmentsVertical { get; set; }

        [DisplayName("Number of segments (horizontal)")]
        [Description("The number of segments the sphere will have horizontally")]
        [MapObjectFactoryPropertyData(MinValue = 3)]
        public int NumberOfSegmentsHorizontal { get; set; }

        public override IEnumerable<MapObject> Create(Box box)
        {
            var numSidesV = NumberOfSegmentsVertical;
            if (numSidesV < 3) throw new ArgumentException("NumberOfSegmentsVertical must be >= 3", nameof(NumberOfSegmentsVertical));

            var numSidesH = NumberOfSegmentsHorizontal;
            if (numSidesH < 3) throw new ArgumentException("NumberOfSegmentsHorizontal must be >= 3", nameof(NumberOfSegmentsHorizontal));

            var roundDecimals = Math.Max(2, RoundDecimals); // don't support rounding < 2 because it would result in invalid faces too often

            var width = box.Width;
            var length = box.Length;
            var height = box.Height;
            var major = width / 2;
            var minor = length / 2;
            var heightRadius = height / 2;

            var angleV = (float)Math.PI / numSidesV;
            var angleH = (float)(Math.PI * 2) / numSidesH;

            var faces = new List<(string textureName, Vector3[] points)>();
            var bottom = new Vector3(box.Center.X, box.Center.Y, box.Start.Z).Round(roundDecimals);
            var top = new Vector3(box.Center.X, box.Center.Y, box.End.Z).Round(roundDecimals);

            for (var i = 0; i < numSidesV; i++)
            {
                // Top -> bottom
                var zAngleStart = angleV * i;
                var zAngleEnd = angleV * (i + 1);
                var zStart = heightRadius * (float)Math.Cos(zAngleStart);
                var zEnd = heightRadius * (float)Math.Cos(zAngleEnd);
                var zMultStart = (float)Math.Sin(zAngleStart);
                var zMultEnd = (float)Math.Sin(zAngleEnd);
                for (var j = 0; j < numSidesH; j++)
                {
                    // Go around the circle in X/Y
                    var xyAngleStart = angleH * j;
                    var xyAngleEnd = angleH * ((j + 1) % numSidesH);
                    var xyStartX = major * (float)Math.Cos(xyAngleStart);
                    var xyStartY = minor * (float)Math.Sin(xyAngleStart);
                    var xyEndX = major * (float)Math.Cos(xyAngleEnd);
                    var xyEndY = minor * (float)Math.Sin(xyAngleEnd);
                    var one = (new Vector3(xyStartX * zMultStart, xyStartY * zMultStart, zStart) + box.Center).Round(roundDecimals);
                    var two = (new Vector3(xyEndX * zMultStart, xyEndY * zMultStart, zStart) + box.Center).Round(roundDecimals);
                    var three = (new Vector3(xyEndX * zMultEnd, xyEndY * zMultEnd, zEnd) + box.Center).Round(roundDecimals);
                    var four = (new Vector3(xyStartX * zMultEnd, xyStartY * zMultEnd, zEnd) + box.Center).Round(roundDecimals);
                    if (i == 0)
                    {
                        // Top faces are triangles
                        faces.Add((VisibleTextureName, new[] { four, three, top }));
                    }
                    else if (i == numSidesV - 1)
                    {
                        // Bottom faces are also triangles
                        faces.Add((VisibleTextureName, new[] { two, one, bottom }));
                    }
                    else
                    {
                        // Inner faces are quads
                        faces.Add((VisibleTextureName, new[] { four, three, two, one }));
                    }
                }
            }
            yield return MakeSolid(faces, ColorUtils.GetRandomBrushColour());
        }
    }
}
