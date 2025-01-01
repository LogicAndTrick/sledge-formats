using System.Drawing;
using Microsoft.Win32;
using Sledge.Formats.Configuration.Worldcraft;

namespace Sledge.Formats.Configuration.Tests;

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CA1416

[TestClass]
public sealed class TestWorldcraftConfiguration
{
    /// <summary>
    /// This test is just for information from your local PC, it will not fail.
    /// </summary>
    [TestMethod]
    public void TestLoadSettingsFromLocalComputer()
    {
        try
        {
            var config = WorldcraftConfiguration.LoadFromRegistry(WorldcraftConfigurationLoadSettings.Default);
            Console.WriteLine("Install directory: " + config.General.InstallDirectory);
            Console.WriteLine("Undo levels: " + config.General.UndoLevels);
            Console.WriteLine("Textures: " + string.Join("; ", config.TextureDirectories));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to load config from local computer.");
            Console.WriteLine(ex.Message);
        }
    }

    [TestMethod]
    public void TestLoadSettings()
    {
        var reg = RegistryUtil.CreateRegistryFromRegString(Worldcraft33RegString);
        var config = WorldcraftConfiguration.LoadFromRegistry(new WorldcraftConfigurationLoadSettings
        {
            LoadGameConfigurations = false,
            LoadCommandSequences = false,
            RegistryLocation = reg.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"Software\Valve\Worldcraft")
        });

        Assert.IsNotNull(config.General);
        Assert.IsNotNull(config.Views2D);
        Assert.IsNotNull(config.Views3D);
        Assert.IsNotNull(config.TextureDirectories);
        Assert.IsNull(config.GameConfigurations);
        Assert.IsNull(config.CommandSequences);

        Assert.AreEqual(@"C:\Users\WDAGUtilityAccount\Desktop\Worldcraft 3.3", config.General.InstallDirectory, StringComparer.InvariantCultureIgnoreCase);
        Assert.AreEqual(true, config.General.UseIndependentWindowConfigurations);
        Assert.AreEqual(false, config.General.LoadDefaultWindowPositionsWithMaps);
        Assert.AreEqual(0x32, config.General.UndoLevels);
        Assert.AreEqual(true, config.General.AllowGroupingWhileIgnoreGroupsChecked);
        Assert.AreEqual(false, config.General.StretchArchesToFitOriginalBoundingRectangle);

        Assert.AreEqual(true, config.Views2D.CrosshairCursor);
        Assert.AreEqual(true, config.Views2D.DefaultTo15DegreeRotations);
        Assert.AreEqual(false, config.Views2D.DisplayScrollbars);
        Assert.AreEqual(false, config.Views2D.DrawVertices);
        Assert.AreEqual(false, config.Views2D.WhiteOnBlackColorScheme);
        Assert.AreEqual(false, config.Views2D.KeepGroupWhenCloneDragging);
        Assert.AreEqual(true, config.Views2D.CenterOnCameraAfterMovement);
        Assert.AreEqual(false, config.Views2D.UseVisgroupColorsForObjectLines);
        Assert.AreEqual(true, config.Views2D.ArrowKeysNudgeSelectedObject);
        Assert.AreEqual(true, config.Views2D.ReorientPrimitivesOnCreation);
        Assert.AreEqual(true, config.Views2D.AutomaticInfiniteSelection);
        Assert.AreEqual(true, config.Views2D.SelectionBoxSelectsByCenterHandlesOnly);
        Assert.AreEqual(0x20, config.Views2D.Grid.Size);
        Assert.AreEqual(0x42, config.Views2D.Grid.Intensity);
        Assert.AreEqual(true, config.Views2D.Grid.HighlightEvery64Units);
        Assert.AreEqual(0x00, config.Views2D.Grid.HighlightEveryNGridLines);
        Assert.AreEqual(false, config.Views2D.Grid.HideGridSmallerThan4Pixels);
        Assert.AreEqual(false, config.Views2D.Grid.HighlightEvery1024Units);
        Assert.AreEqual(false, config.Views2D.Grid.DottedGrid);

