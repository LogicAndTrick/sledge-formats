using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Sledge.Formats.Geometric.Precision;

namespace Sledge.Formats.Map.Objects
{
    public class Solid : MapObject
    {
        public List<Face> Faces { get; set; }
        public List<Mesh> Meshes { get; set; }

        public Solid()
        {
            Faces = new List<Face>();
            Meshes = new List<Mesh>();
        }

        /// <summary>
        /// Computes the vertices for the solid based on the planes of the faces.
        /// Will erase all the current face vertices.
        /// </summary>
        /// <param name="keepFirstVertex">Set to true to attempt to retain the vertex order. This has a small performance impact and should not be needed for most cases.</param>
        public void ComputeVertices(bool keepFirstVertex = false)
        {
            const float planeMatchEpsilon = 0.0075f; // Magic number that seems to match VHE

            if (Faces.Count < 4) return;

            var poly = new Polyhedrond(Faces.Select(x => x.Plane));

            foreach (var face in Faces)
            {
                var firstVert = keepFirstVertex && face.Vertices.Count > 0 ? face.Vertices[0] : (Vector3?)null;
                face.Vertices.Clear();

                var pg = poly.Polygons.FirstOrDefault(x => x.Plane.Normal.EquivalentTo(face.Plane.Normal, planeMatchEpsilon));
                if (pg == null) continue;

                face.Vertices.AddRange(pg.Vertices.Select(x => x.ToVector3()));
                if (!firstVert.HasValue) continue;

                var idx = face.Vertices.FindIndex(x => x.EquivalentTo(firstVert.Value, planeMatchEpsilon));
                while (idx > 0)
                {
                    var tmp = face.Vertices[0];
                    face.Vertices.RemoveAt(0);
                    face.Vertices.Add(tmp);
                    idx--;
                }
            }
        }
    }
}