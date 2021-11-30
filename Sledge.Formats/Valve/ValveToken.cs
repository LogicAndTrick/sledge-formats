using System;

namespace Sledge.Formats.Valve
{
    internal class ValveToken
    {
        public ValveTokenType Type { get; }
        public string Value { get; }
        public int Line { get; set; }
        public int Column { get; set; }

        public char Symbol
        {
            get
            {
                if (Type != ValveTokenType.Symbol || Value == null || Value.Length != 1) throw new ArgumentException($"Not a symbol: {Type}({Value})");
                return Value[0];
            }
        }

        public ValveToken(ValveTokenType type, string value = null)
        {
            Type = type;
            Value = value;
        }
    }
}