using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sledge.Formats.Tokens
{
    public interface ITokenReader
    {
        Token Read(char start, TextReader reader);
    }
}
