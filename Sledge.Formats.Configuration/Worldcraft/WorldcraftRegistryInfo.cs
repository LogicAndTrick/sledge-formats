namespace Sledge.Formats.Configuration.Worldcraft
{
    public static class WorldcraftRegistryInfo
    {
        /// <summary>
        /// The default registry paths where Worldcraft stores its settings, in descending order by version
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly string[] DefaultRegistryPaths = {
            @"Software\Valve\Valve Hammer Editor",
            @"Software\Valve\Worldcraft",
            @"Software\Worldcraft\Worldcraft"
        };

        public const string Key2DViews = "2D Views";
        public const string Key3DViews = "3D Views";
        public const string KeyGeneral = "General";
        public const string KeyRecentFiles = "Recent File List";

        public const string KeyGeneralDirectory = "Directory";
        public const string KeyGeneralBrightness = "Brightness";
        public const string KeyGeneralGroupWhileIgnore = "GroupWhileIgnore";
        public const string KeyGeneralIndependentWindows = "Independent Windows";
        public const string KeyGeneralLoadDefaultPositions = "Load Default Positions";
        public const string KeyGeneralLockingTextures = "Locking Textures";
        public const string KeyGeneralNewBars = "NewBars";
        public const string KeyGeneralStretchArches = "StretchArches";
        public const string KeyGeneralTextureAlignment = "Texture Alignment";
        public const string KeyGeneralTextureFilePrefix = "TextureFile";
        public const string KeyGeneralTextureFileCount = "TextureFileCount";
        public const string KeyGeneralUndoLevels = "Undo Levels";
    }
}