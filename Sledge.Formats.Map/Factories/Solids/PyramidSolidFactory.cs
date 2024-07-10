using System.Collections.Generic;
using System.Numerics;
using Sledge.Formats.Geometric;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Factories.Solids
{
    public class PyramidSolidFactory : SolidFactoryBase
    {
        public override string Name => "Pyramid";

        public override IEnumerable<MapObject> Create(Box box)
        {
            var solid = new Solid
            {
                Color = ColorUtils.GetRandomBrushColour()
            };

            // The lower Z plane will be base
            var c1 = new Vector3(box.Start.X, box.Start.Y, box.Start.Z).Round(RoundDecimals);
            var c2 = new Vector3(box.End.X, box.Start.Y, box.Start.Z).Round(RoundDecimals);
            var c3 = new Vector3(box.End.X, box.End.Y, box.Start.Z).Round(RoundDecimals);
            var c4 = new Vector3(box.Start.X, box.End.Y, box.Start.Z).Round(RoundDecimals);
            var c5 = new Vector3(box.Center.X, box.Center.Y, box.End.Z).Round(RoundDecimals);
            var faces = new[]
            {
                new[] { c4, c3, c2, c1 },
                new[] { c5, c1, c2 },
                new[] { c5, c2, c3 },
                new[] { c5, c3, c4 },
                new[] { c5, c4, c1 }
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