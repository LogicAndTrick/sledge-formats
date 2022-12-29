namespace Sledge.Formats.GameData.Objects
{
    public class VisgroupFilter
    {
        public string FilterType { get; set; }
        public string Material { get; set; }
        public string Tag { get; set; }
        public string Group { get; set; }
        public string ParentGroup { get; set; }
        public GameDataDictionary Metadata { get; set; }

        public VisgroupFilter(GameDataDictionary metadata)
        {
            FilterType = metadata.ContainsKey("filter_type") ? metadata["filter_type"].Value as string : null;
            Material = metadata.ContainsKey("material") ? metadata["material"].Value as string : null;
            Tag = metadata.ContainsKey("tag") ? metadata["tag"].Value as string : null;
            Group = metadata.ContainsKey("group") ? metadata["group"].Value as string : null;
            ParentGroup = metadata.ContainsKey("parent_group") ? metadata["parent_group"].Value as string : null;
            Metadata = metadata;
        }
    }
}