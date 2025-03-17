using Sledge.Formats.Valve;
using System;
using System.IO;
using System.Text;

namespace Sledge.Formats.Id
{
    public class MipTexture
    {
        public string Name { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public int NumMips { get; set; }
        public byte[][] MipData { get; set; }
        public byte[] Palette { get; set; }

        const int NameLength = 16;

        /// <summary>
        /// Create an empty <see cref="MipTexture"/>
        /// </summary>
        public MipTexture()
        {
            //
        }

        /// <summary>
        /// Create a <see cref="MipTexture"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="readPalette">True to read the palette (i.e. Goldsource formats: BSP30 / WAD3)</param>
        public MipTexture(Stream stream, bool readPalette)
        {
            using (var br = new BinaryReader(stream))
            {
                var mt = Read(br, readPalette);
                Name = mt.Name;
                Width = mt.Width;
                Height = mt.Height;
                NumMips = mt.NumMips;
                MipData = mt.MipData;
                Palette = mt.Palette;
            }
        }

        /// <summary>
        /// Write this <see cref="MipTexture"/> to the provided stream.
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        /// <param name="writePalette">True to write the palette (i.e. Goldsource formats: BSP30 / WAD3)</param>
        public void WriteTo(Stream stream, bool writePalette)
        {
            using (var bw = new BinaryWriter(stream))
            {
                Write(bw, writePalette, this);
            }
        }

        public static MipTexture Read(BinaryReader br, bool readPalette)
        {
            var position = br.BaseStream.Position;

            var texture = new MipTexture();

            var name = br.ReadChars(NameLength);
            var len = Array.IndexOf(name, '\0');
            texture.Name = new string(name, 0, len < 0 ? name.Length : len);

            texture.Width = br.ReadUInt32();
            texture.Height = br.ReadUInt32();
            var offsets = new[] { br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32() };

            if (offsets[0] == 0)
            {
                texture.NumMips = 0;
                texture.MipData = Array.Empty<byte[]>();
                texture.Palette = readPalette ? QuakePalette.Data : Array.Empty<byte>();
                return texture;
            }

            texture.NumMips = 4;
            texture.MipData = new byte[4][];

            int w = (int)texture.Width, h = (int)texture.Height;
            for (var i = 0; i < 4; i++)
            {
                br.BaseStream.Seek(position + offsets[i], SeekOrigin.Begin);
                texture.MipData[i] = br.ReadBytes(w * h);
                w /= 2;
                h /= 2;
            }

            if (readPalette)
            {
                var paletteSize = br.ReadUInt16();
                texture.Palette = br.ReadBytes(paletteSize * 3);
            }
            else
            {
                texture.Palette = QuakePalette.Data;
            }

            return texture;
        }

        public static void Write(BinaryWriter bw, bool writePalette, MipTexture texture)
        {
            bw.WriteFixedLengthString(Encoding.ASCII, NameLength, texture.Name);
            bw.Write((uint) texture.Width);
            bw.Write((uint) texture.Height);

            if (texture.NumMips == 0)
            {
                bw.Write((uint) 0);
                bw.Write((uint) 0);
                bw.Write((uint) 0);
                bw.Write((uint) 0);
                bw.Write((ushort) 0);
                return;
            }

            uint currentOffset = NameLength + sizeof(uint) * 2 + sizeof(uint) * 4;

            for (var i = 0; i < 4; i++)
            {
                bw.Write((uint) currentOffset);
                currentOffset += (uint) texture.MipData[i].Length;
            }

            for (var i = 0; i < 4; i++)
            {
                bw.Write((byte[]) texture.MipData[i]);
            }

            if (writePalette)
            {
                bw.Write((ushort) (texture.Palette.Length / 3));
                bw.Write((byte[]) texture.Palette);
            }
        }
    }
}