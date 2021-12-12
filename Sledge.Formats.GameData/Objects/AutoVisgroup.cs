using System.Collections.Generic;

namespace Sledge.Formats.GameData.Objects
{
    public class AutoVisgroup
    {
        public string Name { get; set; }
        public List<string> EntityNames { get; private set; }

        public AutoVisgroup()
        {
            EntityNames = new List<string>();
        }
    }
}