using System.Collections.Generic;
using System.Linq;
using Sledge.Formats.Tokens;

namespace Sledge.Formats.GameData.Addons.TrenchBroom.Nodes
{
    public class InterpolatedStringNode : Node
    {
        public List<Node> Strings { get; set; }

        public InterpolatedStringNode(Token token, IEnumerable<Node> strings) : base(token)
        {
            Strings = strings.ToList();
        }
    }
}
