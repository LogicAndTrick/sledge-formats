using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sledge.Formats.FileSystem;

namespace Sledge.Formats.Packages.FileSystem
{
    /// <summary>
    /// A file resolver for a package.
    /// </summary>
    public class PackageFileResolver : IFileResolver, IDisposable
    {
        private readonly bool _leaveOpen;
        private readonly IPackage _package;

        /// <summary>
        /// Create an instance wrapping an <see cref="IPackage"/> instance.
        /// </summary>
        /// <param name="package">The <see cref="IPackage"/> instance</param>
        /// <param name="leaveOpen">False to dispose the package when this instance is disposed, true to leave it undisposed</param>
        public PackageFileResolver(IPackage package, bool leaveOpen = false)
        {
            _package = package;
            _leaveOpen = leaveOpen;
        }

        private string NormalisePath(string path)
        {
            return path.TrimStart('/');
        }

        public bool FolderExists(string path)
        {
            path = NormalisePath(path) + "/";
            if (path == "/") return true;
            return _package.Entries.Any(x => x.Path.StartsWith(path));
        }

        public bool FileExists(string path)
        {
            path = NormalisePath(path);
            return _package.Entries.Any(x => x.Path == path);
        }

        public long FileSize(string path)
        {
            path = NormalisePath(path);
            var entry = _package.Entries.FirstOrDefault(x => x.Path == path) ?? throw new FileNotFoundException();
            return entry.Size;
        }

        public Stream OpenFile(string path)
        {
            path = NormalisePath(path);
            var entry = _package.Entries.FirstOrDefault(x => x.Path == path) ?? throw new FileNotFoundException();
            return _package.Open(entry);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            if (!FolderExists(path)) throw new DirectoryNotFoundException();
            path = NormalisePath(path) + "/";
            if (path == "/") path = "";

            return _package.Entries.Where(x => x.Path != path && x.Path.StartsWith(path))
                .Select(x => x.Path.Substring(path.Length))
                .Where(x => !x.Contains('/'))
                .Select(x => path + x);
        }

        public IEnumerable<string> GetFolders(string path)
        {
            if (!FolderExists(path)) throw new DirectoryNotFoundException();
            path = NormalisePath(path) + "/";
            if (path == "/") path = "";

            return _package.Entries.Where(x => x.Path != path && x.Path.StartsWith(path))
                .Select(x => x.Path.Substring(path.Length))
                .Where(x => x.Contains('/'))
                .Select(x => path + x.Split('/').First())
                .Distinct();
        }

        public void Dispose()
        {
            if (_leaveOpen) return;
            _package?.Dispose();
        }
    }
}
