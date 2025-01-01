using System.Collections.Generic;
using System.Drawing;
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
        public List<WorldcraftCommandSequence> CommandSequences { get; set; }

        public static WorldcraftConfiguration LoadFromRegistry(WorldcraftConfigurationLoadSettings settings = null)
        {
            settings = settings ?? WorldcraftConfigurationLoadSettings.Default;
            var key = settings.RegistryLocation;
            if (settings.AutodetectRegistryLocation && key == null)
            {
                var reg = new WindowsRegistry();
                var baseKey = reg.OpenBaseKey(settings.RegistryHive, settings.RegistryView);
                key = FindDefaultRegistryKey(baseKey);
            }
            if (key == null) throw new FileNotFoundException("Could not find an installation of Worldcraft in the registry.");

            var config = new WorldcraftConfiguration();
            (config.General, config.TextureDirectories) = LoadGeneralRegistry(key.OpenSubKey(WorldcraftRegistryInfo.KeyGeneral));
            config.Views2D = LoadViews2DRegistry(key.OpenSubKey(WorldcraftRegistryInfo.Key2DViews));
            config.Views3D = LoadViews3DRegistry(key.OpenSubKey(WorldcraftRegistryInfo.Key3DViews));

            var installDir = settings.InstallDirectory;
            if (settings.AutodetectInstallDirectory && installDir == null)
            {
                installDir = config.General.InstallDirectory;
            }

            if (Directory.Exists(installDir))
            {
                if (settings.LoadGameConfigurations)
                {
                    var configFile = Path.Combine(installDir, "GameCfg.wc");
                    if (File.Exists(configFile))
                    {
                        using (var fs = File.OpenRead(configFile))
                        {
                            var cfg = new WorldcraftGameConfigurationFile(fs);
                            config.GameConfigurations = cfg.Configurations;
                        }
                    }
                }

                if (settings.LoadCommandSequences)
                {
                    var cmdSeqFile = Path.Combine(installDir, "CmdSeq.wc");
                    if (File.Exists(cmdSeqFile))
                    {
                        using (var fs = File.OpenRead(cmdSeqFile))
                        {
                            var cfg = new WorldcraftCommandSequenceFile(fs);
                            config.CommandSequences = cfg.CommandSequences;
                        }
                    }
                }
            }

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

        private static Worldcraft2DViewsConfiguration LoadViews2DRegistry(IRegistryKey key)
        {
            var config = new Worldcraft2DViewsConfiguration();
            if (key != null)
            {
                config.CrosshairCursor = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsCrosshairs);
                config.DefaultTo15DegreeRotations = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsRotateConstrain);
                config.DisplayScrollbars = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsScrollbars);
                config.DrawVertices = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsDrawVertices);
                config.WhiteOnBlackColorScheme = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsWhiteOnBlack);
                config.KeepGroupWhenCloneDragging = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsKeepCloneGroup);
                config.CenterOnCameraAfterMovement = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsCenterOnCamera);
                config.UseVisgroupColorsForObjectLines = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsUseGroupColors);
                config.ArrowKeysNudgeSelectedObject = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsNudge);
                config.ReorientPrimitivesOnCreation = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsOrientPrimitives);
                config.AutomaticInfiniteSelection = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsAutoSelect);
                config.SelectionBoxSelectsByCenterHandlesOnly = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsSelectByHandles);

                config.Grid.Size = key.GetIntValue(WorldcraftRegistryInfo.Key2DViewsDefaultGrid);
                config.Grid.Intensity = key.GetIntValue(WorldcraftRegistryInfo.Key2DViewsGridIntensity);
                config.Grid.HighlightEvery64Units = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsGridHigh64);
                var nLines = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsGridHigh10);
                config.Grid.HighlightEveryNGridLines = nLines ? key.GetIntValue(WorldcraftRegistryInfo.Key2DViewsGridHighSpec) : 0;
                config.Grid.HideGridSmallerThan4Pixels = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsHideSmallGrid);
                config.Grid.HighlightEvery1024Units = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsGridHigh1024);
                config.Grid.DottedGrid = key.GetBoolValue(WorldcraftRegistryInfo.Key2DViewsGridDots);

            }
            return config;
        }

        private static Worldcraft3DViewsConfiguration LoadViews3DRegistry(IRegistryKey key)
        {
            var config = new Worldcraft3DViewsConfiguration();
            if (key != null)
            {
                //load 3d view settings into config of type Worldcraft3DViewsConfiguration:
                config.BackClippingPlane = key.GetIntValue(WorldcraftRegistryInfo.Key3DViewsBackPlane);
                config.FilterTextures = key.GetBoolValue(WorldcraftRegistryInfo.Key3DViewsFilterTextures);
                config.AnimateModels = key.GetBoolValue(WorldcraftRegistryInfo.Key3DViewsAnimateModels);
                config.ModelRenderDistance = key.GetIntValue(WorldcraftRegistryInfo.Key3DViewsModelDistance);
                config.UseMouselookNavigation = key.GetBoolValue(WorldcraftRegistryInfo.Key3DViewsUseMouseLook);
                config.ReverseMouseYAxis = key.GetBoolValue(WorldcraftRegistryInfo.Key3DViewsReverseY);
                config.ForwardSpeed = key.GetIntValue(WorldcraftRegistryInfo.Key3DViewsForwardSpeedMax);
                config.TimeToTopSpeed = key.GetIntValue(WorldcraftRegistryInfo.Key3DViewsTimeToMaxSpeed) / 1000m;
                config.ReverseSelectionOrder = key.GetBoolValue(WorldcraftRegistryInfo.Key3DViewsReverseSelection);
                config.BackgroundColor = Color.FromArgb(255, Color.FromArgb(key.GetIntValue(WorldcraftRegistryInfo.Key3DViewsClearColor)));
            }
            return config;
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
