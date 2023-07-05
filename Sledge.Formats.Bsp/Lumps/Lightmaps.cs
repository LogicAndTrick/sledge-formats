using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Lumps
{
    public class Lightmaps : ILump, IList<Lightmap>
    {
        private readonly IList<Lightmap> _lightmaps;
        private byte[] _lightmapData;

        public Lightmaps()
        {
            _lightmaps = new List<Lightmap>();
            _lightmapData = Array.Empty<byte>();
        }

        public void Read(BinaryReader br, Blob blob, Version version)
        {
            _lightmapData = br.ReadBytes(blob.Length);
        }

        public void PostReadProcess(BspFile bsp)
        {
            var textureInfos = bsp.GetLump<Texinfo>();
            var planes = bsp.GetLump<Planes>();
            var surfEdges = bsp.GetLump<Surfedges>();
            var edges = bsp.GetLump<Edges>();
            var vertices = bsp.GetLump<Vertices>();
            var faces = bsp.GetLump<Faces>()
                .Where(x => x.Styles.Length > 0 && x.Styles[0] != byte.MaxValue) // Indicates a fullbright face, no offset
                .Where(x => x.LightmapOffset >= 0 && x.LightmapOffset < _lightmapData.Length) // Invalid offset
                .ToList();

            var offsetDict = new Dictionary<int, Lightmap>();
            foreach (var face in faces)
            {
                if (offsetDict.ContainsKey(face.LightmapOffset)) continue;

                var ti = textureInfos[face.TextureInfo];
                var pl = planes[face.Plane];

                var uvs = new List<Vector2>();
                for (var i = 0; i < face.NumEdges; i++)
                {
                    var ei = surfEdges[face.FirstEdge + i];
                    var edge = edges[Math.Abs(ei)];
                    var point = vertices[ei > 0 ? edge.Start : edge.End];

                    var sn = new Vector3(ti.S.X, ti.S.Y, ti.S.Z);
                    var u = Vector3.Dot(point, sn) + ti.S.W;

                    var tn = new Vector3(ti.T.X, ti.T.Y, ti.T.Z);
                    var v = Vector3.Dot(point, tn) + ti.T.W;

                    uvs.Add(new Vector2(u, v));
                }

                var minu = uvs.Min(x => x.X);
                var maxu = uvs.Max(x => x.X);
                var minv = uvs.Min(x => x.Y);
                var maxv = uvs.Max(x => x.Y);

                var width = (int) Math.Ceiling(maxu / 16) - (int)Math.Floor(minu / 16) + 1;
                var height = (int) Math.Ceiling(maxv / 16) - (int)Math.Floor(minv / 16) + 1;
                var bpp = bsp.Version == Version.Quake1 ? 1 : 3;

                // It's possible for the calculated size of the texture to exceed the remaining amount of data, so don't try to over-read.
                var idealSizeInBytes = bpp * width * height;
                var sizeInBytes = Math.Min(idealSizeInBytes, _lightmapData.Length - face.LightmapOffset);

                if (idealSizeInBytes != sizeInBytes)
                {
                    // Try to find the closest texture size that matches the data we have.
                    var size = sizeInBytes / bpp;

                    // See if the size is a multiple of either width or height.
                    if ((size % height) == 0)
                    {
                        width = size / height;
                    }
                    else
                    {
                        // If size is not a multiple of width this will truncate height to avoid invalid access.
                        height = size / width;
                    }
                }

                var data = new byte[sizeInBytes];
                Array.Copy(_lightmapData, face.LightmapOffset, data, 0, data.Length);

                var map = new Lightmap
                {
                    Offset = face.LightmapOffset,
                    Width = width,
                    Height = height,
                    BitsPerPixel = bpp,
                    Data = data
                };
                _lightmaps.Add(map);
                offsetDict.Add(map.Offset, map);
            }
        }

        public void PreWriteProcess(BspFile bsp, Version version)
        {
            // throw new NotImplementedException("Lightmap data must be pre-processed");
        }

        public int Write(BinaryWriter bw, Version version)
        {
            bw.Write(_lightmapData);
            return _lightmapData.Length;
        }

        #region IList

        public IEnumerator<Lightmap> GetEnumerator()
        {
            return _lightmaps.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _lightmaps).GetEnumerator();
        }

        public void Add(Lightmap item)
        {
            _lightmaps.Add(item);
        }

        public void Clear()
        {
            _lightmaps.Clear();
        }

        public bool Contains(Lightmap item)
        {
            return _lightmaps.Contains(item);
        }

        public void CopyTo(Lightmap[] array, int arrayIndex)
        {
            _lightmaps.CopyTo(array, arrayIndex);
        }

        public bool Remove(Lightmap item)
        {
            return _lightmaps.Remove(item);
        }

        public int Count => _lightmaps.Count;

        public bool IsReadOnly => _lightmaps.IsReadOnly;

        public int IndexOf(Lightmap item)
        {
            return _lightmaps.IndexOf(item);
        }

        public void Insert(int index, Lightmap item)
        {
            _lightmaps.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _lightmaps.RemoveAt(index);
        }

        public Lightmap this[int index]
        {
            get => _lightmaps[index];
            set => _lightmaps[index] = value;
        }

        #endregion
    }
}