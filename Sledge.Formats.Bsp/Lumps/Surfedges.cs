using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Surfedges : ILump, IList<int>
    {
        private readonly IList<int> _surfaceEdges;

        public Surfedges()
        {
            _surfaceEdges = new List<int>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            var num = blob.Length / sizeof(int);
            for (var i = 0; i < num; i++)
            {
                _surfaceEdges.Add(br.ReadInt32());
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
            foreach (var se in _surfaceEdges)
            {
                bw.Write((int) se);
            }
            return sizeof(int) * _surfaceEdges.Count;
        }

        #region IList

        public IEnumerator<int> GetEnumerator()
        {
            return _surfaceEdges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _surfaceEdges).GetEnumerator();
        }

        public void Add(int item)
        {
            _surfaceEdges.Add(item);
        }

        public void Clear()
        {
            _surfaceEdges.Clear();
        }

        public bool Contains(int item)
        {
            return _surfaceEdges.Contains(item);
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            _surfaceEdges.CopyTo(array, arrayIndex);
        }

        public bool Remove(int item)
        {
            return _surfaceEdges.Remove(item);
        }

        public int Count => _surfaceEdges.Count;

        public bool IsReadOnly => _surfaceEdges.IsReadOnly;

        public int IndexOf(int item)
        {
            return _surfaceEdges.IndexOf(item);
        }

        public void Insert(int index, int item)
        {
            _surfaceEdges.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _surfaceEdges.RemoveAt(index);
        }

        public int this[int index]
        {
            get => _surfaceEdges[index];
            set => _surfaceEdges[index] = value;
        }

        #endregion
    }
}