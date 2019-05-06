using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Areas : ILump, IList<Area>
    {
        private readonly IList<Area> _areas;

        public Areas()
        {
            _areas = new List<Area>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                var area = new Area
                {
                    NumAreaPortals = br.ReadInt32(),
                    FirstAreaPortal = br.ReadInt32()
                };
                _areas.Add(area);
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
            foreach (var area in _areas)
            {
                bw.Write((int) area.NumAreaPortals);
                bw.Write((int) area.FirstAreaPortal);
            }
            return (int)(bw.BaseStream.Position - pos);
        }

        #region IList

        public IEnumerator<Area> GetEnumerator()
        {
            return _areas.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _areas).GetEnumerator();
        }

        public void Add(Area item)
        {
            _areas.Add(item);
        }

        public void Clear()
        {
            _areas.Clear();
        }

        public bool Contains(Area item)
        {
            return _areas.Contains(item);
        }

        public void CopyTo(Area[] array, int arrayIndex)
        {
            _areas.CopyTo(array, arrayIndex);
        }

        public bool Remove(Area item)
        {
            return _areas.Remove(item);
        }

        public int Count => _areas.Count;

        public bool IsReadOnly => _areas.IsReadOnly;

        public int IndexOf(Area item)
        {
            return _areas.IndexOf(item);
        }

        public void Insert(int index, Area item)
        {
            _areas.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _areas.RemoveAt(index);
        }

        public Area this[int index]
        {
            get => _areas[index];
            set => _areas[index] = value;
        }

        #endregion
    }
}