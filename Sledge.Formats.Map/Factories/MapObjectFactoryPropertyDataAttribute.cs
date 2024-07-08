using System;

namespace Sledge.Formats.Map.Factories
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MapObjectFactoryPropertyDataAttribute : System.Attribute
    {
        public double MinValue { get; set; } = int.MinValue;
        public double MaxValue { get; set; } = int.MaxValue;
        public int DecimalPrecision { get; set; } = 2;
    }
}