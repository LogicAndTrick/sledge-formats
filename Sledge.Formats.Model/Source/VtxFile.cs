using Sledge.Formats.FileSystem;
using System.IO;
using System.Runtime.InteropServices;

namespace Sledge.Formats.Model.Source
{
	public class VtxFile
	{
		public VtxHeader Header;
		public BodyPart[] BodyParts;

		public VtxFile(Stream stream)
		{
			var buffer = new byte[stream.Length];
			stream.Read(buffer, 0, buffer.Length);

			var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			Header = Marshal.PtrToStructure<VtxHeader>(handle.AddrOfPinnedObject());

			BodyParts = new BodyPart[Header.numBodyParts];
			var position = Header.bodyPartOffset;

			for (int i = 0; i < Header.numBodyParts; i++)
			{
				BodyParts[i] = new BodyPart();
				var offset = i * Marshal.SizeOf<BodyPartHeader>();
				BodyParts[i].BodyPartHeader = Marshal.PtrToStructure<BodyPartHeader>(handle.AddrOfPinnedObject() + position + offset);
				BodyParts[i].ReadObjects(handle, position + offset);
			}

			handle.Free();
		}

		public static VtxFile FromFile(string path)
		{
			var dir = Path.GetDirectoryName(path);
			var fname = Path.GetFileName(path);

			var resolver = new DiskFileResolver(dir);
			return FromFile(resolver, fname);
		}
		public static VtxFile FromFile(IFileResolver resolver, string path)
		{
			var basedir = (Path.GetDirectoryName(path) ?? "").Replace('\\', '/');
			if (basedir.Length > 0 && !basedir.EndsWith("/")) basedir += "/";
			var basepath = basedir + Path.GetFileNameWithoutExtension(path);
			var ext = Path.GetExtension(path);

			try
			{
				using (var stream = resolver.OpenFile(path))
				{
					return new VtxFile(stream);
				}
			}
			finally
			{
			}
		}
	}
}
