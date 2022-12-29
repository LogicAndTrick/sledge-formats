using System;
using System.Collections.Generic;

namespace Sledge.Formats.GameData.Objects
{
    public class Property
    {
        public string Name { get; set; }
        public VariableType VariableType { get; set; }
        public string SubType { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public string DefaultValue { get; set; }
        public List<Option> Options { get; set; }
        public GameDataDictionary Metadata { get; set; }

        public Property(string name, VariableType variableType, string subType)
        {
            Name = name;
            VariableType = variableType;
            SubType = subType;
            Description = "";
            Details = "";
            DefaultValue = "";
            Options = new List<Option>();
            Metadata = new GameDataDictionary(string.Empty);
        }
    }
}
