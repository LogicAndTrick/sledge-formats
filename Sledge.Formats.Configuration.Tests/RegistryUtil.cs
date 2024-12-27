using System.Diagnostics;
using Microsoft.Win32;
using Sledge.Formats.Configuration.Registry;

namespace Sledge.Formats.Configuration.Tests;

#pragma warning disable CA1416 // platform warning
public static class RegistryUtil
{
    /// <summary>
    /// Very rough .reg file parser
    /// </summary>
    public static InMemoryRegistry CreateRegistryFromRegString(string regString)
    {
        var reg = new InMemoryRegistry();

        var spl = regString.Split('\n').Select(x => x.Trim()).ToList();

        if (spl[0] != "Windows Registry Editor Version 5.00") throw new NotSupportedException($"Unknown registry type: {spl[0]}");

        var currentHive = RegistryHive.CurrentUser;
        string? currentKey = null;

        foreach (var line in spl.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line[0] == '[')
            {
                var keyPath = line.Trim('[', ']').Split('\\');
                var hiveStr = keyPath[0][4..].Replace("_", "");
                currentHive = Enum.Parse<RegistryHive>(hiveStr, true);
                currentKey = string.Join('\\', keyPath.Skip(1));
            }
            else if (currentKey != null)
            {
                var kv = line.Split('=', 2);
                var key = kv[0].Trim('"');
                var val = kv[1];
                if (val.StartsWith('"'))
                {
                    reg.OpenBaseKey(currentHive, RegistryView.Default).CreateSubKey(currentKey).SetValue(key, val.Trim('"').Replace(@"\\", @"\"), RegistryValueKind.String);
                }
                else if (val.StartsWith("dword:"))
                {
                    reg.OpenBaseKey(currentHive, RegistryView.Default).CreateSubKey(currentKey).SetValue(key, Convert.ToInt32(val[6..], 16), RegistryValueKind.DWord);
                }
                else
                {
                    throw new NotSupportedException($"Unknown value type: {val}");
                }
            }
        }

        return reg;
    }
}