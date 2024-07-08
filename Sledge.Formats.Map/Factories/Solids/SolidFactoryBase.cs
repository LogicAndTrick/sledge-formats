using System.ComponentModel;

namespace Sledge.Formats.Map.Factories.Solids
{
    public abstract class SolidFactoryBase : MapObjectFactoryBase, ISolidFactory
    {
        /// <inheritdoc cref="ISolidFactory.NullTextureName"/>
        [DisplayName("Null texture")]
        [Description("The texture name to apply to hidden faces")]
        public string NullTextureName { get; set; } = "NULL";

        /// <inheritdoc cref="ISolidFactory.VisibleTextureName"/>
        [DisplayName("Visible texture")]
        [Description("The texture name to apply to exposed faces")]
        public string VisibleTextureName { get; set; } = "AAATRIGGER";

        /// <inheritdoc cref="ISolidFactory.RoundDecimals"/>
        [DisplayName("Round decimals")]
        [Description("The number of decimals to round vertex positions to")]
        public int RoundDecimals { get; set; }
    }
}