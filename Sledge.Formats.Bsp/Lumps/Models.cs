using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Models : ILump, IList<Model>
    {
        private readonly IList<Model> _models;

        public Models()
        {
            _models = new List<Model>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            while (br.BaseStream.Position < blob.Offset + blob.Length)
            {
                var model = new Model
                {
                    Mins = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                    Maxs = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                    Origin = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                };
                switch (version)
                {
                    case Version.Goldsource:
                    case Version.Quake1:
                        model.HeadNodes = new[] { br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32() };
                        model.VisLeaves = br.ReadInt32();
                        break;
                    case Version.Quake2:
                        model.HeadNodes = new[] { br.ReadInt32(), -1, -1, -1 };
                        break;
                }
                model.FirstFace = br.ReadInt32();
                model.NumFaces = br.ReadInt32();
                _models.Add(model);
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
            foreach (var model in _models)
            {
                bw.WriteVector3(model.Mins);
                bw.WriteVector3(model.Maxs);
                bw.WriteVector3(model.Origin);
                switch (version)
                {
                    case Version.Goldsource:
                    case Version.Quake1:
                        bw.Write((int) model.HeadNodes[0]);
                        bw.Write((int) model.HeadNodes[1]);
                        bw.Write((int) model.HeadNodes[2]);
                        bw.Write((int) model.HeadNodes[3]);
                        bw.Write((int) model.VisLeaves);
                        break;
                    case Version.Quake2:
                        bw.Write((int) model.HeadNodes[0]);
                        break;
                }
                bw.Write((int) model.FirstFace);
                bw.Write((int) model.NumFaces);
            }
            return (int) (bw.BaseStream.Position - pos);
        }

        #region IList

        public IEnumerator<Model> GetEnumerator()
        {
            return _models.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _models).GetEnumerator();
        }

        public void Add(Model item)
        {
            _models.Add(item);
        }

        public void Clear()
        {
            _models.Clear();
        }

        public bool Contains(Model item)
        {
            return _models.Contains(item);
        }

        public void CopyTo(Model[] array, int arrayIndex)
        {
            _models.CopyTo(array, arrayIndex);
        }

        public bool Remove(Model item)
        {
            return _models.Remove(item);
        }

        public int Count => _models.Count;

        public bool IsReadOnly => _models.IsReadOnly;

        public int IndexOf(Model item)
        {
            return _models.IndexOf(item);
        }

        public void Insert(int index, Model item)
        {
            _models.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _models.RemoveAt(index);
        }

        public Model this[int index]
        {
            get => _models[index];
            set => _models[index] = value;
        }

        #endregion
    }
}