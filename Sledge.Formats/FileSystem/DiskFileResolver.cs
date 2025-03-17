using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sledge.Formats.FileSystem
{
    public class DiskFileResolver : IFileResolver
    {
        private readonly string _basePath;
        public DiskFileResolver(string basePath)
        {
            _basePath = basePath.Replace('\\', '/');
        }

        private string MakePath(string path)
        {
            path = path.TrimStart('/');
            if (path == string.Empty) return _basePath;

            path = Path.GetFullPath(Path.Combine(_basePath, path)).Replace('\\', '/');
            if (path != _basePath && !path.StartsWith(_basePath)) throw new UnauthorizedAccessException($"Cannot access '{path}' from base path '{_basePath}'.");
            return path;
        }

        private string NormalisePath(string path)
        {
            path = Path.GetFullPath(path).Replace('\\', '/');
            if (path == _basePath) return "/";
            if (!path.StartsWith(_basePath)) throw new UnauthorizedAccessException($"Cannot access '{path}' from base path '{_basePath}'.");
            return path.Substring(_basePath.Length).TrimStart('/');
        }

        public bool FolderExists(string path) => Directory.Exists(MakePath(path));
        public bool FileExists(string path) => File.Exists(MakePath(path));

        public long FileSize(string path)
        {
            var fi = new FileInfo(MakePath(path));
            return fi.Length;
        }

        public Stream OpenFile(string path)
        {
            try
            {
                return File.Open(MakePath(path), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new FileNotFoundException(ex.Message, ex);
            }
        }

        public IEnumerable<string> GetFiles(string path) => Directory.GetFiles(MakePath(path)).Select(NormalisePath);
        public IEnumerable<string> GetFolders(string path) => Directory.GetDirectories(MakePath(path)).Select(NormalisePath);
    }
}