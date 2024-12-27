using Microsoft.Win32;
using Sledge.Formats.Configuration.Worldcraft;

namespace Sledge.Formats.Configuration.Tests;

#pragma warning disable CA1416

[TestClass]
public sealed class TestWorldcraftConfiguration
{
    /// <summary>
    /// If you don't have worldcraft settings in your registry, this test will fail
    /// </summary>
    [TestMethod]
    public void TestLoadSettingsFromLocalComputer()
    {
        var config = WorldcraftConfiguration.LoadFromRegistry(WorldcraftConfigurationLoadSettings.Default);
        Console.WriteLine("Undo levels: " + config.General.UndoLevels);
        Console.WriteLine("Textures: " + string.Join("; ", config.TextureDirectories));
    }

    [TestMethod]
    public void TestLoadSettings()
    {
        var reg = RegistryUtil.CreateRegistryFromRegString(Worldcraft33RegString);
        var config = WorldcraftConfiguration.LoadFromRegistry(new WorldcraftConfigurationLoadSettings
        {
            LoadGameConfigurations = false,
            AutodetectRegistryLocation = false,
            RegistryLocation = reg.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"Software\Valve\Worldcraft")
        });

        Assert.AreEqual(@"C:\Users\WDAGUtilityAccount\Desktop\Worldcraft 3.3", config.General.InstallDirectory, StringComparer.InvariantCultureIgnoreCase);
        Assert.AreEqual(true, config.General.UseIndependentWindowConfigurations);
        Assert.AreEqual(false, config.General.LoadDefaultWindowPositionsWithMaps);
        Assert.AreEqual(0x32, config.General.UndoLevels);
        Assert.AreEqual(true, config.General.AllowGroupingWhileIgnoreGroupsChecked);
        Assert.AreEqual(false, config.General.StretchArchesToFitOriginalBoundingRectangle);

        CollectionAssert.AreEqual(new[] { @"c:\users\wdagutilityaccount\desktop\zhlt.wad" }, config.TextureDirectories, StringComparer.InvariantCultureIgnoreCase);
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