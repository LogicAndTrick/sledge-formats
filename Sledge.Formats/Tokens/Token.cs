using System;

namespace Sledge.Formats.Tokens
{
    public class Token
    {
        public TokenType Type { get; }
        public string CustomType { get; }
        public string Value { get; }
        public int Line { get; set; }
        public int Column { get; set; }

        public char Symbol
        {
            get
            {
                if (Type != TokenType.Symbol || Value == null || Value.Length != 1) throw new ArgumentException($"Not a symbol: {Type}({Value})");
                return Value[0];
            }
        }

        public Token(TokenType type, string value = null)
        {
            Type = type;
            Value = value;
        }

        public Token(string customType, string value = null)
        {
            Type = TokenType.Custom;
            CustomType = customType;
            Value = value;
        }
    }
}
