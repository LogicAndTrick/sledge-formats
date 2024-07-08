using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using Sledge.Formats.Geometric;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Factories.Solids
{
    public class PipeSolidFactory : SolidFactoryBase
    {
        public override string Name => "Cone";

        [DisplayName("Number of sides")]
        [Description("The number of sides the base of the pipe will have")]
        [MapObjectFactoryPropertyData(MinValue = 3)]
        public int NumberOfSides { get; set; }

        [DisplayName("Wall width")]
        [Description("The width of the pipe wall")]
        [MapObjectFactoryPropertyData(MinValue = 0.01, DecimalPrecision = 2)]
        public int WallWidth { get; set; }

        private static Solid MakeSolid(IEnumerable<(string textureName, Vector3[] points)> faces, Color col)
        {
            var solid = new Solid { Color = col };
            foreach (var (texture, arr) in faces)
            {
                var face = new Face
                {
                    Plane = Plane.CreateFromVertices(arr[0], arr[1], arr[2]),
                    TextureName = texture
                };
                face.Vertices.AddRange(arr);
                solid.Faces.Add(face);
            }
            return solid;
        }

        public override IEnumerable<MapObject> Create(Box box)
        {
            var numSides = NumberOfSides;
            if (numSides < 3) throw new ArgumentException("NumberOfSides must be >= 3", nameof(NumberOfSides));

            var wallWidth = (float) WallWidth;
            if (wallWidth < 1) yield break;

            // Very similar to the cylinder, except we have multiple solids this time
            var width = box.Width;
            var length = box.Length;
            var height = box.Height;
            var majorOut = width / 2;
            var majorIn = majorOut - wallWidth;
            var minorOut = length / 2;
            var minorIn = minorOut - wallWidth;
            var angle = 2 * Math.PI / numSides;

            // Calculate the X and Y points for the inner and outer ellipses
            var outer = new Vector3[numSides];
            var inner = new Vector3[numSides];
            for (var i = 0; i < numSides; i++)
            {
                var a = i * angle;
                var xval = box.Center.X + majorOut * (float)Math.Cos(a);
                var yval = box.Center.Y + minorOut * (float)Math.Sin(a);
                var zval = box.Start.Z;
                outer[i] = new Vector3(xval, yval, zval).Round(RoundDecimals);
                xval = box.Center.X + majorIn * (float)Math.Cos(a);
                yval = box.Center.Y + minorIn * (float)Math.Sin(a);
                inner[i] = new Vector3(xval, yval, zval).Round(RoundDecimals);
            }

            // Create the solids
            var colour = ColorUtils.GetRandomBrushColour();
            var z = new Vector3(0, 0, height).Round(RoundDecimals);
            for (var i = 0; i < numSides; i++)
            {
                var faces = new List<(string textureName, Vector3[] points)>();
                var next = (i + 1) % numSides;
                faces.Add((VisibleTextureName, new[] { outer[next], outer[next] + z, outer[i] + z, outer[i] }));
                faces.Add((VisibleTextureName, new[] { inner[i], inner[i] + z, inner[next] + z, inner[next] }));
                faces.Add((NullTextureName, new[] { inner[next], inner[next] + z, outer[next] + z, outer[next] }));
                faces.Add((NullTextureName, new[] { outer[i], outer[i] + z, inner[i] + z, inner[i] }));
                faces.Add((VisibleTextureName, new[] { inner[i] + z, outer[i] + z, outer[next] + z, inner[next] + z }));
                faces.Add((VisibleTextureName, new[] { inner[next], outer[next], outer[i], inner[i] }));
                yield return MakeSolid(faces, colour);
            }
        }
    }
}