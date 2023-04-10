using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sledge.Formats.Tokens.Readers
{
    /// <summary>
    /// Reads a symbol as two repetitions of single character from a list of valid symbols.
    /// </summary>
    public class DoubleSymbolTokenReader : ITokenReader
    {
        private readonly HashSet<int> _symbolSet;

        public DoubleSymbolTokenReader(IEnumerable<char> symbols)
        {
            _symbolSet = new HashSet<int>(symbols.Select(x => (int)x));
        }

        public Token Read(char start, TextReader reader)
        {
            if (!_symbolSet.Contains(start) || reader.Peek() != start) return null;

            reader.Read();
            return new Token(TokenType.Symbol, $"{start}{start}");
        }
    }
}