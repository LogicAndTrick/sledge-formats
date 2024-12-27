namespace Sledge.Formats.Configuration.Worldcraft
{
    public class WorldcraftGameConfigurationBuildPrograms
    {
        /// <summary>
        /// Game executable
        /// </summary>
        public string GameExecutable { get; set; }

        /// <summary>
        /// CSG executable
        /// </summary>
        public string CsgExecutable { get; set; }

        /// <summary>
        /// BSP executable
        /// </summary>
        public string BspExecutable { get; set; }

        /// <summary>
        /// VIS executable
        /// </summary>
        public string VisExecutable { get; set; }

        /// <summary>
        /// RAD executable
        /// </summary>
        public string RadExecutable { get; set; }

        /// <summary>
        /// Place compiled maps in this directory before running the game
        /// </summary>
        public string BspDirectory { get; set; }
    }
}