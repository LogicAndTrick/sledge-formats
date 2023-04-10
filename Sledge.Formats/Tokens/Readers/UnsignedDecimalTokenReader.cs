using System.IO;

namespace Sledge.Formats.Tokens.Readers
{
    /// <summary>
    /// Reads an integer or decimal consisting of at least one digit.
    /// Sign prefixes are not included.
    /// (allowed: `.1`, `1.`, `1.1`, `1`)
    /// </summary>
    public class UnsignedDecimalTokenReader : ITokenReader
    {
        public Token Read(char start, TextReader reader)
        {
            if (start != '.' && (start < '0' || start > '9')) return null;
            var decimalFound = start == '.';

            var value = start.ToString();
            int b;
            while ((b = reader.Peek()) >= 0)
            {
                if ((b >= '0' && b <= '9') || (b == '.' && !decimalFound))
                {
                    value += (char)b;
                    reader.Read(); // advance the stream
                    if (b == '.') decimalFound = true;
                }
                else
                {
                    break;
                }
            }

            return new Token(TokenType.Number, value);
        }
    }
}