using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Edges : ILump, IList<Edge>
    {
        private readonly IList<Edge> _edges;

        public Edges()
        {
            _edges = new List<Edge>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                var edge = new Edge
                {
                    Start = br.ReadUInt16(),
                    End = br.ReadUInt16()
                };
                _edges.Add(edge);
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
            foreach (var edge in _edges)
            {
                bw.Write((ushort) edge.Start);
                bw.Write((ushort) edge.End);
            }
            return sizeof(ushort) * 2 * _edges.Count;
        }

        #region IList

        public IEnumerator<Edge> GetEnumerator()
        {
            return _edges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _edges).GetEnumerator();
        }

        public void Add(Edge item)
        {
            _edges.Add(item);
        }

        public void Clear()
        {
            _edges.Clear();
        }

        public bool Contains(Edge item)
        {
            return _edges.Contains(item);
        }

        public void CopyTo(Edge[] array, int arrayIndex)
        {
            _edges.CopyTo(array, arrayIndex);
        }

        public bool Remove(Edge item)
        {
            return _edges.Remove(item);
        }

        public int Count => _edges.Count;

        public bool IsReadOnly => _edges.IsReadOnly;

        public int IndexOf(Edge item)
        {
            return _edges.IndexOf(item);
        }

        public void Insert(int index, Edge item)
        {
            _edges.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _edges.RemoveAt(index);
        }

        public Edge this[int index]
        {
            get => _edges[index];
            set => _edges[index] = value;
        }

        #endregion
    }
}