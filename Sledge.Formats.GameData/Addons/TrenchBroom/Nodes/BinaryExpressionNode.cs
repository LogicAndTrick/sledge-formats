using Sledge.Formats.Tokens;

namespace Sledge.Formats.GameData.Addons.TrenchBroom.Nodes
{
    public class BinaryExpressionNode : Node
    {
        public BinaryOperatorType Operator { get; }
        public Node Left { get; }
        public Node Right { get; }

        public BinaryExpressionNode(Token token, BinaryOperatorType op, Node left, Node right) : base(token)
        {
            Operator = op;
            Left = left;
            Right = right;
        }
    }
}