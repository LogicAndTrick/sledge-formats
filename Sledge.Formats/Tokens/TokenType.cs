namespace Sledge.Formats.Tokens
{
    public enum TokenType
    {
        Invalid,
        Symbol,
        Name,
        String,
        Number,
        Whitespace,
        NewLine,
        Comment,
        Custom,
        End
    }
}