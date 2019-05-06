using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class AreaPortals : ILump, IList<AreaPortal>
    {
        private readonly IList<AreaPortal> _areaPortals;

        public AreaPortals()
        {
            _areaPortals = new List<AreaPortal>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                var ap = new AreaPortal
                {
                    PortalNum = br.ReadInt32(),
                    OtherArea = br.ReadInt32()
                };
                _areaPortals.Add(ap);
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
            foreach (var ap in _areaPortals)
            {
                bw.Write((int) ap.PortalNum);
                bw.Write((int) ap.OtherArea);
            }
            return (int)(bw.BaseStream.Position - pos);
        }

        #region IList

        public IEnumerator<AreaPortal> GetEnumerator()
        {
            return _areaPortals.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _areaPortals).GetEnumerator();
        }

        public void Add(AreaPortal item)
        {
            _areaPortals.Add(item);
        }

        public void Clear()
        {
            _areaPortals.Clear();
        }

        public bool Contains(AreaPortal item)
        {
            return _areaPortals.Contains(item);
        }

        public void CopyTo(AreaPortal[] array, int arrayIndex)
        {
            _areaPortals.CopyTo(array, arrayIndex);
        }

        public bool Remove(AreaPortal item)
        {
            return _areaPortals.Remove(item);
        }

        public int Count => _areaPortals.Count;

        public bool IsReadOnly => _areaPortals.IsReadOnly;

        public int IndexOf(AreaPortal item)
        {
            return _areaPortals.IndexOf(item);
        }

        public void Insert(int index, AreaPortal item)
        {
            _areaPortals.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _areaPortals.RemoveAt(index);
        }

        public AreaPortal this[int index]
        {
            get => _areaPortals[index];
            set => _areaPortals[index] = value;
        }

        #endregion
    }
}