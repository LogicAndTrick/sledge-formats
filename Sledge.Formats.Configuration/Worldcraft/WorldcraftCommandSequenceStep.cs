using System;

namespace Sledge.Formats.Configuration.Worldcraft
{
    /// <summary>
    /// A step in a command sequence representing a single command to run
    /// </summary>
    public class WorldcraftCommandSequenceStep
    {
        /// <summary>
        /// Whether this step is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// The type of command to run
        /// </summary>
        public CommandType Type { get; set; }

        /// <summary>
        /// The command to run
        /// </summary>
        public string Command { get; set; } = "";

        /// <summary>
        /// The command arguments
        /// </summary>
        public string Arguments { get; set; } = "";

        /// <summary>
        /// Doesn't seem to actually do anything, likely from older Windows where 8.3 filenames were common. Only kept since VHE has the option and it's stored in the configuration file.
        /// </summary>
        [Obsolete] public bool UseLongFileNames { get; set; } = true;

        /// <summary>
        /// Check if a file exists after running the command
        /// </summary>
        public bool EnsureFileExists { get; set; } = false;

        /// <summary>
        /// The file to check for after running the command
        /// </summary>
        public string FileExistsName { get; set; } = "";

        /// <summary>
        /// True to display a command window when running
        /// </summary>
        public bool UseProcessWindow { get; set; } = true;
    }
}