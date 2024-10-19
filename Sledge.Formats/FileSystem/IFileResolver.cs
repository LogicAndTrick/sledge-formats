using System.Collections.Generic;
using System.IO;

namespace Sledge.Formats.FileSystem
{
    /// <summary>
    /// Abstraction for a file system
    /// </summary>
    public interface IFileResolver
    {
        /// <summary>
        /// Check if a folder exists or not
        /// </summary>
        /// <param name="path">An absolute path of a folder to check existance of</param>
        /// <returns>True if the folder exists</returns>
        bool FolderExists(string path);

        /// <summary>
        /// Check if a file exists or not
        /// </summary>
        /// <param name="path">An absolute path of a file to check existance of</param>
        /// <returns>True if the file exists</returns>
        bool FileExists(string path);

        /// <summary>
        /// Get the size, in bytes, of a file
        /// </summary>
        /// <param name="path">An absolute path of a file to get the size of</param>
        /// <returns>The size of the file in bytes</returns>
        /// <exception cref="FileNotFoundException">If the file doesn't exist</exception>
        long FileSize(string path);

        /// <summary>
        /// Open a read-only stream to a file
        /// </summary>
        /// <param name="path">An absolute path of a file to open</param>
        /// <returns>A stream</returns>
        /// <exception cref="FileNotFoundException">If the file doesn't exist</exception>
        Stream OpenFile(string path);

        /// <summary>
        /// Get a list of all the files in a folder
        /// </summary>
        /// <param name="path">An absolute path of a folder to enumerate the files of</param>
        /// <returns>A list of absolute paths for the list of files in the folder. May be empty.</returns>
        /// <exception cref="FileNotFoundException">If the folder doesn't exist</exception>
        IEnumerable<string> GetFiles(string path);

        /// <summary>
        /// Get a list of all the subfolders in a folder
        /// </summary>
        /// <param name="path">An absolute path of a folder to enumerate the subfolders of</param>
        /// <returns>A list of absolute paths for the list of subfolders in the folder. May be empty.</returns>
        /// <exception cref="FileNotFoundException">If the folder doesn't exist</exception>
        IEnumerable<string> GetFolders(string path);
    }
}
