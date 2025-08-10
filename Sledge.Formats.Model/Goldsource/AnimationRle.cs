using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sledge.Formats.Model.Goldsource
{
    public class AnimationRle
    {
        public byte CompressedLength { get; set; }
        public byte UncompressedLength { get; set; }
        public short[] Values { get; set; }

        public AnimationRle(byte compressedLength, byte uncompressedLength, IEnumerable<short> values)
        {
            CompressedLength = compressedLength;
            UncompressedLength = uncompressedLength;
            Values = values.ToArray();
        }

        public void WriteTo(BinaryWriter bw)
        {
            bw.Write(CompressedLength);
            bw.Write(UncompressedLength);
            bw.WriteShortArray(Values.Length, Values);
        }

        public static IEnumerable<AnimationRle> Compress(short[] values)
        {
            /*
             * RLE data:
             * byte compressed_length - compressed number of values in the data
             * byte uncompressed_length - uncompressed number of values in run
             * short values[compressed_length] - values in the run, the last value is repeated to reach the uncompressed length
             */
            if (values.Length == 0 || values.All(x => x == 0)) yield break;

            byte compLen = 1;
            byte totalLen = 1;
            var start = 0;

            for (var i = 1; i < values.Length; i++)
            {
                // run can't be longer than 255 (max byte), start a new run
                if (totalLen == byte.MaxValue)
                {
                    yield return new AnimationRle(compLen, totalLen, new ArraySegment<short>(values, start, totalLen));
                    compLen = totalLen = 1;
                    start = i;
                    continue;
                }

                var prev = values[i - 1];
                var curr = values[i];
                var repeating = compLen != totalLen;

                if (repeating)
                {
                    // start a new run if we're currently repeating and the prev value was different
                    if (prev != curr)
                    {
                        yield return new AnimationRle(compLen, totalLen, new ArraySegment<short>(values, start, totalLen));
                        compLen = totalLen = 1;
                        start = i;
                    }
                    else
                    {
                        // otherwise we are still repeating so just increase by 1
                        totalLen++;
                    }
                }
                else
                {
                    // start repeating if this value matches the previous and next values
                    if (prev == curr && i < values.Length - 1 && values[i + 1] == curr)
                    {
                        totalLen++;
                    }
                    // otherwise we can't repeat this value
                    else
                    {
                        compLen++;
                        totalLen++;
                    }
                }
            }

            // return the remaining run if needed
            if (start < values.Length)
            {
                yield return new AnimationRle(compLen, totalLen, new ArraySegment<short>(values, start, totalLen));
            }
        }
    }
}