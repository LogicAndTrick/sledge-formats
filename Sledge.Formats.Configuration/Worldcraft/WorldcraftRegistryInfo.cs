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

        public const string Key2DViewsCrosshairs = "Crosshairs";
        public const string Key2DViewsGroupCarve = "GroupCarve";
        public const string Key2DViewsScrollbars = "Scrollbars";
        public const string Key2DViewsRotateConstrain = "RotateConstrain";
        public const string Key2DViewsDrawVertices = "Draw Vertices";
        public const string Key2DViewsDefaultGrid = "Default Grid";
        public const string Key2DViewsWhiteOnBlack = "WhiteOnBlack";
        public const string Key2DViewsGridHigh10 = "GridHigh10";
        public const string Key2DViewsGridHigh1024 = "GridHigh1024";
        public const string Key2DViewsGridIntensity = "GridIntensity";
        public const string Key2DViewsHideSmallGrid = "HideSmallGrid";
        public const string Key2DViewsNudge = "Nudge";
        public const string Key2DViewsOrientPrimitives = "OrientPrimitives";
        public const string Key2DViewsAutoSelect = "AutoSelect";
        public const string Key2DViewsSelectByHandles = "SelectByHandles";
        public const string Key2DViewsGridHighSpec = "GridHighSpec";
        public const string Key2DViewsKeepCloneGroup = "KeepCloneGroup";
        public const string Key2DViewsGridHigh64 = "Gridhigh64";
        public const string Key2DViewsGridDots = "GridDots";
        public const string Key2DViewsCenterOnCamera = "Centeroncamera";
        public const string Key2DViewsUseGroupColors = "Usegroupcolors";

        public const string Key3DViewsHardware = "Hardware";
        public const string Key3DViewsReverseY = "Reverse Y";
        public const string Key3DViewsBackPlane = "BackPlane";
        public const string Key3DViewsUseMouseLook = "UseMouseLook";
        public const string Key3DViewsModelDistance = "ModelDistance";
        public const string Key3DViewsAnimateModels = "AnimateModels";
        public const string Key3DViewsForwardSpeedMax = "ForwardSpeedMax";
        public const string Key3DViewsTimeToMaxSpeed = "TimeToMaxSpeed";
        public const string Key3DViewsFilterTextures = "FilterTextures";
        public const string Key3DViewsReverseSelection = "ReverseSelection";
        public const string Key3DViewsClearColor = "ClearColor";

        public const string KeyRecentFileListPrefix = "File";
    }
}