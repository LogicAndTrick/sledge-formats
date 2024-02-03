namespace Sledge.Formats.Map.Objects
{
    public class Prefab
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public MapFile Map { get; set; }

        public Prefab()
        {
            //
        }

        public Prefab(string name, string description, MapFile map)
        {
            Name = name;
            Description = description;
            Map = map;
        }
    }
}
