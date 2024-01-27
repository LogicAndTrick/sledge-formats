using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sledge.Formats.Packages
{
    // http://quakewiki.org/wiki/.pak
    public class PakPackage : IPackage
    {
        private const string Signature = "PACK";

        private readonly Stream _stream;
        private readonly List<PackageEntry> _entries;

        public IEnumerable<PackageEntry> Entries => _entries;

        /// <summary>
        /// Create a <see cref="PakPackage"/> for the given file path.
        /// </summary>
        public PakPackage(string file) : this(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.RandomAccess))
        {
            //
        }

        /// <summary>
        /// Create a <see cref="PakPackage"/> from the given stream.
        /// The stream will be disposed when this package is disposed.
        /// </summary>
        public PakPackage(Stream stream)
        {
            _entries = new List<PackageEntry>();
            _stream = Stream.Synchronized(stream);

            // Read the data from the pak
            using (var br = new BinaryReader(_stream, Encoding.ASCII, true))
            {
                var sig = br.ReadFixedLengthString(Encoding.ASCII, 4);
                if (sig != Signature) throw new Exception($"Unknown package signature: Expected '{Signature}', got '{sig}'.");

                var treeOffset = br.ReadInt32();
                var treeLength = br.ReadInt32();

                // Read all the entries from the pak
                br.BaseStream.Position = treeOffset;
                var numEntries = treeLength / 64;
                for (var i = 0; i < numEntries; i++)
                {
                    var path = br.ReadFixedLengthString(Encoding.ASCII, 56).ToLowerInvariant();
                    var offset = br.ReadInt32();
                    var size = br.ReadInt32();
                    _entries.Add(new PackageEntry
                    {
                        Path = path,
                        Offset = offset,
                        Size = size
                    });
                }
            }
        }

        public Stream Open(PackageEntry entry)
        {
            return new SubStream(_stream, entry.Offset, entry.Size);
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
