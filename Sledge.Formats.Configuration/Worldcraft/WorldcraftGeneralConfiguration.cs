namespace Sledge.Formats.Configuration.Worldcraft
{
    public class WorldcraftGeneralConfiguration
    {
        /// <summary>
        /// Worldcraft install directory
        /// </summary>
        public string InstallDirectory { get; set; }

        /// <summary>
        /// Use independent window configurations
        /// </summary>
        public bool UseIndependentWindowConfigurations { get; set; }

        /// <summary>
        /// Load default window positions with maps
        /// </summary>
        public bool LoadDefaultWindowPositionsWithMaps { get; set; }

        /// <summary>
        /// Undo levels
        /// </summary>
        public int UndoLevels { get; set; }

        /// <summary>
        /// Allow grouping/ungrouping while Ignore Groups is checked
        /// </summary>
        public bool AllowGroupingWhileIgnoreGroupsChecked { get; set; }

        /// <summary>
        /// Stretch arches to fit original bounding rectangle
        /// </summary>
        public bool StretchArchesToFitOriginalBoundingRectangle { get; set; }
    }
}