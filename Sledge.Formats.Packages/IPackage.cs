using System;
using System.Collections.Generic;
using System.IO;

namespace Sledge.Formats.Packages
{
    /// <summary>
    /// A package is a file on disk that contains a virtual file system.
    /// The package manages its own internal file handles and must be disposed by the consumer.
    /// </summary>
    public interface IPackage : IDisposable
    {
        /// <summary>
        /// The list of entries in this package. Each entry corresponds to a single file.
        /// </summary>
        IEnumerable<PackageEntry> Entries { get; }

        /// <summary>
        /// Open an entry in this package. The resulting stream is not guaranteed to have a greater lifetime than the package. The caller must dispose the stream.
        /// </summary>
        /// <param name="entry">The entry to open</param>
        /// <returns>A stream representing the file.</returns>
        Stream Open(PackageEntry entry);
    }
}
