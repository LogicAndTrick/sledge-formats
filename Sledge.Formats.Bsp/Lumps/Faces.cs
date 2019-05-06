using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Faces : ILump, IList<Face>
    {
        private readonly IList<Face> _faces;

        public Faces()
        {
            _faces = new List<Face>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                var face = new Face
                {
                    Plane = br.ReadUInt16(),
                    Side = br.ReadUInt16(),
                    FirstEdge = br.ReadInt32(),
                    NumEdges = br.ReadUInt16(),
                    TextureInfo = br.ReadUInt16(),
                    Styles = br.ReadBytes(Face.MaxLightmaps),
                    LightmapOffset = br.ReadInt32()
                };
                _faces.Add(face);
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
            foreach (var face in _faces)
            {
                bw.Write((ushort) face.Plane);
                bw.Write((ushort) face.Side);
                bw.Write((int) face.FirstEdge);
                bw.Write((ushort) face.NumEdges);
                bw.Write((ushort) face.TextureInfo);
                bw.Write((byte[]) face.Styles);
                bw.Write((int) face.LightmapOffset);
            }
            return (int)(bw.BaseStream.Position - pos);
        }

        #region IList

        public IEnumerator<Face> GetEnumerator()
        {
            return _faces.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _faces).GetEnumerator();
        }

        public void Add(Face item)
        {
            _faces.Add(item);
        }

        public void Clear()
        {
            _faces.Clear();
        }

        public bool Contains(Face item)
        {
            return _faces.Contains(item);
        }

        public void CopyTo(Face[] array, int arrayIndex)
        {
            _faces.CopyTo(array, arrayIndex);
        }

        public bool Remove(Face item)
        {
            return _faces.Remove(item);
        }

        public int Count => _faces.Count;

        public bool IsReadOnly => _faces.IsReadOnly;

        public int IndexOf(Face item)
        {
            return _faces.IndexOf(item);
        }

        public void Insert(int index, Face item)
        {
            _faces.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _faces.RemoveAt(index);
        }

        public Face this[int index]
        {
            get => _faces[index];
            set => _faces[index] = value;
        }

        #endregion
    }
}