using Sledge.Formats.FileSystem;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Sledge.Formats.Model.Source
{
	public class VvdFile
	{
		public VvdHeader Header { get; set; }
		public StudioVertex[] Vertices { get; set; }
		public Vector4[] TangentData { get; set; }
		public VertexFixup[] Fixups { get; set; }
		public VvdFile(Stream stream)
		{
			var buffer = new byte[stream.Length];
			stream.Read(buffer, 0, buffer.Length);
			var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			var vertexSize = Marshal.SizeOf<StudioVertex>();
			Header = Marshal.PtrToStructure<VvdHeader>(handle.AddrOfPinnedObject());

			var vertexCount = Header.numLODVertexes[0];
			Vertices = new StudioVertex[vertexCount];
			for(int i = 0; i < vertexCount; i++)
			{
				var offset = i * vertexSize;
				Vertices[i] = Marshal.PtrToStructure<StudioVertex>(handle.AddrOfPinnedObject() + offset + Header.vertexDataStart);
			}
			var tangentSize = Marshal.SizeOf<Vector4>();
			var tangentOffset = Header.vertexDataStart + vertexCount * vertexSize;
			TangentData = new Vector4[vertexCount];
			for (int i = 0; i < vertexCount; i++)
			{
				var offset = i * tangentSize;
				TangentData[i] = Marshal.PtrToStructure<Vector4>(handle.AddrOfPinnedObject() + offset + tangentOffset);
			}

			var fixupSize = Marshal.SizeOf<VertexFixup>();
			Fixups = new VertexFixup[Header.numFixups];
			for (int i = 0; i < Header.numFixups; i++)
			{
				var offset = i * fixupSize;
				Fixups[i] = Marshal.PtrToStructure<VertexFixup>(handle.AddrOfPinnedObject() + offset + Header.fixupTableStart);
			}
			handle.Free();
		}

		public static VvdFile FromFile(string path)
		{
			var dir = Path.GetDirectoryName(path);
			var fname = Path.GetFileName(path);

			var resolver = new DiskFileResolver(dir);
			return FromFile(resolver, fname);
		}
		public static VvdFile FromFile(IFileResolver resolver, string path)
		{
			var basedir = (Path.GetDirectoryName(path) ?? "").Replace('\\', '/');
			if (basedir.Length > 0 && !basedir.EndsWith("/")) basedir += "/";
			var basepath = basedir + Path.GetFileNameWithoutExtension(path);
			var ext = Path.GetExtension(path);

			try
			{
				var stream = resolver.OpenFile(path);

				return new VvdFile(stream);
			}
			finally
			{
			}
		}

	}
}
