using System;
using System.Collections.Generic;
using System.Text;

namespace Sledge.Formats.GameData.Objects
{
    public class GameDataDictionary : Dictionary<string, GameDataDictionaryValue>
    {
        public string Name { get; }

        public GameDataDictionary(string name)
        {
            Name = name;
        }
    }
}
