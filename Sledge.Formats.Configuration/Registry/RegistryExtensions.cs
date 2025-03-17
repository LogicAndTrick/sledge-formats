using Sledge.Formats.Configuration.Worldcraft;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sledge.Formats.Configuration.Registry
{
    public static class RegistryExtensions
    {
        public static int GetIntValue(this IRegistryKey key, string name)
        {
            return key.GetValue(name, 0) as int? ?? 0;
        }

        public static bool GetBoolValue(this IRegistryKey key, string name)
        {
            return key.GetValue(name, 0) as int? == 1;
        }

        public static string GetStringValue(this IRegistryKey key, string name)
        {
            return key.GetValue(name, 0) as string;
        }
    }
}
