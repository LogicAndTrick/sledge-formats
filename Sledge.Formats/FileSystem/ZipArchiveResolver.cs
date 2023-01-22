using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Sledge.Formats.FileSystem
{
    /// <summary>
    /// A file resolver for a zip file.
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
        /// Create an instance for a <see cref="ZipArchiveResolver"/>.
        /// </summary>
        /// <param name="zip">The ZipArchive instance</param>
        /// <param name="leaveOpen">False to dispose the archive when this instance is dispose, true to leave it undisposed</param>
        public ZipArchiveResolver(ZipArchive zip, bool leaveOpen = false)
        {
            _zip = zip;
            _leaveOpen = leaveOpen;
        }

        private string NormalisePath(string path)
        {
            return path.TrimStart('/');
        }

        public bool FileExists(string path)
        {
            path = NormalisePath(path);
            return _zip.GetEntry(path) != null;
        }

        public Stream OpenFile(string path)
        {
            path = NormalisePath(path);
            var e = _zip.GetEntry(path) ?? throw new FileNotFoundException();
            return e.Open();
        }

        public IEnumerable<string> GetFiles(string path)
        {
            path = NormalisePath(path);
            var basePath = path;

            if (basePath != string.Empty)
            {
                var e = _zip.GetEntry(basePath) ?? throw new FileNotFoundException();
                if (e.Length != 0) throw new FileNotFoundException();
            }

            return _zip.Entries.Where(x => x.Name != String.Empty && x.FullName.StartsWith(basePath) && !x.FullName.EndsWith("/"))
                .Select(x => x.FullName.Substring(basePath.Length))
                .Where(x => !x.Contains('/'));
        }

        public IEnumerable<string> GetFolders(string path)
        {
            path = NormalisePath(path);
            var basePath = path;

            if (basePath != string.Empty)
            {
                var e = _zip.GetEntry(basePath) ?? throw new FileNotFoundException();
                if (e.Length != 0) throw new FileNotFoundException();
            }

            return _zip.Entries.Where(x => x.Name == String.Empty && x.FullName.StartsWith(basePath) && x.FullName.EndsWith("/"))
                .Select(x => x.FullName.Substring(basePath.Length, x.FullName.Length - basePath.Length - 1))
                .Where(x => !x.Contains('/'));
        }

        public void Dispose()
        {
            if (_leaveOpen) return;
            _zip?.Dispose();
        }
    }
}