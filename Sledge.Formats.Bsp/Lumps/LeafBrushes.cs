using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Sledge.Formats.Bsp.Lumps
{
    public class LeafBrushes : ILump, IList<ushort>
    {
        private readonly IList<ushort> _leafBrushes;

        public LeafBrushes()
        {
            _leafBrushes = new List<ushort>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            var num = blob.Length / sizeof(ushort);
            for (var i = 0; i < num; i++)
            {
                _leafBrushes.Add(br.ReadUInt16());
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
            foreach (var lb in _leafBrushes)
            {
                bw.Write((ushort) lb);
            }
            return sizeof(ushort) * _leafBrushes.Count;
        }

        #region IList

        public IEnumerator<ushort> GetEnumerator()
        {
            return _leafBrushes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _leafBrushes).GetEnumerator();
        }

        public void Add(ushort item)
        {
            _leafBrushes.Add(item);
        }

        public void Clear()
        {
            _leafBrushes.Clear();
        }

        public bool Contains(ushort item)
        {
            return _leafBrushes.Contains(item);
        }

        public void CopyTo(ushort[] array, int arrayIndex)
        {
            _leafBrushes.CopyTo(array, arrayIndex);
        }

        public bool Remove(ushort item)
        {
            return _leafBrushes.Remove(item);
        }

        public int Count => _leafBrushes.Count;

        public bool IsReadOnly => _leafBrushes.IsReadOnly;

        public int IndexOf(ushort item)
        {
            return _leafBrushes.IndexOf(item);
        }

        public void Insert(int index, ushort item)
        {
            _leafBrushes.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _leafBrushes.RemoveAt(index);
        }

        public ushort this[int index]
        {
            get => _leafBrushes[index];
            set => _leafBrushes[index] = value;
        }

        #endregion
    }
}