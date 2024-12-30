namespace Sledge.Formats.Configuration.Worldcraft
{
    public class Worldcraft2DViewsConfiguration
    {
        /// <summary>
        /// Crosshair cursor
        /// </summary>
        public bool CrosshairCursor { get; set; }

        /// <summary>
        /// Default to 15 degree rotations
        /// </summary>
        public bool DefaultTo15DegreeRotations { get; set; }

        /// <summary>
        /// Display scrollbars
        /// </summary>
        public bool DisplayScrollbars { get; set; }

        /// <summary>
        /// Draw Vertices
        /// </summary>
        public bool DrawVertices { get; set; }

        /// <summary>
        /// White-on-Black color scheme
        /// </summary>
        public bool WhiteOnBlackColorScheme { get; set; }

        /// <summary>
        /// Keep group when clone-dragging
        /// </summary>
        public bool KeepGroupWhenCloneDragging { get; set; }

        /// <summary>
        /// Center on camera after movement in 3D
        /// </summary>
        public bool CenterOnCameraAfterMovement { get; set; }

        /// <summary>
        /// Use Visgroup colors for object lines
        /// </summary>
        public bool UseVisgroupColorsForObjectLines { get; set; }

        /// <summary>
        /// Arrow keys nudge selected object/vertex
        /// </summary>
        public bool ArrowKeysNudgeSelectedObject { get; set; }

        /// <summary>
        /// Reorient primitives on creation in the active 2D view
        /// </summary>
        public bool ReorientPrimitivesOnCreation { get; set; }

        /// <summary>
        /// Automatic infinite selection in 2D windows (no ENTER)
        /// </summary>
        public bool AutomaticInfiniteSelection { get; set; }

        /// <summary>
        /// Selection box selects by center handles only
        /// </summary>
        public bool SelectionBoxSelectsByCenterHandlesOnly { get; set; }

        /// <summary>
        /// Grid configuration
        /// </summary>
        public WorldcraftGridOptions Grid { get; } = new WorldcraftGridOptions();
    }
}