        Assert.AreEqual(0x1ef9, config.Views3D.BackClippingPlane);
        Assert.AreEqual(false, config.Views3D.FilterTextures);
        Assert.AreEqual(true, config.Views3D.AnimateModels);
        Assert.AreEqual(0x190, config.Views3D.ModelRenderDistance);
        Assert.AreEqual(false, config.Views3D.UseMouselookNavigation);
        Assert.AreEqual(true, config.Views3D.ReverseMouseYAxis);
        Assert.AreEqual(0x0b0d, config.Views3D.ForwardSpeed);
        Assert.AreEqual(0x0783 / 1000m, config.Views3D.TimeToTopSpeed);
        Assert.AreEqual(true, config.Views3D.ReverseSelectionOrder);
        Assert.AreEqual(Color.Black.ToArgb(), config.Views3D.BackgroundColor.ToArgb());

        CollectionAssert.AreEqual(new[] { @"c:\users\wdagutilityaccount\desktop\zhlt.wad" }, config.TextureDirectories, StringComparer.InvariantCultureIgnoreCase);
    }

    [TestMethod]
    public void TestReadGameConfigFile20()
    {
        using var file = typeof(TestWorldcraftConfiguration).Assembly.GetManifestResourceStream("Sledge.Formats.Configuration.Tests.Resources.Worldcraft.GameCfg-wc20.wc");
        var configs = new WorldcraftGameConfigurationFile(file).Configurations;

        Assert.AreEqual(configs.Count, 1);
        var config = configs[0];

        Assert.AreEqual("test", config.Name);
        CollectionAssert.AreEqual(new[] { @"C:\fgd\a.fgd" }, config.GameDataFiles);
        Assert.AreEqual(TextureFormat.Wad3, config.TextureFormat);
        Assert.AreEqual(MapType.HalfLife, config.MapType);
        Assert.AreEqual("ammo_357", config.DefaultPointEntityClass);
        Assert.AreEqual("worldspawn", config.DefaultSolidEntityClass);
        Assert.AreEqual("a", config.GameExecutableDirectory);
        Assert.AreEqual("", config.ModDirectory);
        Assert.AreEqual("", config.GameDirectory);
        Assert.AreEqual("b", config.RmfDirectory);
        Assert.AreEqual("c", config.PaletteFile);
        Assert.AreEqual("d", config.BuildPrograms.GameExecutable);
        Assert.AreEqual("", config.BuildPrograms.CsgExecutable);
        Assert.AreEqual("e", config.BuildPrograms.BspExecutable);
        Assert.AreEqual("g", config.BuildPrograms.VisExecutable);
        Assert.AreEqual("f", config.BuildPrograms.RadExecutable);
        Assert.AreEqual("h", config.BuildPrograms.BspDirectory);
    }

    [TestMethod]
    public void TestReadGameConfigFile33()
    {
        using var file = typeof(TestWorldcraftConfiguration).Assembly.GetManifestResourceStream("Sledge.Formats.Configuration.Tests.Resources.Worldcraft.GameCfg-wc33.wc");
        var configs = new WorldcraftGameConfigurationFile(file).Configurations;

        Assert.AreEqual(configs.Count, 1);
        var config = configs[0];

        Assert.AreEqual("Half-Life", config.Name);
        CollectionAssert.AreEqual(new[] { @"C:\fgd\a.fgd", @"C:\fgd\b.fgd", @"C:\fgd\halflife.fgd", }, config.GameDataFiles);
        Assert.AreEqual(TextureFormat.Wad3, config.TextureFormat);
        Assert.AreEqual(MapType.HalfLife, config.MapType);
        Assert.AreEqual("light", config.DefaultPointEntityClass);
        Assert.AreEqual("func_door", config.DefaultSolidEntityClass);
        Assert.AreEqual(@"C:\hl", config.GameExecutableDirectory);
        Assert.AreEqual(@"C:\hl\cstrike", config.ModDirectory);
        Assert.AreEqual(@"C:\hl\valve", config.GameDirectory);
        Assert.AreEqual(@"C:\rmf", config.RmfDirectory);
        Assert.AreEqual("", config.PaletteFile);
        Assert.AreEqual(@"C:\hl\hl.exe", config.BuildPrograms.GameExecutable);
        Assert.AreEqual(@"C:\hl\build\csg.exe", config.BuildPrograms.CsgExecutable);
        Assert.AreEqual(@"C:\hl\build\bsp.exe", config.BuildPrograms.BspExecutable);
        Assert.AreEqual(@"C:\hl\build\vis.exe", config.BuildPrograms.VisExecutable);
        Assert.AreEqual(@"C:\hl\build\rad.exe", config.BuildPrograms.RadExecutable);
        Assert.AreEqual(@"C:\hl\cstrike\maps", config.BuildPrograms.BspDirectory);
    }

    [TestMethod]
    public void TestWriteGameConfigFile20()
    {
        var file = new WorldcraftGameConfigurationFile();
        file.Configurations.Add(new WorldcraftGameConfiguration
        {
            Name = "Half-Life",
            GameDataFiles = [@"C:\fgd\a.fgd", @"C:\fgd\b.fgd", @"C:\fgd\halflife.fgd"],
            TextureFormat = TextureFormat.Wad3,
            MapType = MapType.HalfLife,
            DefaultPointEntityClass = "light",
            DefaultSolidEntityClass = "func_door",
            GameExecutableDirectory = @"C:\hl",
            PaletteFile = "palette.pal",
            RmfDirectory = @"C:\rmf",
            BuildPrograms = new WorldcraftGameConfigurationBuildPrograms
            {
                GameExecutable = @"C:\hl\hl.exe",
                BspExecutable = @"C:\hl\build\bsp.exe",
                VisExecutable = @"C:\hl\build\vis.exe",
                RadExecutable = @"C:\hl\build\rad.exe",
                BspDirectory = @"C:\hl\cstrike\maps",
            }
        });

        var ms = new MemoryStream();
        file.Write(ms, WorldcraftGameConfigurationFile.MinVersion);
        ms.Position = 0;

        var file2 = new WorldcraftGameConfigurationFile(ms);
        Assert.AreEqual(file.Configurations[0].Name, file2.Configurations[0].Name);
        CollectionAssert.AreEqual(file.Configurations[0].GameDataFiles, file2.Configurations[0].GameDataFiles);
        Assert.AreEqual(file.Configurations[0].TextureFormat, file2.Configurations[0].TextureFormat);
        Assert.AreEqual(file.Configurations[0].MapType, file2.Configurations[0].MapType);
        Assert.AreEqual(file.Configurations[0].DefaultPointEntityClass, file2.Configurations[0].DefaultPointEntityClass);
        Assert.AreEqual(file.Configurations[0].DefaultSolidEntityClass, file2.Configurations[0].DefaultSolidEntityClass);
        Assert.AreEqual(file.Configurations[0].GameExecutableDirectory, file2.Configurations[0].GameExecutableDirectory);
        Assert.AreEqual(file.Configurations[0].ModDirectory, file2.Configurations[0].ModDirectory);
        Assert.AreEqual(file.Configurations[0].GameDirectory, file2.Configurations[0].GameDirectory);
        Assert.AreEqual(file.Configurations[0].RmfDirectory, file2.Configurations[0].RmfDirectory);
        Assert.AreEqual(file.Configurations[0].PaletteFile, file2.Configurations[0].PaletteFile);
        Assert.AreEqual(file.Configurations[0].BuildPrograms.GameExecutable, file2.Configurations[0].BuildPrograms.GameExecutable);
        Assert.AreEqual(file.Configurations[0].BuildPrograms.CsgExecutable, file2.Configurations[0].BuildPrograms.CsgExecutable);
        Assert.AreEqual(file.Configurations[0].BuildPrograms.BspExecutable, file2.Configurations[0].BuildPrograms.BspExecutable);
        Assert.AreEqual(file.Configurations[0].BuildPrograms.VisExecutable, file2.Configurations[0].BuildPrograms.VisExecutable);
        Assert.AreEqual(file.Configurations[0].BuildPrograms.RadExecutable, file2.Configurations[0].BuildPrograms.RadExecutable);
        Assert.AreEqual(file.Configurations[0].BuildPrograms.BspDirectory, file2.Configurations[0].BuildPrograms.BspDirectory);
    }

    [TestMethod]
    public void TestWriteGameConfigFile33()
    {
        var file = new WorldcraftGameConfigurationFile();
        file.Configurations.Add(new WorldcraftGameConfiguration
        {
            Name = "Half-Life",
            GameDataFiles = [@"C:\fgd\a.fgd", @"C:\fgd\b.fgd", @"C:\fgd\halflife.fgd"],
            TextureFormat = TextureFormat.Wad3,
            MapType = MapType.HalfLife,
            DefaultPointEntityClass = "light",
            DefaultSolidEntityClass = "func_door",
            GameExecutableDirectory = @"C:\hl",
            ModDirectory = @"C:\hl\cstrike",
            GameDirectory = @"C:\hl\valve",
            RmfDirectory = @"C:\rmf",
            BuildPrograms = new WorldcraftGameConfigurationBuildPrograms
            {
                GameExecutable = @"C:\hl\hl.exe",
                CsgExecutable = @"C:\hl\build\csg.exe",
                BspExecutable = @"C:\hl\build\bsp.exe",
                VisExecutable = @"C:\hl\build\vis.exe",
                RadExecutable = @"C:\hl\build\rad.exe",
                BspDirectory = @"C:\hl\cstrike\maps",
            }
        });

        var ms = new MemoryStream();
        file.Write(ms, WorldcraftGameConfigurationFile.MaxVersion);
        ms.Position = 0;

        var file2 = new WorldcraftGameConfigurationFile(ms);
        Assert.AreEqual(file.Configurations[0].Name, file2.Configurations[0].Name);
        CollectionAssert.AreEqual(file.Configurations[0].GameDataFiles, file2.Configurations[0].GameDataFiles);
        Assert.AreEqual(file.Configurations[0].TextureFormat, file2.Configurations[0].TextureFormat);
        Assert.AreEqual(file.Configurations[0].MapType, file2.Configurations[0].MapType);
        Assert.AreEqual(file.Configurations[0].DefaultPointEntityClass, file2.Configurations[0].DefaultPointEntityClass);
        Assert.AreEqual(file.Configurations[0].DefaultSolidEntityClass, file2.Configurations[0].DefaultSolidEntityClass);
        Assert.AreEqual(file.Configurations[0].GameExecutableDirectory, file2.Configurations[0].GameExecutableDirectory);
        Assert.AreEqual(file.Configurations[0].ModDirectory, file2.Configurations[0].ModDirectory);
        Assert.AreEqual(file.Configurations[0].GameDirectory, file2.Configurations[0].GameDirectory);
        Assert.AreEqual(file.Configurations[0].RmfDirectory, file2.Configurations[0].RmfDirectory);
        Assert.AreEqual(file.Configurations[0].PaletteFile, file2.Configurations[0].PaletteFile);
        Assert.AreEqual(file.Configurations[0].BuildPrograms.GameExecutable, file2.Configurations[0].BuildPrograms.GameExecutable);
        Assert.AreEqual(file.Configurations[0].BuildPrograms.CsgExecutable, file2.Configurations[0].BuildPrograms.CsgExecutable);
        Assert.AreEqual(file.Configurations[0].BuildPrograms.BspExecutable, file2.Configurations[0].BuildPrograms.BspExecutable);
        Assert.AreEqual(file.Configurations[0].BuildPrograms.VisExecutable, file2.Configurations[0].BuildPrograms.VisExecutable);
        Assert.AreEqual(file.Configurations[0].BuildPrograms.RadExecutable, file2.Configurations[0].BuildPrograms.RadExecutable);
        Assert.AreEqual(file.Configurations[0].BuildPrograms.BspDirectory, file2.Configurations[0].BuildPrograms.BspDirectory);
    }

    [TestMethod]
    public void TestReadCommandSequenceFile15()
    {
        using var file = typeof(TestWorldcraftConfiguration).Assembly.GetManifestResourceStream("Sledge.Formats.Configuration.Tests.Resources.Worldcraft.CmdSeq-wc15.wc");
        var sequences = new WorldcraftCommandSequenceFile(file).CommandSequences;

        Assert.AreEqual(2, sequences.Count);
        var (seq1, seq2) = (sequences[0], sequences[1]);

        Assert.AreEqual("Default", seq1.Name);

        Assert.AreEqual(true, seq1.Steps[0].IsEnabled);
        Assert.AreEqual("a", seq1.Steps[0].Command);
        Assert.AreEqual("b", seq1.Steps[0].Arguments);
        Assert.AreEqual(true, seq1.Steps[0].UseLongFileNames);
        Assert.AreEqual(true, seq1.Steps[0].EnsureFileExists);
        Assert.AreEqual("", seq1.Steps[0].FileExistsName);
        Assert.AreEqual(false, seq1.Steps[0].UseProcessWindow);

        Assert.AreEqual(false, seq1.Steps[1].IsEnabled);
        Assert.AreEqual("c", seq1.Steps[1].Command);
        Assert.AreEqual("d", seq1.Steps[1].Arguments);
        Assert.AreEqual(false, seq1.Steps[1].UseLongFileNames);
        Assert.AreEqual(false, seq1.Steps[1].EnsureFileExists);
        Assert.AreEqual("", seq1.Steps[1].FileExistsName);
        Assert.AreEqual(true, seq1.Steps[1].UseProcessWindow);

        Assert.AreEqual("Test", seq2.Name);

        Assert.AreEqual(false, seq2.Steps[0].IsEnabled);
        Assert.AreEqual("test1", seq2.Steps[0].Command);
        Assert.AreEqual("test2", seq2.Steps[0].Arguments);
        Assert.AreEqual(false, seq2.Steps[0].UseLongFileNames);
        Assert.AreEqual(true, seq2.Steps[0].EnsureFileExists);
        Assert.AreEqual("test3", seq2.Steps[0].FileExistsName);
        Assert.AreEqual(true, seq2.Steps[0].UseProcessWindow);
    }

    [TestMethod]
    public void TestReadCommandSequenceFile33()
    {
        using var file = typeof(TestWorldcraftConfiguration).Assembly.GetManifestResourceStream("Sledge.Formats.Configuration.Tests.Resources.Worldcraft.CmdSeq-wc33.wc"); var sequences = new WorldcraftCommandSequenceFile(file).CommandSequences;

        Assert.AreEqual(4, sequences.Count);
        var (seq1, seq2, seq3, seq4) = (sequences[0], sequences[1], sequences[2], sequences[3]);

        Assert.AreEqual("Half-Life (full)", seq1.Name);
        Assert.AreEqual("Half-Life: Counterstrike (full)", seq2.Name);
        Assert.AreEqual("Half-Life: Opposing Force (full)", seq3.Name);
        Assert.AreEqual("Half-Life: Team Fortress (full)", seq4.Name);

        AssertSteps("valve", seq1);
        AssertSteps("cstrike", seq2);
        AssertSteps("gearbox", seq3);
        AssertSteps("tfc", seq4);

        static void AssertSteps(string game, WorldcraftCommandSequence sequence)
        {
            Assert.AreEqual(8, sequence.Steps.Count);

            AssertStep(true, "Change Directory", "$exedir", true, false, "", true, sequence.Steps[0]);
            AssertStep(true, "$csg_exe", @"$path\$file", true, false, "", true, sequence.Steps[1]);
            AssertStep(true, "$bsp_exe", @"$path\$file", true, false, "", true, sequence.Steps[2]);
            AssertStep(true, "$vis_exe", @"$path\$file", true, false, "", true, sequence.Steps[3]);
            AssertStep(true, "$light_exe", @"$path\$file", true, false, "", true, sequence.Steps[4]);
            AssertStep(true, "Copy File", @"$path\$file.bsp $bspdir\$file.bsp", true, false, "", true, sequence.Steps[5]);
            AssertStep(true, "Copy File", @"$path\$file.pts $bspdir\$file.pts", true, false, "", true, sequence.Steps[6]);
            switch (game)
            {
                case "valve":
                    AssertStep(true, "$game_exe", "+map $file -dev -console", true, false, "", false, sequence.Steps[7]);
                    break;
                case "cstrike":
                    AssertStep(true, "$game_exe", "+map $file -game cstrike -dev -console +deathmatch 1", true, false, "", false, sequence.Steps[7]);
                    break;
                case "gearbox":
                    AssertStep(true, "$game_exe", "+map $file -game gearbox -dev -console", true, false, "", false, sequence.Steps[7]);
                    break;
                case "tfc":
                    AssertStep(true, "$game_exe", "+map $file -game tfc -dev -console -toconsole +sv_lan 1", true, false, "", false, sequence.Steps[7]);
                    break;
            }
        }

        static void AssertStep(bool isEnabled, string command, string args, bool useLongFileNames, bool ensureFileExists, string fileExistsName, bool useProcessWindow, WorldcraftCommandSequenceStep step)
        {
            Assert.AreEqual(isEnabled, step.IsEnabled);
            Assert.AreEqual(command, step.Command);
            Assert.AreEqual(args, step.Arguments);
            Assert.AreEqual(useLongFileNames, step.UseLongFileNames);
            Assert.AreEqual(ensureFileExists, step.EnsureFileExists);
            Assert.AreEqual(fileExistsName, step.FileExistsName);
            Assert.AreEqual(useProcessWindow, step.UseProcessWindow);
        }
    }

    [DataTestMethod]
    [DataRow(0.1f)]
    [DataRow(0.2f)]
    public void TestWriteCommandSequencesFile(float version)
    {
        // The two known versions are identical aside from an unused field, so we can test them together
        var file = new WorldcraftCommandSequenceFile();
        file.CommandSequences.Add(new WorldcraftCommandSequence
        {
            Name = "Test1",
            Steps =
            [
                new WorldcraftCommandSequenceStep
                {
                    IsEnabled = true,
                    Command = "test1.step1.command",
                    Arguments = "test1.step1.args",
                    UseLongFileNames = true,
                    EnsureFileExists = false,
                    FileExistsName = "",
                    UseProcessWindow = true
                },
                new WorldcraftCommandSequenceStep
                {
                    IsEnabled = false,
                    Command = "test1.step2.command",
                    Arguments = "test1.step2.args",
                    UseLongFileNames = false,
                    EnsureFileExists = true,
                    FileExistsName = "test1.step2.filename",
                    UseProcessWindow = false
                },
            ],
        });
        file.CommandSequences.Add(new WorldcraftCommandSequence
        {
            Name = "Test2",
            Steps =
            [
                new WorldcraftCommandSequenceStep
                {
                    IsEnabled = true,
                    Command = "test2.step1.command",
                    Arguments = "test2.step1.args",
                    UseLongFileNames = true,
                    EnsureFileExists = false,
                    FileExistsName = "",
                    UseProcessWindow = true
                },
            ],
        });

        var ms = new MemoryStream();
        file.Write(ms, version);
        ms.Position = 0;

        var file2 = new WorldcraftCommandSequenceFile(ms);
        Assert.AreEqual(file.CommandSequences.Count, file2.CommandSequences.Count);
        AssertSequence(file.CommandSequences[0], file2.CommandSequences[0]);
        AssertSequence(file.CommandSequences[1], file2.CommandSequences[1]);

        static void AssertSequence(WorldcraftCommandSequence expected, WorldcraftCommandSequence actual)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Steps.Count, actual.Steps.Count);
            for (var i = 0; i < expected.Steps.Count; i++)
            {
                Assert.AreEqual(expected.Steps[i].IsEnabled, actual.Steps[i].IsEnabled);
                Assert.AreEqual(expected.Steps[i].Command, actual.Steps[i].Command);
                Assert.AreEqual(expected.Steps[i].Arguments, actual.Steps[i].Arguments);
                Assert.AreEqual(expected.Steps[i].UseLongFileNames, actual.Steps[i].UseLongFileNames);
                Assert.AreEqual(expected.Steps[i].EnsureFileExists, actual.Steps[i].EnsureFileExists);
                Assert.AreEqual(expected.Steps[i].FileExistsName, actual.Steps[i].FileExistsName);
                Assert.AreEqual(expected.Steps[i].UseProcessWindow, actual.Steps[i].UseProcessWindow);
            }
        }
    }

    private const string Worldcraft33RegString = """
                                                 Windows Registry Editor Version 5.00

                                                 [HKEY_CURRENT_USER\SOFTWARE\Valve]

                                                 [HKEY_CURRENT_USER\SOFTWARE\Valve\Worldcraft]

                                                 [HKEY_CURRENT_USER\SOFTWARE\Valve\Worldcraft\2D Views]
                                                 "Crosshairs"=dword:00000001
                                                 "GroupCarve"=dword:00000001
                                                 "Scrollbars"=dword:00000000
                                                 "RotateConstrain"=dword:00000001
                                                 "Draw Vertices"=dword:00000000
                                                 "Default Grid"=dword:00000020
                                                 "WhiteOnBlack"=dword:00000000
                                                 "GridHigh10"=dword:00000000
                                                 "GridIntensity"=dword:00000042
                                                 "HideSmallGrid"=dword:00000000
                                                 "Nudge"=dword:00000001
                                                 "OrientPrimitives"=dword:00000001
                                                 "AutoSelect"=dword:00000001
                                                 "SelectByHandles"=dword:00000001
                                                 "GridHighSpec"=dword:00000008
                                                 "KeepCloneGroup"=dword:00000000
                                                 "Gridhigh64"=dword:00000001
                                                 "GridDots"=dword:00000000
                                                 "Centeroncamera"=dword:00000001
                                                 "Usegroupcolors"=dword:00000000

                                                 [HKEY_CURRENT_USER\SOFTWARE\Valve\Worldcraft\3D Views]
                                                 "Hardware"=dword:00000000
                                                 "Reverse Y"=dword:00000001
                                                 "BackPlane"=dword:00001ef9
                                                 "UseMouseLook"=dword:00000000
                                                 "ModelDistance"=dword:00000190
                                                 "AnimateModels"=dword:00000001
                                                 "ForwardSpeedMax"=dword:00000b0d
                                                 "TimeToMaxSpeed"=dword:00000783
                                                 "FilterTextures"=dword:00000000
                                                 "ReverseSelection"=dword:00000001

                                                 [HKEY_CURRENT_USER\SOFTWARE\Valve\Worldcraft\Configured]
                                                 "Installed"=dword:6767bfdf
                                                 "Configured"=dword:00000002

                                                 [HKEY_CURRENT_USER\SOFTWARE\Valve\Worldcraft\Custom2DColors]

                                                 [HKEY_CURRENT_USER\SOFTWARE\Valve\Worldcraft\General]
                                                 "Directory"="C:\\Users\\WDAGUtilityAccount\\Desktop\\Worldcraft 3.3"
                                                 "TextureFileCount"=dword:00000001
                                                 "TextureFile0"="c:\\users\\wdagutilityaccount\\desktop\\zhlt.wad"
                                                 "Brightness"=dword:0000000a
                                                 "Undo Levels"=dword:00000032
                                                 "Locking Textures"=dword:00000000
                                                 "Texture Alignment"=dword:00000000
                                                 "Independent Windows"=dword:00000001
                                                 "Load Default Positions"=dword:00000000
                                                 "GroupWhileIgnore"=dword:00000001
                                                 "StretchArches"=dword:00000000
                                                 "NewBars"=dword:00000001

                                                 [HKEY_CURRENT_USER\SOFTWARE\Valve\Worldcraft\Recent File List]
                                                 "File1"="C:\\Users\\WDAGUtilityAccount\\Desktop\\Worldcraft 3.3\\123"

                                                 [HKEY_CURRENT_USER\SOFTWARE\Valve\Worldcraft\Settings]


                                                 """;
}