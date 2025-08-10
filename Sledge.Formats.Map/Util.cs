using System;
using System.Globalization;
using System.Linq;

namespace Sledge.Formats.Map
{
    internal static class Util
    {
        public static void Assert(bool b, string message = "Malformed file.")
        {
            if (!b) throw new Exception(message);
        }

        public static bool ParseFloatArray(string input, char[] splitChars, int expected, out float[] array)
        {
            var spl = input.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            if (spl.Length == expected)
            {
                var parsed = spl.Select(x => float.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out var o) ? (float?)o : null).ToList();
                if (parsed.All(x => x.HasValue))
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    array = parsed.Select(x => x.Value).ToArray();
                    return true;
                }
            }
            array = new float[expected];
            return false;
        }

        public static bool ParseDoubleArray(string input, char[] splitChars, int expected, out double[] array)
        {
            var spl = input.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            if (spl.Length == expected)
            {
                var parsed = spl.Select(x => double.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out var o) ? (double?)o : null).ToList();
                if (parsed.All(x => x.HasValue))
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    array = parsed.Select(x => x.Value).ToArray();
                    return true;
                }
            }
            array = new double[expected];
            return false;
        }
    }
}
