using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sledge.Formats.Valve
{
    internal static class ValveTokeniser
    {
        public static IEnumerable<ValveToken> Tokenise(string text)
        {
            using (var reader = new StringReader(text))
            {
                foreach (var t in Tokenise(reader)) yield return t;
            }
        }

        internal static IEnumerable<ValveToken> Tokenise(TextReader input)
        {
            var line = 1;
            var col = 0;
            int b;
            while ((b = input.Read()) >= 0)
            {
                col++;

                // Whitespace
                if (b == ' ' || b == '\t' || b == '\r' || b == 0)
                {
                    continue;
                }

                // Newline
                if (b == '\n')
                {
                    line++;
                    col = 0;
                    continue;
                }

                // Comment
                if (b == '/')
                {
                    // Need to check the next character
                    if (input.Read() == '/')
                    {
                        // It's a comment, skip everything until we hit a newline
                        var done = false;
                        while ((b = input.Read()) >= 0)
                        {
                            if (b == '\n')
                            {
                                line++;
                                col = 0;
                                done = true;
                                break;
                            }
                        }

                        if (done) continue;
                        break; // EOF
                    }

                    // It's not a comment, so it's invalid
                    yield return new ValveToken(ValveTokenType.Invalid, $"Unexpected token: {(char) b}") {Line = line, Column = col};
                }

                ValveToken t;
                if (b == '"') t = TokenString(input);
                else if (b == '{') t = new ValveToken(ValveTokenType.Open);
                else if (b == '}') t = new ValveToken(ValveTokenType.Close);
                else if (b >= 'a' && b <= 'z' || (b >= 'A' && b <= 'Z') || b == '_') t = TokenName(b, input);
                else t = new ValveToken(ValveTokenType.Invalid, $"Unexpected token: {(char) b}");

                t.Line = line;
                t.Column = col;

                yield return t;

                if (t.Type == ValveTokenType.Invalid)
                {
                    yield break;
                }
            }

            yield return new ValveToken(ValveTokenType.End);
        }

        private static ValveToken TokenString(TextReader input)
        {
            var sb = new StringBuilder();
            int b;
            while ((b = input.Read()) >= 0)
            {
                // Newline in string (not allowed)
                if (b == '\n')
                {
                    return new ValveToken(ValveTokenType.Invalid, "String cannot contain a newline");
                }
                // End of string
                else if (b == '"')
                {
                    return new ValveToken(ValveTokenType.String, sb.ToString());
                }
                // Escaped character
                else if (b == '\\')
                {
                    // Read the next character
                    b = input.Read();
                    // EOF reached
                    if (b < 0) return new ValveToken(ValveTokenType.Invalid, "Unexpected end of file while reading string value");
                    // Some common escaped characters
                    else if (b == 'n') sb.Append('\n'); // newline
                    else if (b == 'r') sb.Append('\r'); // return
                    else if (b == 't') sb.Append('\t'); // tab
                    // Otherwise, just use whatever it is
                    sb.Append((char) b);
                }
                // Any other character
                else
                {
                    sb.Append((char) b);
                }
            }

            return new ValveToken(ValveTokenType.Invalid, "Unexpected end of file while reading string value");
        }

        private static ValveToken TokenName(int first, TextReader input)
        {
            var name = ((char) first).ToString();
            int b;
            while ((b = input.Peek()) >= 0)
            {
                if ((b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9') || b == '_')
                {
                    name += (char) b;
                    input.Read(); // advance the stream
                }
                else
                {
                    break;
                }
            }

            return new ValveToken(ValveTokenType.Name, name);
        }
    }
}