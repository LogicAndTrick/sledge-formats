using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Sledge.Formats.FileSystem;
using Sledge.Formats.GameData.Objects;
using Sledge.Formats.Tokens;
using Sledge.Formats.Tokens.Readers;
using static Sledge.Formats.Tokens.TokenParsing;

namespace Sledge.Formats.GameData
{
    public class DefFormat
    {
        private static readonly char[] ValidSymbols = {
            Symbols.OpenParen,
            Symbols.CloseParen,
            Symbols.OpenBrace,
            Symbols.CloseBrace,
            Symbols.Semicolon,
            Symbols.Star,
            Symbols.Slash,
            Symbols.Comma,
            Symbols.Equal,
            Symbols.Minus,
            Symbols.Question,
            Symbols.Dot,
        };

        private static readonly Tokeniser Tokeniser = new Tokeniser(
            new SingleLineCommentTokenReader(),
            new StringTokenReader(),
            new UnsignedIntegerTokenReader(),
            new SymbolTokenReader(ValidSymbols),
            new NameTokenReader()
        )
        {
            EmitWhitespace = true,
            EmitComments = true
        };

        public static GameDefinition ReadFile(string path)
        {
            var format = new DefFormat();
            using (var f = File.OpenRead(path))
            {
                using (var sr = new StreamReader(f))
                {
                    return format.Read(sr);
                }
            }
        }

        public GameDefinition Read(string text)
        {
            using (var reader = new StringReader(text))
            {
                return Read(reader);
            }
        }

        public GameDefinition Read(TextReader reader)
        {
            var def = new GameDefinition();

            var tokens = Tokeniser.Tokenise(reader);
            using (var it = tokens.GetEnumerator())
            {
                it.MoveNext();
                while (it.Current != null && !it.Current.Is(TokenType.End))
                {
                    if (it.Current.Is(TokenType.Symbol, "/"))
                    {
                        var ent = ParseEntity(it);
                        def.Classes.Add(ent);
                    }
                    else
                    {
                        it.MoveNext();
                    }
                }
            }

            return def;
        }

        private GameDataClass ParseEntity(IEnumerator<Token> it)
        {
            Expect(it, TokenType.Symbol, "/");
            Expect(it, TokenType.Symbol, "*");

            // skip QUAKED string
            SkipWhile(it, x => !x.Is(TokenType.Whitespace));
            Expect(it, TokenType.Whitespace);

            var name = Expect(it, TokenType.Name).Value;
            SkipWhitespace(it);
            var cls = new GameDataClass(name, "", ClassType.BaseClass);

            if (it.Current?.Is(TokenType.Symbol, "(") == true)
            {
                cls.ClassType = ClassType.PointClass;
                Expect(it, TokenType.Symbol, "(");
                var rd = ParseDecimal(it);
                SkipWhitespace(it);
                var gd = ParseDecimal(it);
                SkipWhitespace(it);
                var bd = ParseDecimal(it);
                SkipWhitespace(it);
                var r = Convert.ToInt32(rd < 1 ? rd * 255 : rd).ToString(CultureInfo.InvariantCulture);
                var g = Convert.ToInt32(gd < 1 ? gd * 255 : gd).ToString(CultureInfo.InvariantCulture);
                var b = Convert.ToInt32(bd < 1 ? bd * 255 : bd).ToString(CultureInfo.InvariantCulture);
                Expect(it, TokenType.Symbol, ")");
                SkipWhitespace(it);
                cls.Behaviours.Add(new Behaviour("color", r, g, b));

                if (it.Current?.Is(TokenType.Symbol, "(") == true)
                {
                    Expect(it, TokenType.Symbol, "(");
                    SkipWhitespace(it);
                    var x1 = Convert.ToInt32(ParseDecimal(it)).ToString(CultureInfo.InvariantCulture);
                    SkipWhitespace(it);
                    var y1 = Convert.ToInt32(ParseDecimal(it)).ToString(CultureInfo.InvariantCulture);
                    SkipWhitespace(it);
                    var z1 = Convert.ToInt32(ParseDecimal(it)).ToString(CultureInfo.InvariantCulture);
                    SkipWhitespace(it);
                    Expect(it, TokenType.Symbol, ")");
                    SkipWhitespace(it);
                    Expect(it, TokenType.Symbol, "(");
                    SkipWhitespace(it);
                    var x2 = Convert.ToInt32(ParseDecimal(it)).ToString(CultureInfo.InvariantCulture);
                    SkipWhitespace(it);
                    var y2 = Convert.ToInt32(ParseDecimal(it)).ToString(CultureInfo.InvariantCulture);
                    SkipWhitespace(it);
                    var z2 = Convert.ToInt32(ParseDecimal(it)).ToString(CultureInfo.InvariantCulture);
                    SkipWhitespace(it);
                    Expect(it, TokenType.Symbol, ")");
                    SkipWhitespace(it);

                    cls.Behaviours.Add(new Behaviour("size", x1, y1, z1, x2, y2, z2));
                }
                else if (it.Current?.Is(TokenType.Symbol, "?") == true)
                {
                    cls.ClassType = ClassType.SolidClass;
                    Expect(it, TokenType.Symbol, "?");
                    SkipWhitespace(it);
                }

                var flags = new Property("spawnflags", VariableType.Flags, "");
                var num = 0;
                while (it.Current != null && !it.Current.Is(TokenType.NewLine))
                {
                    var flagToken = ExpectAny(it, TokenType.Name, TokenType.Symbol);
                    if (flagToken.Is(TokenType.Name))
                    {
                        flags.Options.Add(new Option
                        {
                            Key = (1u << num).ToString(CultureInfo.InvariantCulture),
                            Description = flagToken.Value
                        });
                    }
                    else if (flagToken.Value != "-")
                    {
                        throw new InvalidOperationException($"Expected -, got {flagToken.Value} instead.");
                    }

                    num++;
                    SkipWhitespace(it);
                }

                if (flags.Options.Count > 0)
                {
                    cls.Properties.Add(flags);
                }
            }

            Expect(it, TokenType.NewLine);
            SkipWhitespace(it);

            if (it.Current?.Is(TokenType.Symbol, "{") == true)
            {
                // parse properties (TB specific?)
                throw new NotImplementedException();
            }

            var desc = "";
            while (it.Current?.Is(TokenType.End) != true)
            {
                if (it.Current?.Is(TokenType.Symbol, "*") == true)
                {
                    it.MoveNext();
                    if (it.Current?.Is(TokenType.Symbol, "/") == true)
                    {
                        it.MoveNext();
                        break;
                    }
                    else
                    {
                        desc += "*";
                    }
                }
                desc += it.Current?.Value;
                it.MoveNext();
            }

            cls.Description = desc.Trim();

            return cls;
        }
    }
}
