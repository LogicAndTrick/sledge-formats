using System.Collections.Generic;
using System.Linq;

namespace Sledge.Formats.GameData.Objects
{
    public class GameDataDictionary : Dictionary<string, GameDataDictionaryValue>
    {
        public string Name { get; }

        public GameDataDictionary(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name + " { " + string.Join(", ", this.Select(kv => kv.Key + " = " + kv.Value)) + " }";
        }
    }
}
