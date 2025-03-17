using System;

namespace Sledge.Formats.Configuration.Worldcraft
{
    public enum MapType
    {
        HalfLife = 3,
        [Obsolete] Quake = 0,
        [Obsolete] Quake2 = 2,
        [Obsolete] Hexen2 = 1,
    }
}