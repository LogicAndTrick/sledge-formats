using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using Sledge.Formats.Geometric;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Factories.Solids
{
    public class CylinderSolidFactory : SolidFactoryBase
    {
        public override string Name => "Cone";

        [DisplayName("Number of sides")]
        [Description("The number of sides the base of the cylinder will have")]
        [MapObjectFactoryPropertyData(MinValue = 3)]
        public int NumberOfSides { get; set; }

        public override IEnumerable<MapObject> Create(Box box)
        {
            var numSides = NumberOfSides;
            if (numSides < 3) throw new ArgumentException("NumberOfSides must be >= 3", nameof(NumberOfSides));

            // Cylinders can be elliptical so use both major and minor rather than just the radius
            // NOTE: when a low number (< 10ish) of faces are selected this will cause the cylinder to not touch all the edges of the box.
            var width = box.Width;
            var length = box.Length;
            var height = box.Height;
            var major = width / 2;
            var minor = length / 2;
            var angle = 2 * (float)Math.PI / numSides;

            // Calculate the X and Y points for the ellipse
            var points = new List<Vector3>();
            for (var i = 0; i < numSides; i++)
            {
                var a = i * angle;
                var xval = box.Center.X + major * (float)Math.Cos(a);
                var yval = box.Center.Y + minor * (float)Math.Sin(a);
                var zval = box.Start.Z;
                points.Add(new Vector3(xval, yval, zval).Round(RoundDecimals));
            }
            points.Reverse();

            var faces = new List<Vector3[]>();

            // Add the vertical faces
            var z = new Vector3(0, 0, height).Round(RoundDecimals);
            for (var i = 0; i < numSides; i++)
            {
                var next = (i + 1) % numSides;
                faces.Add(new[] { points[i], points[i] + z, points[next] + z, points[next] });
            }

            // Add the elliptical top and bottom faces
            faces.Add(points.ToArray());
            faces.Add(points.Select(x => x + z).Reverse().ToArray());

            // Nothing new here, move along
            var solid = new Solid
            {
                Color = ColorUtils.GetRandomBrushColour()
            };

            foreach (var arr in faces)
            {
                var face = new Face
                {
                    Plane = Plane.CreateFromVertices(arr[0], arr[1], arr[2]),
                    TextureName = VisibleTextureName
                };
                face.Vertices.AddRange(arr);
                solid.Faces.Add(face);
            }
            yield return solid;
        }
    }
}