using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Vertices : ILump, IList<Vector3>
    {
        private readonly IList<Vector3> _vertices;

        public Vertices()
        {
            _vertices = new List<Vector3>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                _vertices.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
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
            foreach (var se in _vertices)
            {
                bw.WriteVector3(se);
            }
            return sizeof(float) * 3 * _vertices.Count;
        }

        #region IList

        public IEnumerator<Vector3> GetEnumerator()
        {
            return _vertices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _vertices).GetEnumerator();
        }

        public void Add(Vector3 item)
        {
            _vertices.Add(item);
        }

        public void Clear()
        {
            _vertices.Clear();
        }

        public bool Contains(Vector3 item)
        {
            return _vertices.Contains(item);
        }

        public void CopyTo(Vector3[] array, int arrayIndex)
        {
            _vertices.CopyTo(array, arrayIndex);
        }

        public bool Remove(Vector3 item)
        {
            return _vertices.Remove(item);
        }

        public int Count => _vertices.Count;

        public bool IsReadOnly => _vertices.IsReadOnly;

        public int IndexOf(Vector3 item)
        {
            return _vertices.IndexOf(item);
        }

        public void Insert(int index, Vector3 item)
        {
            _vertices.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _vertices.RemoveAt(index);
        }

        public Vector3 this[int index]
        {
            get => _vertices[index];
            set => _vertices[index] = value;
        }

        #endregion
    }
}