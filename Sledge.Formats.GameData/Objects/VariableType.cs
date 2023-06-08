namespace Sledge.Formats.GameData.Objects
{
    public enum VariableType
    {
        Axis,
        Angle,
        AngleNegativePitch,
        Array,
        BodyGroupChoices, // Source 2
        Bool,
        Boolean = Bool,
        Choices,
        Color255,
        Color1,
        Curve, // Source 2 (alyx)
        Decal,
        FilterClass,
        Flags,
        Float,
        GameItemClass, // Source 2 (dota)
        GameUnitClass, // Source 2 (dota)
        InstanceFile,
        InstanceVariable,
        InstanceParm,
        Int, // Source 2
        Integer,
        LocalAxis, // Source 2
        LocalPoint, // Source 2
        LodLevel, // Source 2
        Material,
        MaterialGroup, // Source 2
        ModelStateChoices, // Source 2 (alyx)
        ModelAttachment, // Source 2
        ModelBreakPiece, // Source 2
        NodeDest,
        NodeId,
        NodeIdList, // Source 2
        NPCClass,
        Origin,
        Other,
        ParentAttachment, // Source 2
        ParticleSystem,
        PointEntityClass,
        RemoveKey, // Source 2 (alyx)
        Resource, // Source 2
        ResourceChoices, // Source 2
        Scale, // Quake 3
        Scene,
        Script,
        ScriptList,
        Sequence, // Source 2
        SideList,
        Sky, // Quake 2
        Sound,
        Sprite,
        String,
        Studio,
        SurfaceProperties, // Source 2
        TagList, // Source 2 (alyx)
        TagListDynamic, // Source 2 (alyx)
        TargetDestination,
        TargetNameOrClass,
        TargetSource,
        TextBlock,
        Vecline,
        Vector,
        Void,
        WorldPoint, // Source 2

        PropDataName, // Source 2
        ModelBodyGroup, // Source 2
        ModelClothEffect, // Source 2
        ModelBone, // Source 2
        ModelMorphChannel, // Source 2
        VDataChoice, // Source 2
        SubclassChoice, // Source 2
        NPCAbilityName, // Source 2
        PathNodeClass, // Source 2
        Color255Alpha, // Source 2
        ParticleCfg, // Source 2
        ModelClothVertexMap, // Source 2
        Kv3, // Source 2
        Vector2d, // Source 2
        PanoramaImage, // Source 2
        Struct, // Source 2
        AnimGraph, // Source 2 (cs2)
        AnimGraphEnum, // Source 2 (cs2)
    }
}
