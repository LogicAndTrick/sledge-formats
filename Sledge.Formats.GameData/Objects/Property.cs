using System;
using System.Collections.Generic;

namespace Sledge.Formats.GameData.Objects
{
    public class Property
    {
        public string Name { get; set; }
        public VariableType VariableType { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public string DefaultValue { get; set; }
        public List<Option> Options { get; set; }
        public bool ReadOnly { get; set; }
        public bool ShowInEntityReport { get; set; }

        public Property(string name, VariableType variableType)
        {
            Name = name;
            VariableType = variableType;
            Description = "";
            Details = "";
            DefaultValue = "";
            Options = new List<Option>();
            ReadOnly = false;
            ShowInEntityReport = false;
        }
    }
}
