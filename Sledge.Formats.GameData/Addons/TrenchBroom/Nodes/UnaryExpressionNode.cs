using Sledge.Formats.Tokens;

namespace Sledge.Formats.GameData.Addons.TrenchBroom.Nodes
{
    public class UnaryExpressionNode : Node
    {
        public UnaryOperatorType Operator { get; }
        public Node Term { get; }

        public UnaryExpressionNode(Token token, UnaryOperatorType op, Node term) : base(token)
        {
            Operator = op;
            Term = term;
        }
    }
}