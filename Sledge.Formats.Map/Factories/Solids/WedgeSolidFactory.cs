using System.Collections.Generic;
using System.Numerics;
using Sledge.Formats.Geometric;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Factories.Solids
{
    public class WedgeSolidFactory : SolidFactoryBase
    {
        public override string Name => "Wedge";

        public override IEnumerable<MapObject> Create(Box box)
        {
            var solid = new Solid
            {
                Color = ColorUtils.GetRandomBrushColour()
            };

            // The lower Z plane will be base, the x planes will be triangles
            var c1 = new Vector3(box.Start.X, box.Start.Y, box.Start.Z).Round(RoundDecimals);
            var c2 = new Vector3(box.End.X, box.Start.Y, box.Start.Z).Round(RoundDecimals);
            var c3 = new Vector3(box.End.X, box.End.Y, box.Start.Z).Round(RoundDecimals);
            var c4 = new Vector3(box.Start.X, box.End.Y, box.Start.Z).Round(RoundDecimals);
            var c5 = new Vector3(box.Center.X, box.Start.Y, box.End.Z).Round(RoundDecimals);
            var c6 = new Vector3(box.Center.X, box.End.Y, box.End.Z).Round(RoundDecimals);
            var faces = new[]
            {
                new[] { c4, c3, c2, c1 },
                new[] { c5, c1, c2 },
                new[] { c2, c3, c6, c5 },
                new[] { c6, c3, c4 },
                new[] { c4, c1, c5, c6 }
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