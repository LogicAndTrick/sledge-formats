using Sledge.Formats.Tokens;

namespace Sledge.Formats.GameData.Addons.TrenchBroom.Nodes
{
    public class VariableNode : Node
    {
        public string Name { get; set; }

        public VariableNode(Token token, string name) : base(token)
        {
            Name = name;
        }
    }
}