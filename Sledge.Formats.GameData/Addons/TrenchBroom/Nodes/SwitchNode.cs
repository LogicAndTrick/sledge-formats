using System.Collections.Generic;
using System.Linq;
using Sledge.Formats.Tokens;

namespace Sledge.Formats.GameData.Addons.TrenchBroom.Nodes
{
    public class SwitchNode : Node
    {
        public List<Node> Cases { get; set; }

        public SwitchNode(Token token, IEnumerable<Node> cases) : base(token)
        {
            Cases = cases.ToList();
        }
    }
}
