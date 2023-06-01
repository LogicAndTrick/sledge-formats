using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sledge.Formats.GameData.Addons.TrenchBroom.Nodes;
using Sledge.Formats.Tokens;
using Sledge.Formats.Tokens.Readers;
using static Sledge.Formats.Tokens.TokenParsing;

namespace Sledge.Formats.GameData.Addons.TrenchBroom
{
    public class ExpressionParser
    {
        private static readonly char[] ValidSymbols = {
            // quotes
            Symbols.DoubleQuote,
            Symbols.SingleQuote,
            // operators
            Symbols.Minus,
            Symbols.Plus,
            Symbols.Star,
            Symbols.Slash,
            Symbols.Percent,
            '~',
            '^',
            Symbols.Less,
            Symbols.Greater,
            Symbols.Equal,
            // logical operators
            Symbols.Bang,
            Symbols.Pipe,
            Symbols.Ampersand,
            // arrays
            Symbols.OpenBracket,
            Symbols.CloseBracket,
            // maps
            Symbols.OpenBrace,
            Symbols.CloseBrace,
            Symbols.Colon,
            Symbols.Comma,
            // misc
            Symbols.Dot, // decimals & range operator
            Symbols.OpenParen,
            Symbols.CloseParen,
            Symbols.Dollar, // string interpolation
            Symbols.Backslash, // allow when evaluating interpolations
        };

        private static readonly Tokeniser Tokeniser = new Tokeniser(
            new SingleLineCommentTokenReader(),
            new StringTokenReader('"', '\''),
            new DoubleSymbolTokenReader(new []{ '.', '{', '}' }),
            new UnsignedIntegerTokenReader(),
            new SymbolTokenReader(ValidSymbols),
            new NameTokenReader()
        );

        private static readonly Tokeniser InterpolateTokeniser = new Tokeniser(Tokeniser.Readers.ToArray())
        {
            EmitWhitespace = true
        };

        public static Node Parse(string str, ExpressionParseOptions options = null)
        {
            options = options ?? ExpressionParseOptions.Default;

            var tokens = Tokeniser.Tokenise(str);
            using (var it = tokens.GetEnumerator())
            {
                it.MoveNext();
                var exp = ParseExpression(it, options);
                if (!options.AllowTrailingCharacters) Expect(it, TokenType.End);
                return exp;
            }
        }
        
        private static Node ParseExpression(IEnumerator<Token> it, ExpressionParseOptions options)
        {
            return ParseTerm(it, options);
        }

        private static Node ParseTerm(IEnumerator<Token> it, ExpressionParseOptions options)
        {
            // handle switch/map separately as they both start with {
            if (it.Current?.Is(TokenType.Symbol, "{{") == true)
            {
                return ParseSwitch(it, options);
            }

            // A (non-switch) term ALWAYS starts with a simple term
            var chain = new List<(BinaryOperatorType? op, Node node)>
            {
                (null, ParseSimpleTerm(it, options))
            };

            while (it.Current?.Type == TokenType.Symbol)
            {
                var op = GetBinaryOperator(it.Current, it);
                if (op == null) break;
                
                chain.Add((op.Value, ParseSimpleTerm(it, options)));
            }

            if (chain.Count == 1) return chain[0].node;
            return BuildOperatorTree(chain);
        }

        private static Node ParseSwitch(IEnumerator<Token> it, ExpressionParseOptions options)
        {
            var tok = it.Current;
            var cases = new List<Node>();
            Expect(it, TokenType.Symbol, "{{");
            while (it.Current?.Is(TokenType.Symbol, "}}") != true)
            {
                if (cases.Any()) Expect(it, TokenType.Symbol, ",");
                cases.Add(ParseExpression(it, options));
            }
            Expect(it, TokenType.Symbol, "}}");
            return new SwitchNode(tok, cases);
        }

