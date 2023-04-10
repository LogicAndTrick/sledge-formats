namespace Sledge.Formats.GameData.Addons.TrenchBroom
{
    public class ExpressionParseOptions
    {
        public static readonly ExpressionParseOptions Default = new ExpressionParseOptions();

        public bool AllowTrailingCharacters { get; set; } = false;
        public bool AutomaticInterpolation { get; set; } = false;
    }
}