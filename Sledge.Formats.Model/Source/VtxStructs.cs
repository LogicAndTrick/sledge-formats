using System.Runtime.InteropServices;

namespace Sledge.Formats.Model.Source
{
	public class BodyPart
	{
		public BodyPartHeader BodyPartHeader;
		public Model[] Models;
		internal void ReadObjects(GCHandle handle, int parentOffset)
		{
			Models = new Model[BodyPartHeader.numModels];
			var position = BodyPartHeader.modelOffset;
			for (int i = 0; i < BodyPartHeader.numModels; i++)
			{
				Models[i] = new Model();
				var offset = i * Marshal.SizeOf<ModelHeader>();
				Models[i].ModelHeader = Marshal.PtrToStructure<ModelHeader>(handle.AddrOfPinnedObject() + parentOffset + position + offset);
				Models[i].ReadObjects(handle, parentOffset + position + offset);
			}
		}
		public class Model
		{
			public ModelHeader ModelHeader;
			public ModelLOD[] LOD;

			internal void ReadObjects(GCHandle handle, int parentOffset)
			{
				LOD = new ModelLOD[ModelHeader.numLODs];
				var position = ModelHeader.lodOffset;
				for (int i = 0; i < ModelHeader.numLODs; i++)
				{
					LOD[i] = new ModelLOD();
					var offset = i * Marshal.SizeOf<ModelLODHeader>();
					LOD[i].ModelLODHeader = Marshal.PtrToStructure<ModelLODHeader>(handle.AddrOfPinnedObject() + parentOffset + position + offset);
					LOD[i].ReadObjects(handle, parentOffset + position + offset);
				}
			}

			public class ModelLOD
			{
				public ModelLODHeader ModelLODHeader;
				public Mesh[] Meshes;

				internal void ReadObjects(GCHandle handle, int parentOffset)
				{
					Meshes = new Mesh[ModelLODHeader.numMeshes];
					var position = ModelLODHeader.meshOffset;
					for (int i = 0; i < ModelLODHeader.numMeshes; i++)
					{
						Meshes[i] = new Mesh();
						var offset = i * Marshal.SizeOf<MeshHeader>();
						Meshes[i].MeshHeader = Marshal.PtrToStructure<MeshHeader>(handle.AddrOfPinnedObject() + parentOffset + position + offset);
						Meshes[i].ReadObjects(handle, parentOffset + position + offset);
					}
				}

				public class Mesh
				{
					public MeshHeader MeshHeader;
					public StripGroup[] StripGroups;

					internal void ReadObjects(GCHandle handle, int parentOffset)
					{
						StripGroups = new StripGroup[MeshHeader.numStripGroups];
						var position = MeshHeader.stripGroupHeaderOffset;
						for (int i = 0; i < MeshHeader.numStripGroups; i++)
						{
							StripGroups[i] = new StripGroup();
							var offset = i * Marshal.SizeOf<StripGroupHeader>();
							StripGroups[i].StripGroupHeader = Marshal.PtrToStructure<StripGroupHeader>(handle.AddrOfPinnedObject() + parentOffset + position + offset);
							StripGroups[i].ReadObjects(handle, parentOffset + position + offset);
						}

					}

					public class StripGroup
					{
						public StripGroupHeader StripGroupHeader;
						public Strip[] Strips;

