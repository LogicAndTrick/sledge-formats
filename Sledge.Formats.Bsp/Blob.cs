namespace Sledge.Formats.Bsp
{
    /// <summary>
    /// An unprocessed lump
    /// </summary>
    public struct Blob
    {
        public int Index;
        public int Offset;
        public int Length;
        public uint Version;
        public string Ident;
    }
}