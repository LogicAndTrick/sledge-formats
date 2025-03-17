using System;
using System.Collections.Generic;
using System.Linq;

namespace Sledge.Formats.Texture.Vtf
{
    public class VtfValidationException : Exception
    {
        public List<string> Errors { get; set; }

        public VtfValidationException(IEnumerable<string> errors) : base("VTF validation failed. See the Errors array for more details.")
        {
            Errors = errors.ToList();
        }
    }
}
