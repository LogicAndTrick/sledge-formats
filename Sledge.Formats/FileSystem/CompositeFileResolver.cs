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

        public bool FileExists(string path)
        {
            return _resolvers.Any(x => x.FileExists(path));
        }

        public Stream OpenFile(string path)
        {
            var resolver = _resolvers.FirstOrDefault(x => x.FileExists(path)) ?? throw new FileNotFoundException();
            return resolver.OpenFile(path);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            return _resolvers.SelectMany(x => x.GetFiles(path)).Distinct();
        }

        public IEnumerable<string> GetFolders(string path)
        {
            return _resolvers.SelectMany(x => x.GetFolders(path)).Distinct();
        }
    }
}