using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sledge.Formats.Tokens
{
    public interface ITokenReader
    {
        /// <summary>
        /// Read a token. Returns null if no token is valid at this point.
        /// </summary>
        Token Read(char start, TextReader reader);
    }
}
