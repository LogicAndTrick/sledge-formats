using System;

namespace Sledge.Formats.Map.Factories
{
    /// <summary>
    /// A class describing a property of a <see cref="IMapObjectFactory"/>
    /// </summary>
    public class MapObjectFactoryProperty
    {
        /// <summary>
        /// The name of the property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The display name of the property
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// A description of the property
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The type of the property
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// For decimal properties, the minimum allowed value. Will default to <see cref="int.MinValue"/>.
        /// </summary>
        public decimal MinValue { get; set; } = int.MinValue;

        /// <summary>
        /// For decimal properties, the maximum allowed value. Will default to <see cref="int.MaxValue"/>.
        /// </summary>
        public decimal MaxValue { get; set; } = int.MaxValue;

        /// <summary>
        /// For decimal properties, the number of decimal places allowed.
        /// </summary>
        public int DecimalPrecision { get; set; }
    }
}