        private static Node ParseSimpleTerm(IEnumerator<Token> it, ExpressionParseOptions options)
        {
            var val = ParseValue(it, options);

            // use a loop to handle a[1][2][3] etc
            while (it.Current?.Is(TokenType.Symbol, "[") == true)
            {
                var tok = it.Current;
                Expect(it, TokenType.Symbol, "[");
                var index = ParseExpressionOrAnyRange(it, options);
                Expect(it, TokenType.Symbol, "]");
                val = new SubscriptNode(tok, val, index);
            }

            return val;
        }

        private static Node ParseExpressionOrRange(IEnumerator<Token> it, ExpressionParseOptions options)
        {
            // expression | range
            // expression [ ".." expression ]
            var tok = it.Current;
            var exp = ParseExpression(it, options);
            if (it.Current?.Is(TokenType.Symbol, "..") == true)
            {
                Expect(it, TokenType.Symbol, "..");
                var exp2 = ParseExpression(it, options);
                return new BinaryExpressionNode(tok, BinaryOperatorType.Range, exp, exp2);
            }
            return exp;
        }

        private static Node ParseExpressionOrAnyRange(IEnumerator<Token> it, ExpressionParseOptions options)
        {
            // expression | range | autorange
            // expression [ ".." [ expression ] ] | ".." expression
            var tok = it.Current;
            if (it.Current?.Is(TokenType.Symbol, "..") == true)
            {
                // autorange: start - exp
                Expect(it, TokenType.Symbol, "..");
                var autoend = ParseExpression(it, options);
                return new BinaryExpressionNode(tok, BinaryOperatorType.Range, new AutoRangeMarkerNode(tok), autoend);
            }

            var exp = ParseExpression(it, options);
            if (it.Current?.Is(TokenType.Symbol, "..") == true)
            {
                // range: exp - ?
                Expect(it, TokenType.Symbol, "..");
                // only way to detect an autorange end is if the next char is `,` or `]`
                if (it.Current.Is(TokenType.Symbol, ",") | it.Current.Is(TokenType.Symbol, "]")) return LiteralNode.Range(tok, exp, new AutoRangeMarkerNode(tok));
                var exp2 = ParseExpression(it, options);
                return new BinaryExpressionNode(tok, BinaryOperatorType.Range, exp, exp2);
            }
            return exp;
        }

