using System.Collections.Generic;

namespace Sledge.Formats.Map.Objects
{
    public class Entity : MapObject
    {
        public string ClassName { get; set; }
        public int SpawnFlags { get; set; }
        public List<KeyValuePair<string, string>> SortedProperties { get; set; }
        public IDictionary<string, string> Properties { get; }

        public Entity()
        {
            SortedProperties = new List<KeyValuePair<string, string>>();
            Properties = new SortedKeyValueDictionaryWrapper(SortedProperties);
        }
    }
}