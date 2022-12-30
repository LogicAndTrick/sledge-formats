using System.IO;

namespace Sledge.Formats.Tokens.Readers
{
    /// <summary>
    /// Reads a name starting with a-z, A-Z or `_` and followed by a-z, A-Z, `_`, `-`, or `.`.
    /// </summary>
    public class NameTokenReader : ITokenReader
    {
        public Token Read(char start, TextReader reader)
        {
            if ((start < 'a' || start > 'z') && (start < 'A' || start > 'Z') && start != '_') return null;

            var name = start.ToString();
            int b;
            while ((b = reader.Peek()) >= 0)
            {
                if ((b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9') || b == '_' || b == '-' || b == '.')
                {
                    name += (char)b;
                    reader.Read(); // advance the stream
                }
                else
                {
                    break;
                }
            }

            return new Token(TokenType.Name, name);

        }
    }
}