        private static Node ParseValue(IEnumerator<Token> it, ExpressionParseOptions options)
        {
            // Handles the following paths for simple term: Name | Literal | UnaryTerm | GroupedTerm
            switch (it.Current?.Type)
            {
                case TokenType.Symbol:
                    // Handles: UnaryTerm | GroupedTerm | Literal>Array | Literal>Map
                    return ParseValueSymbol(it, options);
                case TokenType.Name:
                    // Handles: Name | Literal>Boolean
                    return ParseValueName(it);
                case TokenType.String:
                    // Handles: Literal>String
                    return ParseValueString(it, options);
                case TokenType.Number:
                    // Handles: Literal>Number
                    return ParseValueNumber(it);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Node ParseValueSymbol(IEnumerator<Token> it, ExpressionParseOptions options)
        {
            var token = it.Current;
            it.MoveNext();
            switch (token?.Value)
            {
                case "-":
                    return new UnaryExpressionNode(token, UnaryOperatorType.Minus, ParseSimpleTerm(it, options));
                case "+":
                    return new UnaryExpressionNode(token, UnaryOperatorType.Plus, ParseSimpleTerm(it, options));
                case "!":
                    return new UnaryExpressionNode(token, UnaryOperatorType.LogicalNot, ParseSimpleTerm(it, options));
                case "~":
                    return new UnaryExpressionNode(token, UnaryOperatorType.BitwiseNot, ParseSimpleTerm(it, options));
                case "[":
                    return ParseArray(it, options);
                case "{":
                    return ParseMap(it, options);
                case "(":
                    var exp = ParseTerm(it, options);
                    Expect(it, TokenType.Symbol, ")");
                    return exp;
                default:
                    throw new ArgumentOutOfRangeException(nameof(token), token?.Value);
            }
        }

        private static Node ParseArray(IEnumerator<Token> it, ExpressionParseOptions options)
        {
            var tok = it.Current;
            var list = new List<Node>();
            var first = true;
            while (it.Current?.Is(TokenType.Symbol, "]") == false)
            {
                if (!first) Expect(it, TokenType.Symbol, ",");
                first = false;
                list.Add(ParseExpressionOrRange(it, options));
            }
            Expect(it, TokenType.Symbol, "]");
            return LiteralNode.Array(tok, list);
        }

        private static Node ParseMap(IEnumerator<Token> it, ExpressionParseOptions options)
        {
            var tok = it.Current;
            var map = new List<KeyValuePair<string, Node>>();
            var first = true;
            while (it.Current?.Is(TokenType.Symbol, "}") == false)
            {
                if (!first) Expect(it, TokenType.Symbol, ",");
                first = false;
                var key = ExpectAny(it, TokenType.Name, TokenType.String).Value;
                Expect(it, TokenType.Symbol, ":");
                var value = ParseExpression(it, options);
                map.Add(new KeyValuePair<string, Node>(key, value));
            }
            Expect(it, TokenType.Symbol, "}");
            return LiteralNode.Map(tok, map);
        }

        private static Node ParseValueName(IEnumerator<Token> it)
        {
            var token = it.Current;
            it.MoveNext();
            switch (token?.Value)
            {
                case "true": return LiteralNode.Boolean(token, true);
                case "false": return LiteralNode.Boolean(token, false);
                case "null": return LiteralNode.Null(token);
                default: return new VariableNode(token, token?.Value);
            }
        }

        private static Node ParseValueString(IEnumerator<Token> it, ExpressionParseOptions options)
        {
            var token = it.Current;
            it.MoveNext();
            if (options.AutomaticInterpolation) return ParseInterpolatedString(token, options);
            return LiteralNode.String(token, token?.Value);
        }

        private static Node ParseValueNumber(IEnumerator<Token> it)
        {
            var token = it.Current;
            return LiteralNode.Number(token, ParseDecimal(it));
        }

        private static double ParseDecimal(IEnumerator<Token> it)
        {
            var stringValue = Expect(it, TokenType.Number).Value;
            if (it.Current?.Is(TokenType.Symbol, ".") == true)
            {
                Expect(it, TokenType.Symbol, ".");
                stringValue += "." + Expect(it, TokenType.Number).Value;
            }
            return double.Parse(stringValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
        }

        private static readonly Dictionary<BinaryOperatorType, int> Precedence = new Dictionary<BinaryOperatorType, int>
        {
            { BinaryOperatorType.Multiply, 12 },
            { BinaryOperatorType.Divide, 12 },
            { BinaryOperatorType.Modulus, 12 },

            { BinaryOperatorType.Add, 11 },
            { BinaryOperatorType.Subtract, 11 },

            { BinaryOperatorType.BitwiseLeftShift, 10 },
            { BinaryOperatorType.BitwiseRightShift, 10 },

            { BinaryOperatorType.LessThan, 9 },
            { BinaryOperatorType.LessThanEqual, 9 },
            { BinaryOperatorType.GreaterThan, 9 },
            { BinaryOperatorType.GreaterThanEqual, 9 },

            { BinaryOperatorType.Equal, 8 },
            { BinaryOperatorType.NotEqual, 8 },

            { BinaryOperatorType.BitwiseAnd, 7 },
            { BinaryOperatorType.BitwiseXor, 6 },
            { BinaryOperatorType.BitwiseOr, 5 },
            { BinaryOperatorType.LogicalAnd, 4 },
            { BinaryOperatorType.LogicalOr, 3 },
            { BinaryOperatorType.Range, 2 },
            { BinaryOperatorType.Implies, 1 },
        };

        /// <summary>
        /// Builds an operator tree out of a chain of nodes and binary operators
        /// </summary>
        /// <param name="nodes">The list of nodes in the chain and their operators. The first node must have null for its operator (as it is the beginning of the chain)</param>
        /// <returns>A node representing the chain of binary operators with highest precedence operations pushed down to the lowest possible level</returns>
        /// <remarks>
        /// The nodes collection is a crude way to represent a chain of binary expressions.
        /// As an example, consider the following expression: `1 + x * 7 / 2 > 7`
        /// The nodes chain would be represented as the following:
        /// (null, 1), (+, x), (*, 7), (/, 2), (>, 7)
        /// We then want to build the tree based on precedence, resulting in implicit groups being formed: (1 + ((x * 7) / 2)) > 7
        /// Which gives us a tree looking something like:
        /// returnValue = binaryop(>, l1, r1)
        /// r1 = constant(7)
        /// l1 = binaryop(+, constant(1), r2)
        /// r2 = binaryop(/, l2, constant(2))
        /// l2 = binaryop(*, variable(x), constant(7))
        /// During evaluation, this results in l2 being evaluated as the first binaryop, since to evaluate the returnValue node,
        /// we must evaluate l1, and to evaluate l1 we must evaluate r2, and to evaluate r2 we must first evaluate l2.
        /// </remarks>
        private static Node BuildOperatorTree(List<(BinaryOperatorType? op, Node node)> nodes)
        {
            // classify all the nodes
            var classified = nodes.Select(x => (x.op.HasValue ? Precedence[x.op.Value] : -1, x.op, x.node)).ToList();
            return BuildOperatorTreeRecursive(classified);
        }

        /// <summary>
        /// Recursively build a tree from an ordered list of binary expressions.
        /// the first node MUST have a null operator.
        /// </summary>
        private static Node BuildOperatorTreeRecursive(List<(int precedence, BinaryOperatorType? op, Node node)> nodes)
        {
            if (nodes[0].op.HasValue) throw new InvalidOperationException();

            if (nodes.Count == 1) return nodes.First().node;
            if (nodes.Count == 2) return new BinaryExpressionNode(nodes[0].node.Token, nodes[1].op.GetValueOrDefault(), nodes[0].node, nodes[1].node);

            var lowestPrec = nodes.Where(x => x.op.HasValue).OrderBy(x => x.precedence).First().precedence;
            var index = nodes.FindLastIndex(x => x.precedence == lowestPrec);

            var left = nodes.GetRange(0, index);

            var right = nodes.GetRange(index, nodes.Count - index);
            if (!right[0].op.HasValue) throw new InvalidOperationException();
            var op = right[0].op.Value;
            right[0] = (-1, null, right[0].node);

            return new BinaryExpressionNode(left[0].node.Token, op, BuildOperatorTreeRecursive(left), BuildOperatorTreeRecursive(right));
        }

        private static BinaryOperatorType? GetBinaryOperator(Token token, IEnumerator<Token> it)
        {
            if (token == null) return null;

            string next;
            // only moves next if an operator is found
            switch (token.Value)
            {
                case "+":
                    Next();
                    return BinaryOperatorType.Add;
                case "-":
                    Next();
                    if (NextIs(">")) return BinaryOperatorType.Implies;
                    return BinaryOperatorType.Subtract;
                case "*":
                    Next();
                    return BinaryOperatorType.Multiply;
                case "/":
                    Next();
                    return BinaryOperatorType.Divide;
                case "%":
                    Next();
                    return BinaryOperatorType.Modulus;
                case "|":
                    Next();
                    if (NextIs("|")) return BinaryOperatorType.LogicalOr;
                    return BinaryOperatorType.BitwiseOr;
                case "&":
                    Next();
                    if (NextIs("&")) return BinaryOperatorType.LogicalAnd;
                    return BinaryOperatorType.BitwiseAnd;
                case "^":
                    Next();
                    return BinaryOperatorType.BitwiseXor;
                case "<":
                    Next();
                    if (NextIs("<")) return BinaryOperatorType.BitwiseLeftShift;
                    if (NextIs("=")) return BinaryOperatorType.LessThanEqual;
                    return BinaryOperatorType.LessThan;
                case ">":
                    Next();
                    if (NextIs(">")) return BinaryOperatorType.BitwiseRightShift;
                    if (NextIs("=")) return BinaryOperatorType.GreaterThanEqual;
                    return BinaryOperatorType.GreaterThan;
                case "!":
                    Next();
                    Expect(it, TokenType.Symbol, "=");
                    return BinaryOperatorType.NotEqual;
                case "=":
                    Next();
                    Expect(it, TokenType.Symbol, "=");
                    return BinaryOperatorType.Equal;
            }

            return null;

            void Next()
            {
                it.MoveNext();
                var nt = it.Current;
                if (nt == null || nt.Type != TokenType.Symbol) next = null;
                else next = nt.Value;
            }

            bool NextIs(string str)
            {
                if (next != str) return false;
                it.MoveNext();
                return true;
            }
        }

        // String interpolation
        public static Node ParseInterpolatedString(string str, ExpressionParseOptions options = null)
        {
            var stringToken = new Token(TokenType.String, str);
            return ParseInterpolatedString(stringToken, options ?? ExpressionParseOptions.Default);
        }

        private static Node ParseInterpolatedString(Token stringToken, ExpressionParseOptions options)
        {
            if (stringToken.Type != TokenType.String) throw new InvalidOperationException($"Excepted string token, got {stringToken.Type}");

            var str = stringToken.Value;
            if (!str.Contains("${")) return LiteralNode.String(stringToken, str);

            var strings = new List<Node>();

            var tokens = InterpolateTokeniser.Tokenise(str);
            using (var it = new ConditionalEnumerator<Token>(tokens.GetEnumerator()))
            {
                var currentString = "";
                var currentStringStart = 0;
                it.MoveNext();

                while (it.Current != null && !it.Current.Is(TokenType.End))
                {
                    var tok = it.Current;

                    if (it.Current?.Is(TokenType.Symbol, "$") == true)
                    {
                        Expect(it, TokenType.Symbol, "$");
                        if (it.Current?.Is(TokenType.Symbol, "{") == true)
                        {
                            // We have a string interpolation
                            it.Condition = x => x.Type != TokenType.Whitespace && x.Type != TokenType.NewLine;
                            Expect(it, TokenType.Symbol, "{");
                            var exp = ParseExpression(it, options);
                            it.Condition = _ => true;
                            if (currentString.Length > 0)
                            {
                                var subTok = new Token(TokenType.String, currentString) { Line = stringToken.Line, Column = stringToken.Column + currentStringStart };
                                strings.Add(LiteralNode.String(subTok, currentString));
                                currentString = "";
                            }
                            strings.Add(exp);
                            Expect(it, TokenType.Symbol, "}");
                            currentStringStart = it.Current?.Column ?? 0;
                        }
                        else
                        {
                            // We just have a standalone $
                            currentString += "$";
                        }
                    }
                    else
                    {
                        currentString += tok.Value;
                        it.MoveNext();
                    }
                }

                if (currentString.Length > 0)
                {
                    var subTok = new Token(TokenType.String, currentString) { Line = stringToken.Line, Column = stringToken.Column + currentStringStart };
                    strings.Add(LiteralNode.String(subTok, currentString));
                }
            }

            if (strings.Count == 0) throw new InvalidOperationException("How did this happen?");
            if (strings.Count == 1) return strings[0];
            return new InterpolatedStringNode(stringToken, strings);
        }
    }
}