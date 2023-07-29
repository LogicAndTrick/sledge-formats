using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Texinfo : ILump, IList<TextureInfo>
    {
        private readonly IList<TextureInfo> _textureInfos;

        public Texinfo()
        {
            _textureInfos = new List<TextureInfo>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                var info = new TextureInfo
                {
                    S = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                    T = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                };
                switch (version)
                {
                    case Version.Goldsource:
                    case Version.Quake1:
                        info.MipTexture = br.ReadInt32();
                        break;
                }
                info.Flags = (TextureFlags) br.ReadInt32();
                switch (version)
                {
                    case Version.Quake2:
                        info.Value = br.ReadInt32();
                        info.TextureName = br.ReadFixedLengthString(Encoding.ASCII, 32);
                        info.NextTextureInfo = br.ReadInt32();
                        break;
                }
                _textureInfos.Add(info);
            }
        }

        public void PostReadProcess(BspFile bsp)
        {
            
        }

        public void PreWriteProcess(BspFile bsp, Version version)
        {
            
        }

        public int Write(BinaryWriter bw, Version version)
        {
            var pos = bw.BaseStream.Position;
            foreach (var ti in _textureInfos)
            {
                bw.Write((float) ti.S.X);
                bw.Write((float) ti.S.Y);
                bw.Write((float) ti.S.Z);
                bw.Write((float) ti.S.W);

                bw.Write((float) ti.T.X);
                bw.Write((float) ti.T.Y);
                bw.Write((float) ti.T.Z);
                bw.Write((float) ti.T.W);

                switch (version)
                {
                    case Version.Goldsource:
                    case Version.Quake1:
                        bw.Write((int) ti.MipTexture);
                        break;
                }


                bw.Write((int) ti.Flags);

                switch (version)
                {
                    case Version.Quake2:
                        bw.Write((int) ti.Value);
                        bw.WriteFixedLengthString(Encoding.ASCII, 32, ti.TextureName);
                        bw.Write((int) ti.NextTextureInfo);
                        break;
                }
            }
            return (int)(bw.BaseStream.Position - pos);
        }

        #region IList

        public IEnumerator<TextureInfo> GetEnumerator()
        {
            return _textureInfos.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _textureInfos).GetEnumerator();
        }

        public void Add(TextureInfo item)
        {
            _textureInfos.Add(item);
        }

        public void Clear()
        {
            _textureInfos.Clear();
        }

        public bool Contains(TextureInfo item)
        {
            return _textureInfos.Contains(item);
        }

        public void CopyTo(TextureInfo[] array, int arrayIndex)
        {
            _textureInfos.CopyTo(array, arrayIndex);
        }

        public bool Remove(TextureInfo item)
        {
            return _textureInfos.Remove(item);
        }

        public int Count => _textureInfos.Count;

        public bool IsReadOnly => _textureInfos.IsReadOnly;

        public int IndexOf(TextureInfo item)
        {
            return _textureInfos.IndexOf(item);
        }

        public void Insert(int index, TextureInfo item)
        {
            _textureInfos.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _textureInfos.RemoveAt(index);
        }

        public TextureInfo this[int index]
        {
            get => _textureInfos[index];
            set => _textureInfos[index] = value;
        }

        #endregion
    }
}