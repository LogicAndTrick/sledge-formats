using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Leaves : ILump, IList<Leaf>
    {
        private readonly IList<Leaf> _leaves;

        public Leaves()
        {
            _leaves = new List<Leaf>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                var leaf = new Leaf
                {
                    Contents = (Contents)br.ReadInt32(),
                    AmbientLevels = new byte[Leaf.MaxNumAmbientLevels]
                };

                switch (version)
                {
                    case Version.Goldsource:
                    case Version.Quake1:
                        leaf.VisOffset = br.ReadInt32();
                        break;
                    case Version.Quake2:
                        leaf.Cluster = br.ReadInt16();
                        leaf.Area = br.ReadInt16();
                        break;
                }

                leaf.Mins = new[] { br.ReadInt16(), br.ReadInt16(), br.ReadInt16() };
                leaf.Maxs = new[] { br.ReadInt16(), br.ReadInt16(), br.ReadInt16() };

                leaf.FirstLeafFace = br.ReadUInt16();
                leaf.NumLeafFaces = br.ReadUInt16();

                switch (version)
                {
                    case Version.Goldsource:
                    case Version.Quake1:
                        leaf.AmbientLevels = br.ReadBytes(Leaf.MaxNumAmbientLevels);
                        break;
                    case Version.Quake2:
                        leaf.FirstLeafBrush = br.ReadUInt16();
                        leaf.NumLeafBrushes = br.ReadUInt16();
                        break;
                }

                _leaves.Add(leaf);
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
            foreach (var leaf in _leaves)
            {
                bw.Write((int) leaf.Contents);
                switch (version)
                {
                    case Version.Goldsource:
                    case Version.Quake1:
                        bw.Write((int) leaf.VisOffset);
                        break;
                    case Version.Quake2:
                        bw.Write((short) leaf.Cluster);
                        bw.Write((short) leaf.Area);
                        break;
                }

                bw.Write((short) leaf.Mins[0]);
                bw.Write((short) leaf.Mins[1]);
                bw.Write((short) leaf.Mins[2]);

                bw.Write((short) leaf.Maxs[0]);
                bw.Write((short) leaf.Maxs[1]);
                bw.Write((short) leaf.Maxs[2]);

                bw.Write((ushort) leaf.FirstLeafFace);
                bw.Write((ushort) leaf.NumLeafFaces);

                switch (version)
                {
                    case Version.Goldsource:
                    case Version.Quake1:
                        bw.Write((byte[]) leaf.AmbientLevels);
                        break;
                    case Version.Quake2:
                        bw.Write((ushort)leaf.FirstLeafBrush);
                        bw.Write((ushort)leaf.NumLeafBrushes);
                        break;
                }
            }
            return (int) (bw.BaseStream.Position - pos);
        }

        #region IList

        public IEnumerator<Leaf> GetEnumerator()
        {
            return _leaves.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _leaves).GetEnumerator();
        }

        public void Add(Leaf item)
        {
            _leaves.Add(item);
        }

        public void Clear()
        {
            _leaves.Clear();
        }

        public bool Contains(Leaf item)
        {
            return _leaves.Contains(item);
        }

        public void CopyTo(Leaf[] array, int arrayIndex)
        {
            _leaves.CopyTo(array, arrayIndex);
        }

        public bool Remove(Leaf item)
        {
            return _leaves.Remove(item);
        }

        public int Count => _leaves.Count;

        public bool IsReadOnly => _leaves.IsReadOnly;

        public int IndexOf(Leaf item)
        {
            return _leaves.IndexOf(item);
        }

        public void Insert(int index, Leaf item)
        {
            _leaves.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _leaves.RemoveAt(index);
        }

        public Leaf this[int index]
        {
            get => _leaves[index];
            set => _leaves[index] = value;
        }

        #endregion
    }
}