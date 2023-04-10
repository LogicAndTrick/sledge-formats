using System.Collections.Generic;

namespace Sledge.Formats.GameData.Addons.TrenchBroom
{
    public class EvaluationContext
    {
        public Dictionary<string, Value> Variables { get; set; }

        public EvaluationContext() : this(new Dictionary<string, Value>())
        {
        }

        public EvaluationContext(Dictionary<string, Value> variables)
        {
            Variables = variables;
        }
    }
}
