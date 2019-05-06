using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Brushes : ILump, IList<Brush>
    {
        private readonly IList<Brush> _brushes;

        public Brushes()
        {
            _brushes = new List<Brush>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                var brush = new Brush
                {
                    FirstSide = br.ReadInt32(),
                    NumSides = br.ReadInt32(),
                    Contents = br.ReadInt32()
                };
                _brushes.Add(brush);
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
            foreach (var brush in _brushes)
            {
                bw.Write((int) brush.FirstSide);
                bw.Write((int) brush.NumSides);
                bw.Write((int) brush.Contents);
            }
            return (int)(bw.BaseStream.Position - pos);
        }

        #region IList

        public IEnumerator<Brush> GetEnumerator()
        {
            return _brushes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _brushes).GetEnumerator();
        }

        public void Add(Brush item)
        {
            _brushes.Add(item);
        }

        public void Clear()
        {
            _brushes.Clear();
        }

        public bool Contains(Brush item)
        {
            return _brushes.Contains(item);
        }

        public void CopyTo(Brush[] array, int arrayIndex)
        {
            _brushes.CopyTo(array, arrayIndex);
        }

        public bool Remove(Brush item)
        {
            return _brushes.Remove(item);
        }

        public int Count => _brushes.Count;

        public bool IsReadOnly => _brushes.IsReadOnly;

        public int IndexOf(Brush item)
        {
            return _brushes.IndexOf(item);
        }

        public void Insert(int index, Brush item)
        {
            _brushes.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _brushes.RemoveAt(index);
        }

        public Brush this[int index]
        {
            get => _brushes[index];
            set => _brushes[index] = value;
        }

        #endregion
    }
}