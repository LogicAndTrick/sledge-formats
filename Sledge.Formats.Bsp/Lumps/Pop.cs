using System.IO;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Pop : ILump
    {
        public byte[] Data { get; set; }

        public Pop()
        {
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            // Unused ...?
            Data = br.ReadBytes(blob.Length);
        }

        public void PostReadProcess(BspFile bsp)
        {
            
        }

        public void PreWriteProcess(BspFile bsp, Version version)
        {
            
        }

        public int Write(BinaryWriter bw, Version version)
        {
            bw.Write((byte[]) Data);
            return Data.Length;
        }
    }
}