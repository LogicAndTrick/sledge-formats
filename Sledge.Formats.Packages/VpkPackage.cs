using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sledge.Formats.Packages
{
    // https://developer.valvesoftware.com/wiki/VPK_File_Format
    public class VpkPackage : IPackage
    {
        private const uint Signature = 0x55aa1234;
        private const string DirString = "_dir";
        private const ushort DirectoryIndex = 0x7fff;
        private const ushort EntryTerminator = 0xffff;

        private readonly Stream _stream;
        private readonly uint _directoryDataOffset;
        private readonly List<PackageEntry> _entries;
        private readonly Dictionary<int, Stream> _chunks;

        public IEnumerable<PackageEntry> Entries => _entries;

        public VpkPackage(string directoryFile)
        {
            _entries = new List<PackageEntry>();
            _stream = OpenFile(directoryFile);

            var folder = Path.GetDirectoryName(directoryFile) ?? "";
            var nameWithoutExt = Path.GetFileNameWithoutExtension(directoryFile) ?? "";
            var ext = Path.GetExtension(directoryFile);

            if (!nameWithoutExt.EndsWith(DirString)) throw new Exception("This is not a valid VPK directory file.");
            
            var baseName = nameWithoutExt.Substring(0, nameWithoutExt.Length - DirString.Length);

            // Scan and find all chunk files that match this vpk directory
            _chunks = new Dictionary<int, Stream>();
            var matchingFiles = Directory.GetFiles(folder, baseName + "_???" + ext);
            foreach (var mf in matchingFiles)
            {
                var fn = Path.GetFileNameWithoutExtension(mf);
                var index = fn.Substring(fn.Length - 3);
                if (UInt16.TryParse(index, out var num))
                {
                    _chunks[num] = OpenFile(mf);
                }
            }

            _chunks[DirectoryIndex] = _stream;

            // Read the data from the vpk
            using (var br = new BinaryReader(_stream, Encoding.UTF8, true))
            {
                var sig = br.ReadUInt32();
                if (sig != Signature) throw new Exception($"Unknown package signature: Expected 0x{Signature:x8}, got 0x{sig:x8}.");

                var version = br.ReadUInt32();
                var treeLength = br.ReadUInt32();
                _directoryDataOffset = treeLength;
                switch (version)
                {
                    case 1:
                        _directoryDataOffset += 12;
                        break;
                    case 2:
                        _directoryDataOffset += 28;
                        var dataLength = br.ReadInt32();
                        var archiveMd5Length = br.ReadUInt32();
                        var fileMd5Length = br.ReadInt32();
                        var signatureLength = br.ReadInt32();
                        break;
                    default:
                        throw new Exception($"Unknown version number: Expected 1 or 2, got {version}.");
                }

                // Read all the entries from the vpk
                string extension, path, filename;
                while ((extension = br.ReadNullTerminatedString()).Length > 0)
                {
                    while ((path = br.ReadNullTerminatedString()).Length > 0)
                    {
                        // Single space = root directory
                        path = path == " " ? "" : path + '/';
                        while ((filename = br.ReadNullTerminatedString()).Length > 0)
                        {
                            // get me some file information
                            var entry = ReadEntry(br, path + filename + "." + extension);
                            _entries.Add(entry);
                        }
                    }
                }
            }
        }

        private PackageEntry ReadEntry(BinaryReader br, string path)
        {
            var crc = br.ReadUInt32();
            var preloadBytes = br.ReadUInt16();
            var archiveIndex = br.ReadUInt16(); // 0x7fff = directory archive
            var entryOffset = br.ReadUInt32(); // If archive directory, relative to END of directory structure
            var entryLength = br.ReadUInt32(); // If 0, preload data contains the entire file

            var terminator = br.ReadUInt16();
            if (terminator != EntryTerminator) throw new Exception($"Invalid terminator. Expected {EntryTerminator:x8}, got {terminator:x8}.");

            var preloadOffset = br.BaseStream.Position;
            // Skip past the preload bytes
            br.BaseStream.Seek(preloadBytes, SeekOrigin.Current);

            return new PackageEntry
            {
                Path = path,
                Offset = entryOffset + (archiveIndex == DirectoryIndex ? _directoryDataOffset : 0),
                Size = entryLength,
                Chunk = archiveIndex,
                PreloadOffset = preloadOffset,
                PreloadSize = preloadBytes
            };
        }

        private static Stream OpenFile(string file)
        {
            return Stream.Synchronized(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.RandomAccess));
        }

        public Stream Open(PackageEntry entry)
        {
            var streams = new List<Stream>();

            if (entry.PreloadSize > 0) streams.Add(new SubStream(_stream, entry.PreloadOffset, entry.PreloadSize));
            if (entry.Size > 0) streams.Add(new SubStream(_chunks[entry.Chunk], entry.Offset, entry.Size));

            return new ConcatStream(streams);
        }

        public void Dispose()
        {
            // _stream is included in the _chunks collection
            foreach (var s in _chunks.Values) s.Dispose();
            _chunks.Clear();
        }
    }
}
