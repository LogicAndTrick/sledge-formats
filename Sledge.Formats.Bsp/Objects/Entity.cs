using System;
using System.Collections.Generic;
using System.Numerics;

namespace Sledge.Formats.Bsp.Objects
{
    public class Entity
    {
        public int Model
        {
            get
            {
                var val = Get("model", "");
                return val.Length > 0 && int.TryParse(val.Substring(1), out var m) ? m : 0;
            }
            set => Set("model", value == 0 ? "" : $"*{value}");
        }

        public string ClassName
        {
            get => Get("classname", "");
            set => Set("classname", value);
        }

        public List<KeyValuePair<string, string>> SortedKeyValues { get; }
        public IDictionary<string, string> KeyValues { get; }

        public Entity()
        {
            SortedKeyValues = new List<KeyValuePair<string, string>>();
            KeyValues = new SortedKeyValueDictionaryWrapper(SortedKeyValues);
        }

        private KeyValuePair<string, string>? GetKeyValue(string key)
        {
            foreach (var kv in SortedKeyValues)
            {
                if (kv.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)) return kv;
            }

            return null;
        }

        public Vector3 GetVector3(string name, Vector3 defaultValue)
        {
            var kv = GetKeyValue(name);
            if (!kv.HasValue) return defaultValue;

            var val = kv.Value.Value;
            var spl = val.Split(' ');
            if (spl.Length != 3) return defaultValue;

            if (!float.TryParse(spl[0], out var x)) return defaultValue;
            if (!float.TryParse(spl[1], out var y)) return defaultValue;
            if (!float.TryParse(spl[2], out var z)) return defaultValue;

            return new Vector3(x, y, z);
        }

        public T Get<T>(string name, T defaultValue)
        {
            var kv = GetKeyValue(name);
            if (!kv.HasValue) return defaultValue;
            var val = kv.Value.Value;
            try
            {
                return (T) Convert.ChangeType(val, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public void Set<T>(string name, T value)
        {
            Unset(name);
            SortedKeyValues.Add(new KeyValuePair<string, string>(name, Convert.ToString(value)));
        }

        public void Unset(string name)
        {
            SortedKeyValues.RemoveAll(x => x.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}