namespace Sledge.Formats.GameData.Objects
{
    public enum ClassType
    {
        Any,

        BaseClass,
        PointClass,
        SolidClass,

        FilterClass, // Source
        KeyFrameClass, // Source
        MoveClass, // Source
        NPCClass, // Source

        PathClass, // Source 2
        PathNodeClass, // Source 2
        CableClass, // Source 2
        OverrideClass, // Source 2
        ModelGameData, // Source 2
        ModelAnimEvent, // Source 2
        ModelBreakCommand, // Source 2
        Struct, // Source 2
        VData, // Source 2
        VDataDerived, // Source 2
    }
}
