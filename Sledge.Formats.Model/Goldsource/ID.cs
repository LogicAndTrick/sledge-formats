namespace Sledge.Formats.Model.Goldsource
{
    public enum ID : uint
    {
        /// <summary>
        /// Studio model base file or texture file
        /// </summary>
        Idst = (byte)'I' | (byte)'D' << 8 | (byte)'S' << 16 | (byte)'T' << 24,

        /// <summary>
        /// Studio model sequence file
        /// </summary>
        Idsq = (byte)'I' | (byte)'D' << 8 | (byte)'S' << 16 | (byte)'Q' << 24
    }
}