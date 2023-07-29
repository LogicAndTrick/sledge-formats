using System;

namespace Sledge.Formats.Bsp.Objects
{
    [Flags]
    public enum TextureFlags : int
    {
        None = 0,
        NoLightmaps = 1,
    }
}