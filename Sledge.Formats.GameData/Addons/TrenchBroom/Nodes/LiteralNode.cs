using System;
using System.Collections.Generic;
using Sledge.Formats.Tokens;

namespace Sledge.Formats.GameData.Addons.TrenchBroom.Nodes
{
    public class LiteralNode : Node
    {
        private static readonly object UndefinedValue = new object();
        private static readonly object NullValue = new object();

        public DataType Type { get; }
        public object ObjectValue { get; }

        public bool BooleanValue => (bool) ObjectValue;
        public string StringValue => (string) ObjectValue;
        public double NumberValue => (double) ObjectValue;
        public (Node start, Node end) RangeValue => (ValueTuple<Node, Node>) ObjectValue;
        public List<Node> ArrayValue => (List<Node>) ObjectValue;
        public List<KeyValuePair<string, Node>> MapValue => (List<KeyValuePair<string, Node>>) ObjectValue;

        public LiteralNode(Token token, DataType type, object value) : base(token)
        {
            Type = type;
            ObjectValue = value;
            VerifyType();
        }

        public static LiteralNode Undefined(Token token) => new LiteralNode(token, DataType.Undefined, UndefinedValue);
        public static LiteralNode Null(Token token) => new LiteralNode(token, DataType.Null, NullValue);
        public static LiteralNode Boolean(Token token, bool value) => new LiteralNode(token, DataType.Boolean, value);
        public static LiteralNode Number(Token token, double value) => new LiteralNode(token, DataType.Number, value);
        public static LiteralNode String(Token token, string value) => new LiteralNode(token, DataType.String, value);
        public static LiteralNode Range(Token token, Node start, Node end) => new LiteralNode(token, DataType.Range, ValueTuple.Create(start, end));
        public static LiteralNode Array(Token token, List<Node> value) => new LiteralNode(token, DataType.Array, value);
        public static LiteralNode Map(Token token, List<KeyValuePair<string, Node>> value) => new LiteralNode(token, DataType.Map, value);

        private void VerifyType()
        {
            bool valid;
            switch (Type)
            {
                case DataType.Undefined:
                    valid = ReferenceEquals(ObjectValue, UndefinedValue);
                    break;
                case DataType.Null:
                    valid = ReferenceEquals(ObjectValue, NullValue);
                    break;
                case DataType.Boolean:
                    valid = ObjectValue is bool;
                    break;
                case DataType.Number:
                    valid = ObjectValue is double;
                    break;
                case DataType.String:
                    valid = ObjectValue is string;
                    break;
                case DataType.Array:
                    valid = ObjectValue is List<Node>;
                    break;
                case DataType.Map:
                    valid = ObjectValue is List<KeyValuePair<string, Node>>;
                    break;
                case DataType.Range:
                    valid = ObjectValue is ValueTuple<Node, Node>;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!valid) throw new InvalidOperationException($"Constant type mismatch. Expected {Type}, got {ObjectValue.GetType().Name}.");
        }

    }
}