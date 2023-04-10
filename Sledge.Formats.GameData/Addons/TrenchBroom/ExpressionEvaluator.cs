using System;
using System.Linq;
using System.Text;
using Sledge.Formats.GameData.Addons.TrenchBroom.Nodes;

namespace Sledge.Formats.GameData.Addons.TrenchBroom
{
    public class ExpressionEvaluator
    {
        public Value Evaluate(Node node, EvaluationContext context)
        {
            if (node is AutoRangeMarkerNode armn) return EvaluateAutoRangeMarker(armn, context);
            if (node is BinaryExpressionNode ben) return EvaluateBinary(ben, context);
            if (node is InterpolatedStringNode isn) return EvaluateInterpolatedString(isn, context);
            if (node is LiteralNode ln) return EvaluateLiteral(ln, context);
            if (node is SubscriptNode sn) return EvaluateSubscript(sn, context);
            if (node is SwitchNode sw) return EvaluateSwitch(sw, context);
            if (node is UnaryExpressionNode uen) return EvaluateUnary(uen, context);
            if (node is VariableNode vn) return EvaluateVariable(vn, context);
            throw new ArgumentException($"Unknown node type {node.GetType().Name}", nameof(node));
        }

        private Value EvaluateAutoRangeMarker(AutoRangeMarkerNode node, EvaluationContext context)
        {
            return null;
        }

        private Value EvaluateBinary(BinaryExpressionNode node, EvaluationContext context)
        {
            var l = Evaluate(node.Left, context);

            // short-circuiting
            switch (node.Operator)
            {
                case BinaryOperatorType.LogicalAnd:
                    if (!l.BooleanValue) return new Value(false);
                    break;
                case BinaryOperatorType.LogicalOr:
                    if (l.BooleanValue) return new Value(true);
                    break;
                case BinaryOperatorType.Implies:
                    if (l.Type == DataType.Undefined) return Value.Undefined;
                    if (!l.ConvertTo(DataType.Boolean).BooleanValue) return Value.Undefined;
                    break;
            }

            var r = Evaluate(node.Right, context);

            switch (node.Operator)
            {
                case BinaryOperatorType.Add:
                    if (l.Type == DataType.String && r.Type == DataType.String) return new Value(l.StringValue + r.StringValue);
                    if (l.Type == DataType.Array && r.Type == DataType.Array) return new Value(l.ArrayValue.Concat(r.ArrayValue));
                    if (l.Type == DataType.Map && r.Type == DataType.Map) return new Value(l.MapValue.Concat(r.MapValue));
                    return new Value(l.ConvertTo(DataType.Number).NumberValue + r.ConvertTo(DataType.Number).NumberValue);
                case BinaryOperatorType.Subtract:
                    return new Value(l.ConvertTo(DataType.Number).NumberValue - r.ConvertTo(DataType.Number).NumberValue);
                case BinaryOperatorType.Multiply:
                    return new Value(l.ConvertTo(DataType.Number).NumberValue * r.ConvertTo(DataType.Number).NumberValue);
                    break;
                case BinaryOperatorType.Divide:
                    return new Value(l.ConvertTo(DataType.Number).NumberValue / r.ConvertTo(DataType.Number).NumberValue);
                case BinaryOperatorType.Modulus:
                    return new Value(l.ConvertTo(DataType.Number).NumberValue % r.ConvertTo(DataType.Number).NumberValue);
                case BinaryOperatorType.LogicalAnd:
                    return new Value(l.BooleanValue && r.BooleanValue);
                case BinaryOperatorType.LogicalOr:
                    return new Value(l.BooleanValue || r.BooleanValue);
                case BinaryOperatorType.BitwiseAnd:
                    return new Value((int)l.ConvertTo(DataType.Number).NumberValue & (int)r.ConvertTo(DataType.Number).NumberValue);
                case BinaryOperatorType.BitwiseOr:
                    return new Value((int)l.ConvertTo(DataType.Number).NumberValue | (int)r.ConvertTo(DataType.Number).NumberValue);
                case BinaryOperatorType.BitwiseXor:
                    return new Value((int)l.ConvertTo(DataType.Number).NumberValue ^ (int)r.ConvertTo(DataType.Number).NumberValue);
                case BinaryOperatorType.BitwiseLeftShift:
                    return new Value((int)l.ConvertTo(DataType.Number).NumberValue << (int)r.ConvertTo(DataType.Number).NumberValue);
                case BinaryOperatorType.BitwiseRightShift:
                    return new Value((int)l.ConvertTo(DataType.Number).NumberValue >> (int)r.ConvertTo(DataType.Number).NumberValue);
                case BinaryOperatorType.Implies:
                    // we already checked the LHS is true above, so just return the RHS
                    return r;
                case BinaryOperatorType.LessThan:
                    return new Value(l.CompareTo(r) < 0);
                case BinaryOperatorType.LessThanEqual:
                    return new Value(l.CompareTo(r) <= 0);
                case BinaryOperatorType.GreaterThan:
                    return new Value(l.CompareTo(r) > 0);
                case BinaryOperatorType.GreaterThanEqual:
                    return new Value(l.CompareTo(r) >= 0);
                case BinaryOperatorType.Equal:
                    return new Value(l.CompareTo(r) == 0);
                case BinaryOperatorType.NotEqual:
                    return new Value(l.CompareTo(r) != 0);
                case BinaryOperatorType.Range:
                    return new Value(l, r);
                default:
                    throw new ArgumentOutOfRangeException(nameof(node.Operator), node.Operator, null);
            }
        }

