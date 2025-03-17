using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Sledge.Formats.Geometric;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Factories.Solids
{
    public class TetrahedronSolidFactory : SolidFactoryBase
    {
        public override string Name => "Tetrahedron";

        [DisplayName("Top vertex at centroid")]
        [Description("Put the top vertex at the centroid of the base triangle instead of at the center of the bounding box")]
        public bool TopVertexAtCentroid { get; set; }
        
        public override IEnumerable<MapObject> Create(Box box)
        {
            var useCentroid = TopVertexAtCentroid;

            // The lower Z plane will be the triangle, with the lower Y value getting the two corners
            var c1 = new Vector3(box.Start.X, box.Start.Y, box.Start.Z).Round(RoundDecimals);
            var c2 = new Vector3(box.End.X, box.Start.Y, box.Start.Z).Round(RoundDecimals);
            var c3 = new Vector3(box.Center.X, box.End.Y, box.Start.Z).Round(RoundDecimals);
            var centroid = new Vector3((c1.X + c2.X + c3.X) / 3, (c1.Y + c2.Y + c3.Y) / 3, box.End.Z);
            var c4 = (useCentroid ? centroid : new Vector3(box.Center.X, box.Center.Y, box.End.Z)).Round(RoundDecimals);

            var faces = new[] {
                new[] { c3, c2, c1 },
                new[] { c3, c1, c4 },
                new[] { c2, c3, c4 },
                new[] { c1, c2, c4 }
            };

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