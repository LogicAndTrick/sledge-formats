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
            set => KeyValues["model"] = value == 0 ? "" : $"*{value}";
        }

        public string ClassName
        {
            get => Get("classname", "");
            set => KeyValues["classname"] = value;
        }

        public Dictionary<string, string> KeyValues { get; }

        public Entity()
        {
            KeyValues = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public Vector3 GetVector3(string name, Vector3 defaultValue)
        {
            if (!KeyValues.ContainsKey(name)) return defaultValue;

            var val = KeyValues[name];
            var spl = val.Split(' ');
            if (spl.Length != 3) return defaultValue;

            if (!float.TryParse(spl[0], out var x)) return defaultValue;
            if (!float.TryParse(spl[1], out var y)) return defaultValue;
            if (!float.TryParse(spl[2], out var z)) return defaultValue;

            return new Vector3(x, y, z);
        }

        public T Get<T>(string name, T defaultValue)
        {
            if (!KeyValues.ContainsKey(name)) return defaultValue;
            var val = KeyValues[name];
            try
            {
                return (T) Convert.ChangeType(val, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}