using System;
using Sledge.Formats.FileSystem;

namespace Sledge.Formats.GameData
{
    [Obsolete("Use FgdFormat instead")]
    public class FgdFormatter : FgdFormat
    {
        public FgdFormatter()
        {
        }

        public FgdFormatter(IFileResolver resolver) : base(resolver)
        {
        }
    }
}
