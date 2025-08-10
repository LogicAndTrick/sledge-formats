using System;
using System.Collections.Generic;
using System.Linq;

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

        public GameDataDictionaryValue(IEnumerable<GameDataDictionaryValue> values)
        {
            Type = GameDataDictionaryValueType.Array;
            Value = values.ToList();
        }

        public static implicit operator GameDataDictionaryValue(string val) => new GameDataDictionaryValue(val);
        public static implicit operator GameDataDictionaryValue(decimal val) => new GameDataDictionaryValue(val);
        public static implicit operator GameDataDictionaryValue(bool val) => new GameDataDictionaryValue(val);
        public static implicit operator GameDataDictionaryValue(GameDataDictionary val) => new GameDataDictionaryValue(val);

        public override string ToString()
        {
            switch (Value)
            {
                case null:
                    return "null";
                case string s:
                    return '"' + s.Replace("\"", "\\\"") + '"';
                case IList<GameDataDictionaryValue> list:
                    return "[ " + String.Join(",", list) + "]";
                default:
                    return Value.ToString();
            }
        }
    }
}