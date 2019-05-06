namespace Sledge.Formats.Bsp
{
    internal enum Magic : uint
    {
        Vbsp = ('P' << 24) + ('S' << 16) + ('B' << 8) + 'V',
        Ibsp = ('P' << 24) + ('S' << 16) + ('B' << 8) + 'I',
    }
}