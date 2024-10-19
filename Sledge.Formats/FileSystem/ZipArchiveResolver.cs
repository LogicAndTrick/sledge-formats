using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Sledge.Formats.FileSystem
{
    /// <summary>
    /// A file resolver for a zip file. Zip archives are treated as case-sensitive on all platforms.
    /// </summary>
    public class ZipArchiveResolver : IFileResolver, IDisposable
    {
        private readonly bool _leaveOpen;
        private readonly ZipArchive _zip;

        /// <summary>
        /// Create an instance for a file on disk.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        public ZipArchiveResolver(string filePath)
        {
            _zip = new ZipArchive(File.OpenRead(filePath), ZipArchiveMode.Read, false);
            _leaveOpen = false;
        }

        /// <summary>
        /// Create an instance for a <see cref="ZipArchive"/>.
        /// </summary>
        /// <param name="zip">The ZipArchive instance</param>
        /// <param name="leaveOpen">False to dispose the archive when this instance is disposed, true to leave it undisposed</param>
        public ZipArchiveResolver(ZipArchive zip, bool leaveOpen = false)
        {
            _zip = zip;
            _leaveOpen = leaveOpen;
        }

        private string NormalisePath(string path)
        {
            return path.TrimStart('/');
        }

        public bool FolderExists(string path)
        {
            path = NormalisePath(path) + "/";
            if (path == "/" || _zip.GetEntry(path) != null) return true; // directory entry exists
            return _zip.Entries.Any(x => x.FullName.StartsWith(path));
        }

        public bool FileExists(string path)
        {
            path = NormalisePath(path);
            var e = _zip.GetEntry(path);
            return e != null && !e.FullName.EndsWith("/");
        }

        public long FileSize(string path)
        {
            path = NormalisePath(path);
            var e = _zip.GetEntry(path);
            if (e == null || e.FullName.EndsWith("/")) throw new FileNotFoundException();
            return e.Length;
        }

        public Stream OpenFile(string path)
        {
            path = NormalisePath(path);
            var e = _zip.GetEntry(path) ?? throw new FileNotFoundException();
            if (e.FullName.EndsWith("/")) throw new FileNotFoundException(); // its a directory entry
            return e.Open();
        }

        public IEnumerable<string> GetFiles(string path)
        {
            if (!FolderExists(path)) throw new DirectoryNotFoundException();
            path = NormalisePath(path) + "/";
            if (path == "/") path = "";

            return _zip.Entries.Where(x => x.FullName != path && x.FullName.StartsWith(path))
                .Select(x => x.FullName.Substring(path.Length))
                .Where(x => !x.Contains('/'))
                .Select(x => path + x);
        }

        public IEnumerable<string> GetFolders(string path)
        {
            if (!FolderExists(path)) throw new DirectoryNotFoundException();
            path = NormalisePath(path) + "/";
            if (path == "/") path = "";

            return _zip.Entries.Where(x => x.FullName != path && x.FullName.StartsWith(path))
                .Select(x => x.FullName.Substring(path.Length))
                .Where(x => x.Contains('/'))
                .Select(x => path + x.Split('/').First())
                .Distinct();
        }

        public void Dispose()
        {
            if (_leaveOpen) return;
            _zip?.Dispose();
        }
    }
}