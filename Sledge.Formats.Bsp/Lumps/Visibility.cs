using System;
using System.IO;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Visibility : ILump
    {
        public byte[] VisData { get; private set; }

        public Visibility()
        {

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
    }
}