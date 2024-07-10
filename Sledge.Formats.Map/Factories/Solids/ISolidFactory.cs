using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Factories.Solids
{
    /// <summary>
    /// A variant of a <see cref="IMapObjectFactory"/> that is focused on creating <see cref="Solid"/> instances.
    /// </summary>
    public interface ISolidFactory : IMapObjectFactory
    {
        /// <summary>
        /// The texture name to apply to hidden faces
        /// </summary>
        string NullTextureName { get; set; }

        /// <summary>
        /// The texture name to apply to exposed faces
        /// </summary>
        string VisibleTextureName { get; set; }

        /// <summary>
        /// The number of decimals to round vertex positions to
        /// </summary>
        int RoundDecimals { get; set; }
    }
}