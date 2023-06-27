namespace Sledge.Formats.Bsp.Objects
{
    public enum Contents : int
    {
        Empty = -1,
        Solid = -2,
        Water = -3,
        Slime = -4,
        Lava = -5,
        Sky = -6,
        Origin = -7,
        Clip = -8,
        Current0 = -9,
        Current90 = -10,
        Current180 = -11,
        Current270 = -12,
        CurrentUp = -13,
        CurrentDown = -14,
        Translucent = -15,
        Hint = -16,
        Null = -17,
        Detail = -18,
        BoundingBox = -19,
        ToEmpty = -32,
    }
}