        private Value EvaluateInterpolatedString(InterpolatedStringNode node, EvaluationContext context)
        {
            var sb = new StringBuilder();
            foreach (var ns in node.Strings)
            {
                sb.Append(Evaluate(ns, context).AsString());
            }
            return new Value(DataType.String, sb.ToString());
        }

        private Value EvaluateLiteral(LiteralNode node, EvaluationContext context)
        {
            switch (node.Type)
            {
                case DataType.Null:
                    return Value.Null;
                case DataType.Undefined:
                    return Value.Undefined;
                case DataType.Array:
                    return new Value(node.ArrayValue.Select(x => Evaluate(x, context)));
                case DataType.Map:
                    return new Value(node.MapValue.ToDictionary(x => x.Key, x => Evaluate(x.Value, context)));
                case DataType.Range:
                    return new Value(Evaluate(node.RangeValue.start, context), Evaluate(node.RangeValue.end, context));
                case DataType.Boolean:
                case DataType.String:
                case DataType.Number:
                    return new Value(node.Type, node.ObjectValue);
                default:
                    throw new ArgumentOutOfRangeException(nameof(node.Type), node.Type, null);
            }
        }

        private Value EvaluateSubscript(SubscriptNode node, EvaluationContext context)
        {
            var val = Evaluate(node.Term, context);
            var idx = Evaluate(node.Subscript, context);
            return val.ValueAtIndex(idx);
        }

        private Value EvaluateSwitch(SwitchNode node, EvaluationContext context)
        {
            foreach (var exp in node.Cases)
            {
                var val = Evaluate(exp, context);
                if (val.Type != DataType.Undefined) return val;
            }
            return Value.Undefined;
        }

        private Value EvaluateUnary(UnaryExpressionNode node, EvaluationContext context)
        {
            var value = Evaluate(node.Term, context);
            switch (node.Operator)
            {
                case UnaryOperatorType.Plus:
                    if (value.Type == DataType.Boolean) return value.ConvertTo(DataType.Number);
                    if (value.Type == DataType.String) return value.ConvertTo(DataType.Number);
                    if (value.Type == DataType.Number) return value;
                    break;
                case UnaryOperatorType.Minus:
                    if (value.Type == DataType.Boolean) return new Value(-value.ConvertTo(DataType.Number).NumberValue);
                    if (value.Type == DataType.String) return new Value(-value.ConvertTo(DataType.Number).NumberValue);
                    if (value.Type == DataType.Number) return new Value(-value.NumberValue);
                    break;
                case UnaryOperatorType.LogicalNot:
                    return new Value(!value.BooleanValue);
                case UnaryOperatorType.BitwiseNot:
                    return new Value(~(int)value.ConvertTo(DataType.Number).NumberValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(node.Operator), node.Operator, null);
            }

            throw new InvalidOperationException();
        }

        private Value EvaluateVariable(VariableNode node, EvaluationContext context)
        {
            return context.Variables.TryGetValue(node.Name, out var v) ? v : Value.Undefined;
        }
    }
}