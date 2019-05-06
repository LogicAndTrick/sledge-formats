using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Sledge.Formats.Bsp.Lumps
{
    public class LeafFaces : ILump, IList<ushort>
    {
        private readonly IList<ushort> _leafFaces;

        public LeafFaces()
        {
            _leafFaces = new List<ushort>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            var num = blob.Length / sizeof(ushort);
            for (var i = 0; i < num; i++)
            {
                _leafFaces.Add(br.ReadUInt16());
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
            foreach (var se in _leafFaces)
            {
                bw.Write((ushort) se);
            }
            return sizeof(ushort) * _leafFaces.Count;
        }

        #region IList

        public IEnumerator<ushort> GetEnumerator()
        {
            return _leafFaces.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _leafFaces).GetEnumerator();
        }

        public void Add(ushort item)
        {
            _leafFaces.Add(item);
        }

        public void Clear()
        {
            _leafFaces.Clear();
        }

        public bool Contains(ushort item)
        {
            return _leafFaces.Contains(item);
        }

        public void CopyTo(ushort[] array, int arrayIndex)
        {
            _leafFaces.CopyTo(array, arrayIndex);
        }

        public bool Remove(ushort item)
        {
            return _leafFaces.Remove(item);
        }

        public int Count => _leafFaces.Count;

        public bool IsReadOnly => _leafFaces.IsReadOnly;

        public int IndexOf(ushort item)
        {
            return _leafFaces.IndexOf(item);
        }

        public void Insert(int index, ushort item)
        {
            _leafFaces.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _leafFaces.RemoveAt(index);
        }

        public ushort this[int index]
        {
            get => _leafFaces[index];
            set => _leafFaces[index] = value;
        }

        #endregion
    }
}