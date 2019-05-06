using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class BrushSides : ILump, IList<BrushSide>
    {
        private readonly IList<BrushSide> _brushSides;

        public BrushSides()
        {
            _brushSides = new List<BrushSide>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                var bs = new BrushSide
                {
                    Plane = br.ReadUInt16(),
                    Texinfo = br.ReadInt16()
                };
                _brushSides.Add(bs);
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
            foreach (var bs in _brushSides)
            {
                bw.Write((ushort) bs.Plane);
                bw.Write((short) bs.Texinfo);
            }
            return (int)(bw.BaseStream.Position - pos);
        }

        #region IList

        public IEnumerator<BrushSide> GetEnumerator()
        {
            return _brushSides.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _brushSides).GetEnumerator();
        }

        public void Add(BrushSide item)
        {
            _brushSides.Add(item);
        }

        public void Clear()
        {
            _brushSides.Clear();
        }

        public bool Contains(BrushSide item)
        {
            return _brushSides.Contains(item);
        }

        public void CopyTo(BrushSide[] array, int arrayIndex)
        {
            _brushSides.CopyTo(array, arrayIndex);
        }

        public bool Remove(BrushSide item)
        {
            return _brushSides.Remove(item);
        }

        public int Count => _brushSides.Count;

        public bool IsReadOnly => _brushSides.IsReadOnly;

        public int IndexOf(BrushSide item)
        {
            return _brushSides.IndexOf(item);
        }

        public void Insert(int index, BrushSide item)
        {
            _brushSides.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _brushSides.RemoveAt(index);
        }

        public BrushSide this[int index]
        {
            get => _brushSides[index];
            set => _brushSides[index] = value;
        }

        #endregion
    }
}