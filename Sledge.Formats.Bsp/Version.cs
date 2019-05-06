namespace Sledge.Formats.Bsp
{
    public enum Version : ulong
    {
        // No magic
        Quake1           = 29,
        Goldsource       = 30,
        Bsp2             = ('2' << 24) + ('P' << 16) + ('S' << 8) + 'B',
        Bsp2Rmqe         = ('P' << 24) + ('S' << 16) + ('B' << 8) + '2',

        // IBSP
        Quake2           = (Magic.Ibsp << 32) + 38L,
        Quake3           = (Magic.Ibsp << 32) + 46L,

        // VBSP
        Source17         = (17L << 32) + Magic.Vbsp,
        Source18         = (18L << 32) + Magic.Vbsp,
        Source2004       = (19L << 32) + Magic.Vbsp,
        Source2007       = (20L << 32) + Magic.Vbsp,
        Source2013       = (21L << 32) + Magic.Vbsp,
    }
}