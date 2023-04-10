using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sledge.Formats.GameData.Addons.TrenchBroom
{
    public class Value : IComparable<Value>
    {
        public static readonly Value Null = new Value(DataType.Null, null);
        public static readonly Value Undefined = new Value(DataType.Undefined, null);

        public DataType Type { get; }
        public object ObjectValue { get; }

        public bool BooleanValue => (bool)ObjectValue;
        public string StringValue => (string)ObjectValue;
        public double NumberValue => (double)ObjectValue;
        public int IntValue => (int)NumberValue;
        public (Value start, Value end) RangeValue => ((Value, Value))ObjectValue;
        public List<Value> ArrayValue => (List<Value>)ObjectValue;
        public Dictionary<string, Value> MapValue => (Dictionary<string, Value>)ObjectValue;

        public Value(DataType type, object objectValue)
        {
            #if DEBUG
            if (objectValue != null)
            {
                var ty = objectValue.GetType();
                if (ty != typeof(bool) &&
                    ty != typeof(string) &&
                    ty != typeof(double) &&
                    ty != typeof(ValueTuple<Value, Value>) &&
                    ty != typeof(List<Value>) &&
                    ty != typeof(Dictionary<string, Value>))
                {
                    throw new InvalidOperationException($"Unsupported value type: {ty.Name}");
                }
            }
            else if (type != DataType.Null && type != DataType.Undefined)
            {
                throw new InvalidOperationException($"Unsupported null type: {type}");
            }
            #endif
            Type = type;
            ObjectValue = objectValue;
        }

        public Value() : this(DataType.Null, null) {}
        public Value(bool val) : this(DataType.Boolean, val) {}
        public Value(string val) : this(DataType.String, val) {}
        public Value(int val) : this(DataType.Number, (double) val) {}
        public Value(float val) : this(DataType.Number, (double) val) {}
        public Value(double val) : this(DataType.Number, (double) val) {}
        public Value(decimal val) : this(DataType.Number, (double) val) {}
        public Value(Value start, Value end) : this(DataType.Range, (start, end)) {}
        public Value(IEnumerable<Value> val) : this(DataType.Array, new List<Value>(val)) {}
        public Value(IDictionary<string, Value> val) : this(DataType.Map, new Dictionary<string, Value>(val)) {}
        public Value(IEnumerable<KeyValuePair<string, Value>> pairs)
        {
            Type = DataType.Map;
            var d = new Dictionary<string, Value>();
            foreach (var kv in pairs) d[kv.Key] = kv.Value;
            ObjectValue = d;
        }

        public Value ValueAtIndex(Value index)
        {
            if (Type == DataType.Array && index.Type == DataType.Number)
            {
                var iv = index.IntValue;
                if (iv < 0) iv = ArrayValue.Count + iv;
                if (iv < 0 || iv >= ArrayValue.Count) throw new InvalidOperationException("Index out of range");
                return ArrayValue[iv];
            }
            else if (Type == DataType.Array && index.Type == DataType.Range)
            {
                var ret = new List<Value>();
                var rv = index.RangeValue;
                var start = rv.start?.NumberValue ?? 0;
                var end = rv.end?.NumberValue ?? ArrayValue.Count - 1;
                for (var i = start; i <= end; i++)
                {
                    var iv = (int) i;
                    if (iv < 0) iv = ArrayValue.Count + iv;
                    if (iv < 0 || iv >= ArrayValue.Count) throw new InvalidOperationException("Index out of range");
                    ret.Add(ArrayValue[iv]);
                }
                return new Value(ret);
            }
            else if (Type == DataType.Array && index.Type == DataType.Array)
            {
                var ret = new List<Value>();
                foreach (var v in index.ArrayValue)
                {
                    if (v.Type != DataType.Number) throw new InvalidOperationException($"Incorrect array index type: {v.Type}");
                    var iv = v.IntValue;
                    if (iv < 0) iv = ArrayValue.Count + iv;
                    if (iv < 0 || iv >= ArrayValue.Count) throw new InvalidOperationException("Index out of range");
                    ret.Add(ArrayValue[iv]);
                }

                return new Value(ret);
            }
            else if (Type == DataType.String && index.Type == DataType.Number)
            {
                return new Value(SafeCharAt(StringValue, index.IntValue));
            }
            else if (Type == DataType.String && index.Type == DataType.Array)
            {
                return new Value(string.Join("", index.ArrayValue.Select(i => SafeCharAt(StringValue, ((Value)i).IntValue)))
                );
            }
            else if (Type == DataType.Map && index.Type == DataType.String)
            {
                var map = MapValue;
                var key = index.StringValue;
                if (!map.ContainsKey(key)) return Undefined;
                return MapValue[key];
            }
            else if (Type == DataType.Map && index.Type == DataType.Array)
            {
                var map = MapValue;
                var ret = new Dictionary<string, Value>();
                foreach (var Value in index.ArrayValue)
                {
                    var v = (Value)Value;
                    if (v.Type != DataType.String) throw new InvalidCastException($"Incorrect map index type: {v.Type}");
                    var key = v.StringValue;
                    if (map.ContainsKey(key)) ret[key] = (Value)MapValue[key];
                }

                return new Value(ret);
            }

            throw new InvalidOperationException($"Cannot index a value of type {Type} with type {index.Type}.");
        }

        private static string SafeCharAt(string str, int index)
        {
            if (index < 0) index = str.Length + index;
            if (index < 0 || index >= str.Length) return String.Empty;
            return str[index].ToString();
        }

        #region Conversions

        public override string ToString()
        {
            switch (Type)
            {
                case DataType.Boolean:
                    return AsString();
                case DataType.String:
                    return '"' + AsString() + '"';
                case DataType.Number:
                    return AsString();
                case DataType.Range:
                    return "(" + RangeValue.start + ',' + RangeValue.end + ')';
                case DataType.Array:
                    return '[' + String.Join(", ", ArrayValue) + ']';
                case DataType.Map:
                    return '{' + String.Join(", ", MapValue.Select(kv => $"{kv.Key}: {kv.Value}")) + '}';
                case DataType.Null:
                    return "null";
                case DataType.Undefined:
                    return "undefined";
                default:
                    return $"{Type}[{ObjectValue}]";
            }
        }

        public string AsString() => ConvertTo(DataType.String).StringValue;

        public Value ConvertTo(DataType targetType)
        {
            if (targetType == Type) return new Value(Type, ObjectValue);

            Value val = null;
            switch (targetType)
            {
                case DataType.Boolean:
                    val = AsBooleanValue();
                    break;
                case DataType.String:
                    val = AsStringValue();
                    break;
                case DataType.Number:
                    val = AsNumberValue();
                    break;
                case DataType.Map:
                    if (Type == DataType.Null) return new Value(new Dictionary<string, Value>());
                    break;
                case DataType.Array:
                    if (Type == DataType.Null) return new Value(Array.Empty<Value>());
                    break;
                case DataType.Null:
                case DataType.Range:
                case DataType.Undefined:
                    // nothing
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
            }

            return val ?? throw new InvalidCastException($"Unable to convert from {Type} to {targetType}.");
        }

        private Value AsBooleanValue()
        {
            switch (Type)
            {
                case DataType.String:
                    return new Value(StringValue != "false" && StringValue != "");
                case DataType.Number:
                    return new Value(NumberValue != 0);
                case DataType.Null:
                    return new Value(false);
                default:
                    return null;
            }
        }

        private Value AsStringValue()
        {
            switch (Type)
            {
                case DataType.Boolean:
                    return new Value(BooleanValue ? "true" : "false");
                case DataType.Number:
                    return new Value(NumberValue.ToString("G17", CultureInfo.InvariantCulture));
                case DataType.Null:
                    return new Value("");
                default:
                    return null;
            }
        }

        private Value AsNumberValue()
        {
            switch (Type)
            {
                case DataType.Boolean:
                    return new Value(BooleanValue ? 1 : 0);
                case DataType.String:
                    if (String.IsNullOrEmpty(StringValue)) return new Value(0);
                    return double.TryParse(StringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var dec) ? new Value(dec) : null;
                case DataType.Null:
                    return new Value(0);
                default:
                    return null;
            }
        }

        #endregion

        #region Equality
        
        public int CompareTo(Value other)
        {
            if (Type == DataType.Null) return other.Type == DataType.Null ? 0 : -1;
            if (other.Type == DataType.Null) return Type == DataType.Null ? 0 : 1;

            var a = this;
            var b = other.Type == Type ? other : other.ConvertTo(Type);
            switch (Type)
            {
                case DataType.Boolean:
                    return (a.BooleanValue ? 1 : 0).CompareTo(b.BooleanValue ? 1 : 0);
                case DataType.String:
                    return String.Compare(a.StringValue, b.StringValue, StringComparison.Ordinal);
                case DataType.Number:
                    return a.NumberValue.CompareTo(b.NumberValue);
                case DataType.Array:
                    var arc = a.ArrayValue.Count.CompareTo(b.ArrayValue.Count);
                    if (arc != 0) return arc;
                    for (var i = 0; i < a.ArrayValue.Count; i++)
                    {
                        var aa = (Value) a.ArrayValue[i];
                        var bb = (Value) b.ArrayValue[i];
                        arc = aa.CompareTo(bb);
                        if (arc != 0) return arc;
                    }
                    return 0;
                case DataType.Map:
                    var mac = a.MapValue.Count.CompareTo(b.MapValue.Count);
                    if (mac != 0) return mac;
                    var sorta = a.MapValue.OrderBy(x => x.Key).ToList();
                    var sortb = b.MapValue.OrderBy(x => x.Key).ToList();
                    for (var i = 0; i < sorta.Count; i++)
                    {
                        var aa = sorta[i];
                        var bb = sortb[i];

                        arc = String.Compare(aa.Key, bb.Key, StringComparison.Ordinal);
                        if (arc != 0) return arc;

                        arc = ((Value)aa.Value).CompareTo((Value)bb.Value);
                        if (arc != 0) return arc;
                    }
                    return 0;
                case DataType.Range:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            throw new NotImplementedException();
        }

        public bool Equals(Value other)
        {
            if (Type != other.Type) return false;
            switch (Type)
            {
                case DataType.Null:
                case DataType.Undefined:
                    return true;
                case DataType.Boolean:
                case DataType.String:
                case DataType.Number:
                    return Object.Equals(ObjectValue, other.ObjectValue);
                case DataType.Array:
                    return ArrayValue.SequenceEqual(other.ArrayValue);
                case DataType.Map:
                    return MapValue.OrderBy(x => x.Key).SequenceEqual(other.MapValue.OrderBy(x => x.Key));
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Value)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Type * 397) ^ (ObjectValue != null ? ObjectValue.GetHashCode() : 0);
            }
        }

        #endregion
    }
}
