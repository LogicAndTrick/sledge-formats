using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Sledge.Formats.Bsp.Objects;
using Plane = Sledge.Formats.Bsp.Objects.Plane;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Planes : ILump, IList<Plane>
    {
        private readonly IList<Plane> _planes;

        public Planes()
        {
            _planes = new List<Plane>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                _planes.Add(new Plane
                {
                    Normal = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                    Distance = br.ReadSingle(),
                    Type = (PlaneType) br.ReadInt32()
                });
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
            foreach (var plane in _planes)
            {
                bw.WriteVector3(plane.Normal);
                bw.Write((float) plane.Distance);
                bw.Write((int) plane.Type);
            }
            return (int)(bw.BaseStream.Position - pos);
        }

        #region IList

        public IEnumerator<Plane> GetEnumerator()
        {
            return _planes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_planes).GetEnumerator();
        }

        public void Add(Plane item)
        {
            _planes.Add(item);
        }

        public void Clear()
        {
            _planes.Clear();
        }

        public bool Contains(Plane item)
        {
            return _planes.Contains(item);
        }

        public void CopyTo(Plane[] array, int arrayIndex)
        {
            _planes.CopyTo(array, arrayIndex);
        }

        public bool Remove(Plane item)
        {
            return _planes.Remove(item);
        }

        public int Count => _planes.Count;

        public bool IsReadOnly => _planes.IsReadOnly;

        public int IndexOf(Plane item)
        {
            return _planes.IndexOf(item);
        }

        public void Insert(int index, Plane item)
        {
            _planes.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _planes.RemoveAt(index);
        }

        public Plane this[int index]
        {
            get => _planes[index];
            set => _planes[index] = value;
        }

        #endregion
    }
}