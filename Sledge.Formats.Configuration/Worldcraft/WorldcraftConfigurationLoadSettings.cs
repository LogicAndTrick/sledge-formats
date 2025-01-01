using Microsoft.Win32;
using Sledge.Formats.Configuration.Registry;

namespace Sledge.Formats.Configuration.Worldcraft
{
    public class WorldcraftConfigurationLoadSettings
    {
        /// <summary>
        /// True to attempt to autodetect the registry location from the known default registry locations.
        /// Ignored if <see cref="RegistryLocation">RegistryLocation</see> is not null.
        /// </summary>
        public bool AutodetectRegistryLocation { get; set; } = true;

        /// <summary>
        /// The registry hive to use
        /// </summary>
        public RegistryHive RegistryHive { get; set; } = RegistryHive.CurrentUser;

        /// <summary>
        /// The registry view to use
        /// </summary>
        public RegistryView RegistryView { get; set; } = RegistryView.Default;

        /// <summary>
        /// Set to a non-null value to specify the registry location.
        /// The registry location will usually be called "Worldcraft" or "Valve Hammer Editor" and contain subkeys called "General", "2D Views", "3D Views", etc.
        /// </summary>
        public IRegistryKey RegistryLocation { get; set; }

        /// <summary>
        /// True to attempt to load game configurations from the install directory
        /// </summary>
        public bool LoadGameConfigurations { get; set; } = true;

        /// <summary>
        /// True to attempt to load command sequences from the install directory
        /// </summary>
        public bool LoadCommandSequences { get; set; } = true;

        /// <summary>
        /// True to attempt to autodetect the install directory from the registry ([Worldcraft/General/Directory] registry key).
        /// All worldcraft versions store the install directory in the registry except for version 1.0.
        /// Ignored if <see cref="InstallDirectory">InstallDirectory</see> is not null.
        /// </summary>
        public bool AutodetectInstallDirectory { get; set; } = true;

        /// <summary>
        /// Set to a non-null value to specify the install directory
        /// </summary>
        public string InstallDirectory { get; set; }

        /// <summary>
        /// The default settings for loading a configuration, with registry and install directory auto-detection enabled
        /// </summary>
        public static WorldcraftConfigurationLoadSettings Default => new WorldcraftConfigurationLoadSettings
        {
            AutodetectRegistryLocation = true,
            LoadGameConfigurations = true,
            LoadCommandSequences = true,
            AutodetectInstallDirectory = true,
        };
    }
}