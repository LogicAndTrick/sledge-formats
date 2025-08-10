using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Factories.Solids
{
    public abstract class SolidFactoryBase : MapObjectFactoryBase, ISolidFactory
    {
        /// <inheritdoc cref="ISolidFactory.NullTextureName"/>
        [DisplayName("Null texture")]
        [Description("The texture name to apply to hidden faces")]
        public string NullTextureName { get; set; } = "NULL";

        /// <inheritdoc cref="ISolidFactory.VisibleTextureName"/>
        [DisplayName("Visible texture")]
        [Description("The texture name to apply to exposed faces")]
        public string VisibleTextureName { get; set; } = "AAATRIGGER";

        /// <inheritdoc cref="ISolidFactory.RoundDecimals"/>
        [DisplayName("Round decimals")]
        [Description("The number of decimals to round vertex positions to")]
        public int RoundDecimals { get; set; }

        protected static Solid MakeSolid(IEnumerable<(string textureName, Vector3[] points)> faces, Color col)
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
    }
}