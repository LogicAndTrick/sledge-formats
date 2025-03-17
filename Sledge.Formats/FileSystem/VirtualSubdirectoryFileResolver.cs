using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sledge.Formats.FileSystem
{
    /// <summary>
    /// A file resolver that can emulate a package to have all its contents in a subfolder instead of the root path.
    /// </summary>
    public class VirtualSubdirectoryFileResolver : IFileResolver
    {
        private readonly string _subdirectoryPath;
        private readonly IFileResolver _fileResolver;

        public VirtualSubdirectoryFileResolver(string subdirectoryPath, IFileResolver fileResolver)
        {
            _subdirectoryPath = subdirectoryPath.Trim('/');
            _fileResolver = fileResolver;
        }

        /// <summary>
        /// Gets the non-virtual child or parent path given virtual path.
        /// Only one will be non-null, if any.
        /// If both are null, the virtual path cannot be resolved.
        /// </summary>
        /// <param name="path">The virtual path from the caller</param>
        /// <returns>A tuple of the unvirtualised parent or child paths</returns>
        private (string parent, string child) MakePaths(string path)
        {
            path = path.TrimStart('/');
            string parent = null, child = null;
            if (path == _subdirectoryPath)
            {
                child = "";
            }
            else if (path.StartsWith(_subdirectoryPath + "/"))
            {
                child = path.Substring(_subdirectoryPath.Length + 1);
            }
            else if (path.Length == 0)
            {
                parent = _subdirectoryPath;
            }
            else if ((_subdirectoryPath + '/').StartsWith(path + '/'))
            {
                parent = _subdirectoryPath.Substring(0, path.Length);
            }
            return (parent, child);
        }

        public bool FolderExists(string path)
        {
            var (parent, child) = MakePaths(path.TrimEnd('/'));
            if (child != null) return _fileResolver.FolderExists(child);
            else return parent != null;
        }

        public bool FileExists(string path)
        {
            var (_, child) = MakePaths(path);
            if (child == null) return false;
            return _fileResolver.FileExists(child);
        }

        public long FileSize(string path)
        {
            var (_, child) = MakePaths(path);
            if (child == null) throw new FileNotFoundException();
            return _fileResolver.FileSize(child);
        }

        public Stream OpenFile(string path)
        {
            var (_, child) = MakePaths(path);
            if (child == null) throw new FileNotFoundException();
            return _fileResolver.OpenFile(child);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            var (parent, child) = MakePaths(path.TrimEnd('/'));
            if (child != null)
            {
                return _fileResolver.GetFiles(child).Select(x => _subdirectoryPath + "/" + x);
            }
            else if (parent != null)
            {
                // we have a parent path, but we don't have any files in there
                return Array.Empty<string>();
            }
            throw new DirectoryNotFoundException();
        }

        public IEnumerable<string> GetFolders(string path)
        {
            var (parent, child) = MakePaths(path);
            if (child != null)
            {
                return _fileResolver.GetFolders(child).Select(x => _subdirectoryPath + "/" + x);
            }
            else if (parent != null)
            {
                // the only folder we know about is our virtual one
                return new[] { parent };
            }
            throw new DirectoryNotFoundException();
        }
    }
}
