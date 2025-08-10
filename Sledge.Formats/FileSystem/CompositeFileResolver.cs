using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sledge.Formats.FileSystem
{
    /// <summary>
    /// A file resolver made of multiple filesystem abstractions
    /// </summary>
    public class CompositeFileResolver : IFileResolver
    {
        private readonly List<IFileResolver> _resolvers;

        /// <summary>
        /// Create a composite file resolver. Resolvers earlier in the list are given precedence.
        /// </summary>
        public CompositeFileResolver(IEnumerable<IFileResolver> resolvers)
        {
            _resolvers = resolvers.ToList();
        }

        /// <summary>
        /// Create a composite file resolver. Resolvers earlier in the list are given precedence.
        /// </summary>
        public CompositeFileResolver(params IFileResolver[] resolvers)
        {
            _resolvers = resolvers.ToList();
        }

        public bool FolderExists(string path)
        {
            return _resolvers.Any(x => x.FolderExists(path));
        }

        public bool FileExists(string path)
        {
            return _resolvers.Any(x => x.FileExists(path));
        }

        public long FileSize(string path)
        {
            var resolver = _resolvers.FirstOrDefault(x => x.FileExists(path)) ?? throw new FileNotFoundException();
            return resolver.FileSize(path);
        }

        public Stream OpenFile(string path)
        {
            var resolver = _resolvers.FirstOrDefault(x => x.FileExists(path)) ?? throw new FileNotFoundException();
            return resolver.OpenFile(path);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            var resolvers = _resolvers.Where(x => x.FolderExists(path)).ToList();
            if (resolvers.Count == 0) throw new DirectoryNotFoundException();
            return resolvers.SelectMany(x => x.GetFiles(path)).Distinct();
        }

        public IEnumerable<string> GetFolders(string path)
        {
            var resolvers = _resolvers.Where(x => x.FolderExists(path)).ToList();
            if (resolvers.Count == 0) throw new DirectoryNotFoundException();
            return resolvers.SelectMany(x => x.GetFolders(path)).Distinct();
        }
    }
}