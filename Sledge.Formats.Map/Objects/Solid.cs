using System.Collections.Generic;
using System.Linq;
using Sledge.Formats.Geometric;
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
        /// <param name="useHighPrecisionTypes">True to use double-precision floats to calculate, false otherwise.</param>
        public void ComputeVertices(bool useHighPrecisionTypes = false)
        {
            const float planeMatchEpsilon = 0.0075f; // Magic number that seems to match VHE

            if (Faces.Count < 4) return;

            if (useHighPrecisionTypes)
            {
                var poly = new Polyhedrond(Faces.Select(x => new Planed(x.Plane.Normal.ToVector3d(), x.Plane.D)));

                foreach (var face in Faces)
                {
                    face.Vertices.Clear();
                    var pg = poly.Polygons.FirstOrDefault(x => x.Plane.Normal.EquivalentTo(face.Plane.Normal.ToVector3d(), planeMatchEpsilon));
                    if (pg != null)
                    {
                        face.Vertices.AddRange(pg.Vertices.Select(x => x.ToVector3()));
                    }
                }
            }
            else
            {
                var poly = new Polyhedron(Faces.Select(x => x.Plane));

                foreach (var face in Faces)
                {
                    face.Vertices.Clear();
                    var pg = poly.Polygons.FirstOrDefault(x => x.Plane.Normal.EquivalentTo(face.Plane.Normal, planeMatchEpsilon));
                    if (pg != null)
                    {
                        face.Vertices.AddRange(pg.Vertices);
                    }
                }
            }
        }
    }
}