						internal void ReadObjects(GCHandle handle, int parentOffset)
						{
							Strips = new Strip[StripGroupHeader.numStrips];
							var position = StripGroupHeader.stripOffset;
							for (int i = 0; i < StripGroupHeader.numStrips; i++)
							{
								Strips[i] = new Strip();
								var offset = i * Marshal.SizeOf<StripHeader>();
								Strips[i].StripHeader = Marshal.PtrToStructure<StripHeader>(handle.AddrOfPinnedObject() + parentOffset + position + offset);
								// Read vertices
								Strips[i].Verts = new Vertex[Strips[i].StripHeader.numVerts];
								var vertPosition = parentOffset + StripGroupHeader.vertOffset + Strips[i].StripHeader.vertOffset;
								for (int v = 0; v < Strips[i].StripHeader.numVerts; v++)
								{
									var vertOffset = v * Marshal.SizeOf<Vertex>();
									Strips[i].Verts[v] = Marshal.PtrToStructure<Vertex>(handle.AddrOfPinnedObject() + vertPosition + vertOffset);
								}
								// Read indices
								Strips[i].Indices = new ushort[Strips[i].StripHeader.numIndices];
								var indexPosition = StripGroupHeader.indexOffset + Strips[i].StripHeader.indexOffset + parentOffset;
								for (int idx = 0; idx < Strips[i].StripHeader.numIndices; idx++)
								{
									var indexOffset = idx * sizeof(ushort);
									Strips[i].Indices[idx] = (ushort)Marshal.ReadInt16(handle.AddrOfPinnedObject() + indexPosition + indexOffset);
								}
							}

						}

						public class Strip
						{
							public StripHeader StripHeader;
							public Vertex[] Verts;
							public ushort[] Indices;
						}
					}
				}
			}
		}
	}
	// this structure is in <mod folder>/src/public/optimize.h
	public struct VtxHeader
	{
		// file version as defined by OPTIMIZED_MODEL_FILE_VERSION (currently 7)
		public int version;

		// hardware params that affect how the model is to be optimized.
		public int vertCacheSize;
		public ushort maxBonesPerStrip;
		public ushort maxBonesPerTri;
		public int maxBonesPerVert;

		// must match checkSum in the .mdl
		public int checkSum;

		public int numLODs; // Also specified in ModelHeader_t's and should match

		// Offset to materialReplacementList Array. one of these for each LOD, 8 in total
		public int materialReplacementListOffset;

		//Defines the size and location of the body part array
		public int numBodyParts;
		public int bodyPartOffset;
	};
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct BodyPartHeader
	{
		//Model array
		public int numModels;
		public int modelOffset;
	};
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ModelHeader
	{
		//LOD mesh array
		public int numLODs;   //This is also specified in FileHeader_t
		public int lodOffset;
	};
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ModelLODHeader
	{
		//Mesh array
		public int numMeshes;
		public int meshOffset;
		public float switchPoint;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MeshHeader
	{
		public int numStripGroups;
		public int stripGroupHeaderOffset;
		public byte flags;
	};
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct StripGroupHeader
	{
		// These are the arrays of all verts and indices for this mesh.  strips index into this.
		public int numVerts;
		public int vertOffset;

		public int numIndices;
		public int indexOffset;

		public int numStrips;
		public int stripOffset;


		public StripGroupFlags flags;

		// The following fields are only present if MDL version is >=49
		// Points to an array of unsigned shorts (16 bits each)
		//public int numTopologyIndices;
		//public int topologyOffset;
	};
	public enum StripGroupFlags : byte
	{
		STRIPGROUP_IS_FLEXED = 0x01,
		STRIPGROUP_IS_HWSKINNED = 0x02,
		STRIPGROUP_IS_DELTA_FLEXED = 0x04,
		STRIPGROUP_SUPPRESS_HW_MORPH = 0x08,    // NOTE: This is a temporary flag used at run time.
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct StripHeader
	{
		public int numIndices;
		public int indexOffset;
		public int numVerts;
		public int vertOffset;
		public short numBones;
		public StripFlags flags;
		public int numBoneStateChanges;
		public int boneStateChangeOffset;

		// MDL Version 49 and up only
		//public int numTopologyIndices;
		//public int topologyOffset;
	};
	public enum StripFlags : byte
	{
		STRIP_IS_TRILIST = 0x01,
		STRIP_IS_TRISTRIP = 0x02,
	};
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Vertex
	{
		// these index into the mesh's vert[origMeshVertID]'s bones
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public byte[] boneWeightIndex;
		public byte numBones;
		public ushort origMeshVertID;

		// for sw skinned verts, these are indices into the global list of bones
		// for hw skinned verts, these are hardware bone indices
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public byte[] boneID;
	};
	public struct MaterialReplacementListHeader
	{
		public int numReplacements;
		public int replacementOffset;
	};
}
