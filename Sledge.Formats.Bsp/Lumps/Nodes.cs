using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Nodes : ILump, IList<Node>
    {
        private readonly IList<Node> _nodes;

        public Nodes()
        {
            _nodes = new List<Node>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                var node = new Node
                {
                    Plane = br.ReadUInt32(),
                    Children = new int[2],
                    Mins = new short[3],
                    Maxs = new short[3]
                };
                switch (version)
                {
                    case Version.Quake1:
                    case Version.Goldsource:
                        for (var i = 0; i < 2; i++) node.Children[i] = br.ReadInt16();
                        break;
                    case Version.Quake2:
                        for (var i = 0; i < 2; i++) node.Children[i] = br.ReadInt32();
                        break;
                }
                for (var i = 0; i < 3; i++) node.Mins[i] = br.ReadInt16();
                for (var i = 0; i < 3; i++) node.Maxs[i] = br.ReadInt16();
                node.FirstFace = br.ReadUInt16();
                node.NumFaces = br.ReadUInt16();
                _nodes.Add(node);
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
            foreach (var node in _nodes)
            {
                bw.Write((uint) node.Plane);

                switch (version)
                {
                    case Version.Quake1:
                    case Version.Goldsource:
                        bw.Write((short) node.Children[0]);
                        bw.Write((short) node.Children[1]);
                        break;
                    case Version.Quake2:
                        bw.Write((int) node.Children[0]);
                        bw.Write((int) node.Children[1]);
                        break;
                }

                bw.Write((short) node.Mins[0]);
                bw.Write((short) node.Mins[1]);
                bw.Write((short) node.Mins[2]);

                bw.Write((short) node.Maxs[0]);
                bw.Write((short) node.Maxs[1]);
                bw.Write((short) node.Maxs[2]);

                bw.Write((ushort) node.FirstFace);
                bw.Write((ushort) node.NumFaces);
            }
            return (int)(bw.BaseStream.Position - pos);
        }

        #region IList

        public IEnumerator<Node> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _nodes).GetEnumerator();
        }

        public void Add(Node item)
        {
            _nodes.Add(item);
        }

        public void Clear()
        {
            _nodes.Clear();
        }

        public bool Contains(Node item)
        {
            return _nodes.Contains(item);
        }

        public void CopyTo(Node[] array, int arrayIndex)
        {
            _nodes.CopyTo(array, arrayIndex);
        }

        public bool Remove(Node item)
        {
            return _nodes.Remove(item);
        }

        public int Count => _nodes.Count;

        public bool IsReadOnly => _nodes.IsReadOnly;

        public int IndexOf(Node item)
        {
            return _nodes.IndexOf(item);
        }

        public void Insert(int index, Node item)
        {
            _nodes.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _nodes.RemoveAt(index);
        }

        public Node this[int index]
        {
            get => _nodes[index];
            set => _nodes[index] = value;
        }

        #endregion
    }
}