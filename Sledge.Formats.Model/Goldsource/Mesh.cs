namespace Sledge.Formats.Model.Goldsource
{
    public class Mesh
    {
        public int NumTriangles;
        public int TriangleIndex;
        public int SkinRef;
        public int NumNormals;
        public int NormalIndex;

        public MeshVertex[] Vertices;
    }
}