using System;
using System.IO;

namespace Sledge.Formats.Model.Goldsource
{
    /// <summary>
    /// A single file in the output of a writing a model file.
    /// </summary>
    public class MdlWriteOutputFile : IDisposable
    {
        /// <summary>
        /// The type of file
        /// </summary>
        public MdlWriteOutputType Type { get; set; }

        /// <summary>
        /// The suffix to add to the file name when saving.
        /// For base files, this will be blank.
        /// For texture files, this will be the letter "t".
        /// For animation files, this will be <see cref="FileNumber"/> formatted to 2 digits.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// The file number.
        /// For base files, this will be 0.
        /// For texture files, this will be -1.
        /// For animation files, this will increment starting from 1.
        /// </summary>
        public int FileNumber { get; set; }

        /// <summary>
        /// A stream containing the file data.
        /// </summary>
        public Stream Stream { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}