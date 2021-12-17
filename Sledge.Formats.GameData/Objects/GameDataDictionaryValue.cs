namespace Sledge.Formats.GameData.Objects
{
    public class GameDataDictionaryValue
    {
        public GameDataDictionaryValueType Type { get; set; }
        public object Value { get; set; }

        public GameDataDictionaryValue(string value)
        {
            Type = GameDataDictionaryValueType.String;
            Value = value;
        }

        public GameDataDictionaryValue(decimal value)
        {
            Type = GameDataDictionaryValueType.Number;
            Value = value;
        }

        public GameDataDictionaryValue(bool value)
        {
            Type = GameDataDictionaryValueType.Boolean;
            Value = value;
        }

        public GameDataDictionaryValue(GameDataDictionary value)
        {
            Type = GameDataDictionaryValueType.Dictionary;
            Value = value;
        }
    }
}