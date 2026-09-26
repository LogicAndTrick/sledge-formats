using System;
using System.Collections.Generic;
using System.Linq;

namespace Sledge.Formats.GameData.Objects
{
    public class GameDataDictionaryValue : IEquatable<GameDataDictionaryValue>
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

        public bool Equals(GameDataDictionaryValue other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Type != other.Type) return false;
            switch (Type)
            {
                case GameDataDictionaryValueType.String:
                case GameDataDictionaryValueType.Number:
                case GameDataDictionaryValueType.Boolean:
                    return Equals(Value, other.Value);
                case GameDataDictionaryValueType.Dictionary:
                    return Value is GameDataDictionary d1 &&
                           other.Value is GameDataDictionary d2 &&
                           d1.Count == d2.Count &&
                           d1.Keys.SequenceEqual(d2.Keys) &&
                           d1.Keys.All(k => Equals(d1[k], d2[k]));
                case GameDataDictionaryValueType.Array:
                    return Value is List<GameDataDictionaryValue> a1 &&
                           other.Value is List<GameDataDictionaryValue> a2 &&
                           a1.Count == a2.Count &&
                           a1.SequenceEqual(a2);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GameDataDictionaryValue)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Type * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public static bool operator ==(GameDataDictionaryValue left, GameDataDictionaryValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GameDataDictionaryValue left, GameDataDictionaryValue right)
        {
            return !Equals(left, right);
        }

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