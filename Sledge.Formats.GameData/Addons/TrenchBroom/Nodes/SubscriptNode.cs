using Sledge.Formats.Tokens;

namespace Sledge.Formats.GameData.Addons.TrenchBroom.Nodes
{
    public class SubscriptNode : Node
    {
        public Node Term { get; set; }
        public Node Subscript { get; set; }

        public SubscriptNode(Token token, Node term, Node subscript) : base(token)
        {
            Term = term;
            Subscript = subscript;
        }
    }
}