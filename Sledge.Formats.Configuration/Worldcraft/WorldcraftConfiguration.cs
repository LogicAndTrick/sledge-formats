using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sledge.Formats.Configuration.Registry;

namespace Sledge.Formats.Configuration.Worldcraft
{
    public class WorldcraftConfiguration
    {
        public WorldcraftGeneralConfiguration General { get; set; }
        public Worldcraft2DViewsConfiguration Views2D { get; set; }
        public Worldcraft3DViewsConfiguration Views3D { get; set; }
        public List<string> TextureDirectories { get; set; }
        public List<WorldcraftGameConfiguration> GameConfigurations { get; set; }

        public static WorldcraftConfiguration LoadFromRegistry(WorldcraftConfigurationLoadSettings settings = null)
        {
            settings = settings ?? WorldcraftConfigurationLoadSettings.Default;
            var key = settings.RegistryLocation;
            if (settings.AutodetectRegistryLocation)
            {
                var reg = new WindowsRegistry();
                var baseKey = reg.OpenBaseKey(settings.RegistryHive, settings.RegistryView);
                key = FindDefaultRegistryKey(baseKey);
            }
            if (key == null) throw new FileNotFoundException("Could not find an installation of Worldcraft in the registry.");

            var config = new WorldcraftConfiguration();
            (config.General, config.TextureDirectories) = LoadGeneralRegistry(key.OpenSubKey(WorldcraftRegistryInfo.KeyGeneral));
            config.Views2D = null;
            config.Views3D = null;
            config.GameConfigurations = null;
            return config;
        }

        private static (WorldcraftGeneralConfiguration, List<string>) LoadGeneralRegistry(IRegistryKey key)
        {
            var config = new WorldcraftGeneralConfiguration();
            var textures = new List<string>();
            if (key != null)
            {
                config.InstallDirectory = key.GetStringValue(WorldcraftRegistryInfo.KeyGeneralDirectory);
                config.UseIndependentWindowConfigurations = key.GetBoolValue(WorldcraftRegistryInfo.KeyGeneralIndependentWindows);
                config.LoadDefaultWindowPositionsWithMaps = key.GetBoolValue(WorldcraftRegistryInfo.KeyGeneralLoadDefaultPositions);
                config.UndoLevels = key.GetIntValue(WorldcraftRegistryInfo.KeyGeneralUndoLevels);
                config.AllowGroupingWhileIgnoreGroupsChecked = key.GetBoolValue(WorldcraftRegistryInfo.KeyGeneralGroupWhileIgnore);
                config.StretchArchesToFitOriginalBoundingRectangle = key.GetBoolValue(WorldcraftRegistryInfo.KeyGeneralStretchArches);

                if (key.GetValue(WorldcraftRegistryInfo.KeyGeneralTextureFileCount, false) is int texFileCount && texFileCount > 0)
                {
                    for (var i = 0; i < texFileCount; i++)
                    {
                        var file = key.GetStringValue($"{WorldcraftRegistryInfo.KeyGeneralTextureFilePrefix}{i}");
                        if (file != null) textures.Add(file);
                    }
                }
            }

            return (config, textures);
        }

        private static IRegistryKey FindDefaultRegistryKey(IRegistryKey baseKey)
        {
            foreach (var path in WorldcraftRegistryInfo.DefaultRegistryPaths)
            {
                var key = baseKey.OpenSubKey(path);
                if (key == null) continue;
                if (key.GetSubKeyNames().Contains("2D Views")) return key;
            }
            throw new FileNotFoundException("No Worldcraft/Hammer installation could be found.");
        }
    }
}
