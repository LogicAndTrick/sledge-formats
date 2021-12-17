using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sledge.Formats.Tokens
{
    public class Tokeniser
    {
        private readonly HashSet<int> _symbolSet;
        public List<ITokenReader> CustomReaders { get; }
        public bool AllowNewlinesInStrings { get; set; } = false;

        public Tokeniser(IEnumerable<char> symbols)
        {
            _symbolSet = new HashSet<int>(symbols.Select(x => (int) x));
            CustomReaders = new List<ITokenReader>();
        }

        public IEnumerable<Token> Tokenise(string text)
        {
            using (var reader = new StringReader(text))
            {
                foreach (var t in Tokenise(reader)) yield return t;
            }
        }

        public IEnumerable<Token> Tokenise(TextReader input)
        {
            var reader = new CountingTextReader(input);
            var custom = CustomReaders.Any();
            int b;
            var leaders = new List<Token>();
            var currentWhitespace = "";
            while ((b = reader.Read()) >= 0)
            {
                if (b == '\r' || b == 0) continue;

                // Whitespace
                if (b == ' ' || b == '\t' || b == '\n')
                {
                    currentWhitespace += (char)b;
                    continue;
                }

                // Comment
                if (b == '/' && reader.Peek() == '/')
                {
                    if (currentWhitespace.Length > 0)
                    {
                        leaders.Add(new Token(TokenType.Whitespace, currentWhitespace));
                        currentWhitespace = "";
                    }

                    var comment = TokenComment(reader);
                    if (comment.Type == TokenType.Invalid)
                    {
                        comment.Line = reader.Line;
                        comment.Column = reader.Column;
                        yield return comment;
                        yield break;
                    }

                    leaders.Add(comment);
                    continue;
                }

                if (currentWhitespace.Length > 0)
                {
                    leaders.Add(new Token(TokenType.Whitespace, currentWhitespace));
                    currentWhitespace = "";
                }

                Token t;
                if (custom && ReadCustom(b, reader, out var ct)) t = ct;
                else if (b == '"') t = TokenString(reader);
                else if (b >= '0' & b <= '9') t = TokenNumber(b, reader);
                else if (_symbolSet.Contains(b)) t = new Token(TokenType.Symbol, ((char) b).ToString());
                else if (b >= 'a' && b <= 'z' || (b >= 'A' && b <= 'Z') || b == '_') t = TokenName(b, reader);
                else t = new Token(TokenType.Invalid, $"Unexpected token: {(char) b}");

                t.Line = reader.Line;
                t.Column = reader.Column;
                t.Leaders.AddRange(leaders);
                leaders.Clear();

                yield return t;

                if (t.Type == TokenType.Invalid)
                {
                    yield break;
                }
            }

            yield return new Token(TokenType.End);
        }

        private bool ReadCustom(int start, TextReader input, out Token token)
        {
            foreach (var reader in CustomReaders)
            {
                token = reader.Read((char) start, input);
                if (token != null) return true;
            }

            token = null;
            return false;
        }

        private Token TokenComment(TextReader input)
        {
            // Need to check the next character
            var b = input.Read();
            if (b == '/')
            {
                // It's a comment, read everything until we hit a newline
                var text = "";
                while ((b = input.Read()) >= 0)
                {
                    if (b == '\r') continue;
                    if (b == '\n') break;
                    text += (char)b;
                }
                return new Token(TokenType.Comment, text);
            }

            // It's not a comment, so it's invalid
            return new Token(TokenType.Invalid, $"Unexpected token: {(char)b}");
        }

        private Token TokenString(TextReader input)
        {
            var sb = new StringBuilder();
            int b;
            while ((b = input.Read()) >= 0)
            {
                switch (b)
                {
                    // ignore carriage returns
                    case '\r':
                        continue;
                    // Newline in string (when they're not allowed)
                    case '\n' when !AllowNewlinesInStrings:
                        // Syntax error, unterminated string
                        return new Token(TokenType.String, sb.ToString())
                        {
                            Warnings =
                            {
                                "String cannot contain a newline"
                            }
                        };
                    // End of string
                    case '"':
                        return new Token(TokenType.String, sb.ToString());
                    // Escaped character
                    case '\\':
                    {
                        // Read the next character
                        b = input.Read();
                        // EOF reached
                        if (b < 0) return new Token(TokenType.Invalid, "Unexpected end of file while reading string value");
                        // Some common escaped characters
                        else if (b == 'n') sb.Append('\n'); // newline
                        else if (b == 'r') sb.Append('\r'); // return
                        else if (b == 't') sb.Append('\t'); // tab
                        // Otherwise, just use whatever it is
                        sb.Append((char) b);
                        break;
                    }
                    // Any other character
                    default:
                        sb.Append((char) b);
                        break;
                }
            }

            return new Token(TokenType.Invalid, "Unexpected end of file while reading string value");
        }

        private static Token TokenNumber(int first, TextReader input)
        {
            var value = ((char) first).ToString();
            int b;
            while ((b = input.Peek()) >= 0)
            {
                if (b >= '0' && b <= '9')
                {
                    value += (char) b;
                    input.Read(); // advance the stream
                }
                else
                {
                    break;
                }
            }

            return new Token(TokenType.Number, value);
        }

        private static Token TokenName(int first, TextReader input)
        {
            var name = ((char) first).ToString();
            int b;
            while ((b = input.Peek()) >= 0)
            {
                if ((b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9') || b == '_' || b == '-' || b == '.')
                {
                    name += (char) b;
                    input.Read(); // advance the stream
                }
                else
                {
                    break;
                }
            }

            return new Token(TokenType.Name, name);
        }

        private class CountingTextReader : TextReader
        {
            private readonly TextReader _reader;

            public int Line { get; set; } = 1;
            public int Column { get; set; } = 0;

            public CountingTextReader(TextReader reader)
            {
                _reader = reader;
            }

            public override int Read()
            {
                var num = _reader.Read();
                Column++;
                if (num == '\n') (Line, Column) = (Line + 1, 0);
                return num;
            }

            public override void Close() => _reader.Close();
            public override int Peek() => _reader.Peek();
            protected override void Dispose(bool disposing) => _reader.Dispose();
            public override object InitializeLifetimeService() => _reader.InitializeLifetimeService();

            // Technically these should count also, but meh
            public override int Read(char[] buffer, int index, int count) => _reader.Read(buffer, index, count);
            public override Task<int> ReadAsync(char[] buffer, int index, int count) => _reader.ReadAsync(buffer, index, count);
            public override int ReadBlock(char[] buffer, int index, int count) => _reader.ReadBlock(buffer, index, count);
            public override Task<int> ReadBlockAsync(char[] buffer, int index, int count) => _reader.ReadBlockAsync(buffer, index, count);
            public override string ReadLine() => _reader.ReadLine();
            public override Task<string> ReadLineAsync() => _reader.ReadLineAsync();
            public override string ReadToEnd() => _reader.ReadToEnd();
            public override Task<string> ReadToEndAsync() => _reader.ReadToEndAsync();
        }
    }
}