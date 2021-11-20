namespace Sledge.Formats.Valve
{
    internal class ValveToken
    {
        public ValveTokenType Type { get; }
        public string Value { get; }
        public int Line { get; set; }
        public int Column { get; set; }

        public ValveToken(ValveTokenType type, string value = null)
        {
            Type = type;
            Value = value;
        }
    }
}