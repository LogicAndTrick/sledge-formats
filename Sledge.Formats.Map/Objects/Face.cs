using System.Collections.Generic;
using System.Numerics;

namespace Sledge.Formats.Map.Objects
{
    public class Face : Surface
    {
        /// <summary>
        /// The plane for this face. The plane normal will face outwards in the direction the face is pointing.
        /// </summary>
        public Plane Plane { get; set; }

        /// <summary>
        /// A list of the vertices in the face, in counter-clockwise order when looking at the visible side of the face.
        /// </summary>
        public List<Vector3> Vertices { get; set; }

        public Face()
        {
            Vertices = new List<Vector3>();
        }
    }
}