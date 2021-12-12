using System;

namespace Sledge.Formats.GameData.Objects
{
    public class Option
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public bool On { get; set; }

        public Option()
        {
            Key = "";
            Description = "";
            Details = "";
            On = false;
        }
    }
}
