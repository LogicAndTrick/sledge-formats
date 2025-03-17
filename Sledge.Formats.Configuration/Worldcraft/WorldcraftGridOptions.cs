namespace Sledge.Formats.Configuration.Worldcraft
{
    public class WorldcraftGridOptions
    {
        /// <summary>
        /// Size
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Intensity (0-100)
        /// </summary>
        public int Intensity { get; set; }

        /// <summary>
        /// Highlight every 64 units
        /// </summary>
        public bool HighlightEvery64Units { get; set; }

        /// <summary>
        /// Highlight every N grid lines, set to 0 to disable
        /// </summary>
        public int HighlightEveryNGridLines { get; set; }

        /// <summary>
        /// Hide grid smaller than 4 pixels
        /// </summary>
        public bool HideGridSmallerThan4Pixels { get; set; }

        /// <summary>
        /// Highlight every 1024 units
        /// </summary>
        public bool HighlightEvery1024Units { get; set; }

        /// <summary>
        /// Dotted Grid
        /// </summary>
        public bool DottedGrid { get; set; }
    }
}