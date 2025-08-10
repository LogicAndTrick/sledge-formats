namespace Sledge.Formats.Model.Goldsource
{
    /// <summary>
    /// The type of file
    /// </summary>
    public enum MdlWriteOutputType
    {
        /// <summary>
        /// The base file, may contain texture and animation data
        /// </summary>
        Base,

        /// <summary>
        /// The external texture file
        /// </summary>
        Texture,

        /// <summary>
        /// A numbered external sequences file
        /// </summary>
        Sequence,
    }
}