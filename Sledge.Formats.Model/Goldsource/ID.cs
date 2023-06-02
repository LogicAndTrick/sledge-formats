namespace Sledge.Formats.Model.Goldsource
{
    public enum ID : uint
    {
        Idst = (byte)'I' | (byte)'D' << 8 | (byte)'S' << 16 | (byte)'T' << 24,
        Idsq = (byte)'I' | (byte)'D' << 8 | (byte)'S' << 16 | (byte)'Q' << 24
    }
}