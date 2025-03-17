using System.Collections.Generic;

namespace Sledge.Formats.Configuration.Worldcraft
{
    /// <summary>
    /// A sequence of commands to run when compiling a map
    /// </summary>
    public class WorldcraftCommandSequence
    {
        /// <summary>
        /// Command sequence name
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Command sequence steps
        /// </summary>
        public List<WorldcraftCommandSequenceStep> Steps { get; set; } = new List<WorldcraftCommandSequenceStep>();
    }
}