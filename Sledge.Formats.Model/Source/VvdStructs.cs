using System.Numerics;
using System.Runtime.InteropServices;

namespace Sledge.Formats.Model.Source
{
	public struct VvdHeader
	{
		const int MAX_NUM_LODS = 8; // max number of LODs supported by the engine
		const int IDSV_SIZE = 4;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = IDSV_SIZE)]
		public char[] id;             // MODEL_VERTEX_FILE_ID
		public int version;            // MODEL_VERTEX_FILE_VERSION
		public int checksum;           // same as studiohdr_t, ensures sync      ( Note: maybe long instead of int in versions other than 4. )
		public int numLODs;            // num of valid lods
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NUM_LODS)]
		public int[] numLODVertexes;   // num verts for desired root lod
		public int numFixups;          // num of vertexFileFixup_t
		public int fixupTableStart;        // offset from base to fixup table
		public int vertexDataStart;        // offset from base to vertex block
		public int tangentDataStart;       // offset from base to tangent block
	}
	// NOTE: This is exactly 48 bytes
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct StudioVertex
	{
		public StudioBoneWeight m_BoneWeights;
		public Vector3 m_vecPosition;
		public Vector3 m_vecNormal;
		public Vector2 m_vecTexCoord;
	};
	public struct VertexFixup
	{
		public int lod;            // used to skip culled root lod
		public int sourceVertexID;     // absolute index from start of vertex/tangent blocks
		public int numVertexes;
	};
	// 16 bytes
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct StudioBoneWeight
	{
		const int MAX_NUM_BONES_PER_VERT = 3; // max number of bones per vertex supported by the engine

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NUM_BONES_PER_VERT)]
		public float[] weight;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NUM_BONES_PER_VERT)]
		public char[] bone;
		public byte numbones;

	};
}
