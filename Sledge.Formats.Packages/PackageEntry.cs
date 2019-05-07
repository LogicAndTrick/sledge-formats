using System.Linq;

namespace Sledge.Formats.Packages
{
    public class PackageEntry
    {
        // Shared stuff

        public string Name => Path.Split('/').LastOrDefault();
        public string Path { get; set; }

        public long Offset { get; set; }
        public long Size { get; set; }

        // VPK stuff

        public int Chunk { get; set; }
        public long PreloadOffset { get; set; }
        public long PreloadSize { get; set; }
    }
}