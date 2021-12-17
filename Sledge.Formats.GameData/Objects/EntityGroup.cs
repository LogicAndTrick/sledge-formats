using System;
using System.Collections.Generic;
using System.Text;

namespace Sledge.Formats.GameData.Objects
{
    public class EntityGroup
    {
        public string Name { get; set; }
        public bool StartExpanded { get; set; }

        public EntityGroup(string name)
        {
            Name = name;
        }
    }
}
