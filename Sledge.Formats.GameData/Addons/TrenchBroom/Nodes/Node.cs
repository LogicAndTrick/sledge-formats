using Sledge.Formats.Tokens;

namespace Sledge.Formats.GameData.Addons.TrenchBroom.Nodes
{
    public abstract class Node
    {
        public Token Token { get; }

        protected Node(Token token)
        {
            Token = token;
        }
    }
}