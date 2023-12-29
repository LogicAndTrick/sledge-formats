using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Visibility : ILump
    {
        public byte[] VisData { get; private set; }

        public Visibility()
        {
            VisData = Array.Empty<byte>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            VisData = br.ReadBytes(blob.Length);
        }

        public void PostReadProcess(BspFile bsp)
        {
            //throw new NotImplementedException("Visibility data must be post-processed");
        }

        public void PreWriteProcess(BspFile bsp, Version version)
        {
            // throw new NotImplementedException("Visibility data must be pre-processed");
        }

        public int Write(BinaryWriter bw, Version version)
        {
            bw.Write(VisData);
            return VisData.Length;
        }

        public bool IsVisible(Leaves lump, int self, int other)
        {
            var leaf = lump[self];
            return GetVisibleLeaves(lump.Count, leaf).Contains(other);
        }

        public IEnumerable<int> GetVisibleLeaves(int numLeaves, Leaf leaf)
        {
            var offset = leaf.VisOffset;
            for (var i = 1; i < numLeaves; offset++)
            {
                if (VisData[offset] == 0)
                {
                    // not visible, next byte is the number of leaves to skip
                    offset++;
                    i += 8 * VisData[offset];
                }
                else
                {
                    // at least 1 of the next 8 leaves is visible, look through each bit to find which one
                    for (byte bit = 1; bit != 0; bit <<= 1, i++)
                    {
                        if ((VisData[offset] & bit) != 0)
                        {
                            yield return i;
                        }
                    }
                }
            }
        }
    }
}