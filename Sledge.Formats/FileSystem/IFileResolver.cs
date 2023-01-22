using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sledge.Formats.FileSystem
{
    public interface IFileResolver
    {
        bool FileExists(string path);
        Stream OpenFile(string path);
        IEnumerable<string> GetFiles(string path);
        IEnumerable<string> GetFolders(string path);
    }
}
