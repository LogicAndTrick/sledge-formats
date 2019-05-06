using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Clipnodes : ILump, IList<Clipnode>
    {
        private readonly IList<Clipnode> _clipnodes;

        public Clipnodes()
        {
            _clipnodes = new List<Clipnode>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                var clip = new Clipnode
                {
                    Plane = br.ReadUInt32(),
                    Children = new[] { br.ReadInt16(), br.ReadInt16() }
                };
                _clipnodes.Add(clip);
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
            foreach (var node in _clipnodes)
            {
                bw.Write((uint) node.Plane);
                bw.Write((short) node.Children[0]);
                bw.Write((short) node.Children[1]);
            }
            return (int)(bw.BaseStream.Position - pos);
        }

        #region IList

        public IEnumerator<Clipnode> GetEnumerator()
        {
            return _clipnodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _clipnodes).GetEnumerator();
        }

        public void Add(Clipnode item)
        {
            _clipnodes.Add(item);
        }

        public void Clear()
        {
            _clipnodes.Clear();
        }

        public bool Contains(Clipnode item)
        {
            return _clipnodes.Contains(item);
        }

        public void CopyTo(Clipnode[] array, int arrayIndex)
        {
            _clipnodes.CopyTo(array, arrayIndex);
        }

        public bool Remove(Clipnode item)
        {
            return _clipnodes.Remove(item);
        }

        public int Count => _clipnodes.Count;

        public bool IsReadOnly => _clipnodes.IsReadOnly;

        public int IndexOf(Clipnode item)
        {
            return _clipnodes.IndexOf(item);
        }

        public void Insert(int index, Clipnode item)
        {
            _clipnodes.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _clipnodes.RemoveAt(index);
        }

        public Clipnode this[int index]
        {
            get => _clipnodes[index];
            set => _clipnodes[index] = value;
        }

        #endregion
    }
}