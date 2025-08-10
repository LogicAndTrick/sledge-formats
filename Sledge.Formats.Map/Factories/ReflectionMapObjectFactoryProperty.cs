using System.ComponentModel;
using System.Reflection;

namespace Sledge.Formats.Map.Factories
{
    /// <summary>
    /// A <see cref="MapObjectFactoryProperty"/> with a <see cref="PropertyInfo"/> instance for get/set.
    /// </summary>
    public class ReflectionMapObjectFactoryProperty : MapObjectFactoryProperty
    {
        /// <summary>
        /// The reflection property
        /// </summary>
        public PropertyInfo Property { get; }

        public ReflectionMapObjectFactoryProperty(PropertyInfo property)
        {
            Property = property;
            Name = property.Name;
            DisplayName = property.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName ?? property.Name;
            Description = property.GetCustomAttribute<DescriptionAttribute>(true)?.Description ?? "";
            Type = property.PropertyType;
            var pd = property.GetCustomAttribute<MapObjectFactoryPropertyDataAttribute>(true);
            if (pd != null)
            {
                MinValue = (decimal) pd.MinValue;
                MaxValue = (decimal) pd.MaxValue;
                DecimalPrecision = pd.DecimalPrecision;
            }
        }
    }
}