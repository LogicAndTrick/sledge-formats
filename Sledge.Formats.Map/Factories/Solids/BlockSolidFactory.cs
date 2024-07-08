using System.Collections.Generic;
using System.Linq;
using Sledge.Formats.Geometric;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Factories.Solids
{
    /// <summary>
    /// Factory that creates a six sided rectangular prism
    /// </summary>
    public class BlockSolidFactory : SolidFactoryBase
    {
        public override string Name => "Block";

        public override IEnumerable<MapObject> Create(Box box)
        {
            var solid = new Solid
            {
                Color = ColorUtils.GetRandomBrushColour()
            };

            foreach (var polygon in box.GetBoxFaces())
            {
                var face = new Face
                {
                    Plane = polygon.Plane,
                    TextureName = VisibleTextureName
                };
                face.Vertices.AddRange(polygon.Vertices.Select(x => x.Round(RoundDecimals)));
                solid.Faces.Add(face);
            }
            yield return solid;
        }
    }
}