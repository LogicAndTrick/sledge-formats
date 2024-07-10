using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Sledge.Formats.Geometric;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Factories.Solids
{
    public class ConeSolidFactory : SolidFactoryBase
    {
        public override string Name => "Cone";

        [DisplayName("Number of sides")]
        [Description("The number of sides the base of the cone will have")]
        [MapObjectFactoryPropertyData(MinValue = 3)]
        public int NumberOfSides { get; set; }

        public override IEnumerable<MapObject> Create(Box box)
        {
            var numSides = NumberOfSides;
            if (numSides < 3) throw new ArgumentException("NumberOfSides must be >= 3", nameof(NumberOfSides));

            // This is all very similar to the cylinder brush.
            var width = box.Width;
            var length = box.Length;
            var major = width / 2;
            var minor = length / 2;
            var angle = 2 * Math.PI / numSides;

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

            var point = new Vector3(box.Center.X, box.Center.Y, box.End.Z).Round(RoundDecimals);
            for (var i = 0; i < numSides; i++)
            {
                var next = (i + 1) % numSides;
                faces.Add(new[] { points[next], points[i], point });
            }
            faces.Add(points.ToArray());

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