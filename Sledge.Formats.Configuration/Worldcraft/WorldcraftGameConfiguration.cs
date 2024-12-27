using System;
using System.Collections.Generic;

namespace Sledge.Formats.Configuration.Worldcraft
{
    public class WorldcraftGameConfiguration
    {
        /// <summary>
        /// Configuration name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of game data files (.fgd)
        /// </summary>
        public List<string> GameDataFiles { get; set; }

        /// <summary>
        /// Texture Format
        /// </summary>
        public TextureFormat TextureFormat { get; set; }

        /// <summary>
        /// Map Type
        /// </summary>
        public MapType MapType { get; set; }

        /// <summary>
        /// Default PointEntity class
        /// </summary>
        public string DefaultPointEntityClass { get; set; }

        /// <summary>
        /// Default SolidEntity class
        /// </summary>
        public string DefaultSolidEntityClass { get; set; }

        /// <summary>
        /// Game executable directory (ex: C:\HalfLife)
        /// </summary>
        public string GameExecutableDirectory { get; set; }

        /// <summary>
        /// Mod directory (ex: C:\HalfLife\tfc)
        /// </summary>
        public string ModDirectory { get; set; }

        /// <summary>
        /// Game directory (ex: C:\HalfLife\valve)
        /// </summary>
        public string GameDirectory { get; set; }

        /// <summary>
        /// RMF directory
        /// </summary>
        public string RmfDirectory { get; set; }

        /// <summary>
        /// Palette file
        /// </summary>
        [Obsolete] public string PaletteFile { get; set; }

        /// <summary>
        /// Build programs for this configuration
        /// </summary>
        public WorldcraftGameConfigurationBuildPrograms BuildPrograms { get; set; }
    }
}