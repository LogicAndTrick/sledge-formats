using System;
using System.Collections.Generic;
using System.Linq;

namespace Sledge.Formats.Model.Goldsource
{
    /// <summary>
    /// The result of writing a model file.
    /// </summary>
    public class MdlWriteOutput : IDisposable
    {
        /// <summary>
        /// A list of files that make up the file.
        /// </summary>
        public List<MdlWriteOutputFile> Files { get; set; }

        public MdlWriteOutput(IEnumerable<MdlWriteOutputFile> files)
        {
            Files = files.ToList();
        }

        public MdlWriteOutput(params MdlWriteOutputFile[] files) : this(files.AsEnumerable()) {}

        public void Dispose()
        {
            Files.ForEach(x => x.Dispose());
        }
    }
}