using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Numerics;

namespace Sledge.Formats.Map.Objects
{
    public class Entity : MapObject
    {
        public string ClassName { get; set; }
        public int SpawnFlags { get; set; }
        public List<KeyValuePair<string, string>> SortedProperties { get; }
        public IDictionary<string, string> Properties { get; }

        public Entity()
        {
            SortedProperties = new List<KeyValuePair<string, string>>();
            Properties = new SortedKeyValueDictionaryWrapper(SortedProperties);
        }

        public int GetIntProperty(string key, int defaultValue) => GetProperty(key, defaultValue);
        public float GetFloatProperty(string key, float defaultValue) => GetProperty(key, defaultValue);
        public decimal GetDecimalProperty(string key, decimal defaultValue) => GetProperty(key, defaultValue);
        public string GetStringProperty(string key, string defaultValue) => GetProperty(key, defaultValue);

        public Vector3 GetVectorProperty(string key, Vector3 defaultValue)
        {
            if (!Properties.ContainsKey(key)) return defaultValue;

            var value = Properties[key];
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            var spl = value.Split(' ');
            if (spl.Length != 3) return defaultValue;

            if (!float.TryParse(spl[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) return defaultValue;
            if (!float.TryParse(spl[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) return defaultValue;
            if (!float.TryParse(spl[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z)) return defaultValue;

            return new Vector3(x, y, z);
        }

        public Vector4 GetVector4Property(string key, Vector4 defaultValue)
        {
            if (!Properties.ContainsKey(key)) return defaultValue;

            var value = Properties[key];
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            var spl = value.Split(' ');
            if (spl.Length != 4) return defaultValue;

            if (!float.TryParse(spl[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) return defaultValue;
            if (!float.TryParse(spl[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) return defaultValue;
            if (!float.TryParse(spl[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z)) return defaultValue;
            if (!float.TryParse(spl[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var w)) return defaultValue;

            return new Vector4(x, y, z, w);
        }

        public Color GetColorProperty(string key, Color defaultValue)
        {
            var vv = GetVector4Property(key, new Vector4(defaultValue.R, defaultValue.G, defaultValue.B, defaultValue.A));
            return Color.FromArgb((int)vv.W, (int)vv.X, (int)vv.Y, (int)vv.Z);
        }

        public T GetProperty<T>(string key, T defaultValue)
        {
            if (!Properties.ContainsKey(key)) return defaultValue;

            var value = Properties[key];
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}