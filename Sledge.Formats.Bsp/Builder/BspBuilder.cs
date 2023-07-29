using System;
using System.Collections.Generic;
using Sledge.Formats.Bsp.Lumps;
using Sledge.Formats.Bsp.Objects;
using Sledge.Formats.Id;

namespace Sledge.Formats.Bsp.Builder
{
    public class BspBuilder
    {
        public Version Version => BspFile.Version;
        public BspFile BspFile { get; }

        public BspBuilder(Version version)
        {
            BspFile = new BspFile(version);
        }

        public BspBuilder(BspFile bspFile)
        {
            BspFile = bspFile;
        }

        public T GetLump<T>() where T : ILump
        {
            var lump = BspFile.GetLump<T>();
            if (lump == null)
            {
                lump = Activator.CreateInstance<T>();
                BspFile.Lumps.Add(lump);
            }

            return lump;
        }

        private readonly Dictionary<ILump, object> _dictWrappers = new Dictionary<ILump, object>();

        public int AddItem<TLump, TItem>(TItem item) where TLump : ILump, IList<TItem>
        {
            return AddItem<TLump, TItem>(item, null);
        }

        public int AddItem<TLump, TItem>(TItem item, IEqualityComparer<TItem> comparer) where TLump : ILump, IList<TItem>
        {
            var lump = GetLump<TLump>();
            if (!_dictWrappers.TryGetValue(lump, out var w))
            {
                w = new ListDictionaryWrapper<TItem>(lump, comparer);
                _dictWrappers.Add(lump, w);
            }
            var wrapper = (ListDictionaryWrapper<TItem>) w;
            return wrapper.AddOrGet(item);
        }

        private int GetItemIndex<TLump, TItem>(TItem item) where TLump : ILump, IList<TItem>
        {
            var lump = BspFile.GetLump<TLump>();
            if (lump == null) return -1;
            if (!_dictWrappers.TryGetValue(lump, out var w)) return -1;
            var wrapper = (ListDictionaryWrapper<TItem>)w;
            return wrapper.Get(item);
        }

        private TItem GetItem<TLump, TItem>(int index) where TLump : ILump, IList<TItem>
        {
            var lump = BspFile.GetLump<TLump>();
            return lump[index];
        }

        private bool TryGetItem<TLump, TItem>(int index, out TItem item) where TLump : ILump, IList<TItem>
        {
            item = default;
            var lump = BspFile.GetLump<TLump>();
            if (lump == null) return false;
            if (lump.Count <= index) return false;
            item = lump[index];
            return true;
        }

        /*
        typeof(Entities),
        typeof(Planes),
        typeof(Textures),
        typeof(Texinfo),
        ---
        typeof(Vertices),
        typeof(Visibility),
        typeof(Nodes),
        typeof(Faces),
        typeof(Lightmaps),
        typeof(Clipnodes),
        typeof(Leaves),
        typeof(LeafFaces),
        typeof(Edges),
        typeof(Surfedges),
        typeof(Models),
         */

        /// <summary>
        /// Adds an entity to the bsp, if it's not already present.
        /// </summary>
        /// <returns>The index of the entity in the bsp</returns>
        public int AddEntity(Entity entity) => AddItem<Entities, Entity>(entity);

        public int GetEntityIndex(Entity entity) => GetItemIndex<Entities, Entity>(entity);

        public Entity GetEntity(int index) => GetItem<Entities, Entity>(index);

        /// <summary>
        /// Adds a plane to the bsp, if it's not already present.
        /// </summary>
        /// <param name="plane">The plane to add</param>
        /// <param name="addInverse">True to also add the inverse of the plane, for axial planes, the positive-facing version of the plane will be assigned to the lower index.</param>
        /// <returns>The index of the plane in the bsp (not the index of the inverse)</returns>
        public int AddPlane(System.Numerics.Plane plane, bool addInverse = true)
        {
            return AddPlane(new Plane
            {
                Normal = plane.Normal,
                Distance = -plane.D,
                Type = plane.Normal.GetBspPlaneTypeForNormal()
            }, addInverse);
        }

        /// <summary>
        /// Adds a plane to the bsp, if it's not already present.
        /// </summary>
        /// <param name="plane">The plane to add</param>
        /// <param name="addInverse">True to also add the inverse of the plane, for axial planes, the positive-facing version of the plane will be assigned to the lower index.</param>
        /// <returns>The index of the plane in the bsp (not the index of the inverse)</returns>
        public int AddPlane(Plane plane, bool addInverse = true)
        {
            if (addInverse)
            {
                var p1 = plane;
                var p2 = new Plane
                {
                    Normal = -p1.Normal,
                    Distance = -p1.Distance,
                    Type = p1.Type
                };

                // always put axial planes facing positive first
                if (p1.Type <= PlaneType.Z && (p1.Normal.X < 0 || p1.Normal.Y < 0 || p1.Normal.Z < 0))
                {
                    AddItem<Planes, Plane>(p2);
                    var idx = AddItem<Planes, Plane>(p1);
                    return idx;
                }
                else
                {
                    var idx = AddItem<Planes, Plane>(p1);
                    AddItem<Planes, Plane>(p2);
                    return idx;
                }
            }
            else
            {
                return AddItem<Planes, Plane>(plane);
            }
        }

        public int GetPlaneIndex(System.Numerics.Plane plane)
        {
            return GetPlaneIndex(new Plane
            {
                Normal = plane.Normal,
                Distance = -plane.D,
                Type = plane.Normal.GetBspPlaneTypeForNormal()
            });
        }

        public int GetPlaneIndex(Plane plane) => GetItemIndex<Planes, Plane>(plane);

        public Plane GetPlane(int index) => GetItem<Planes, Plane>(index);

        /// <summary>
        /// Adds a miptexture to the bsp, if one with the same name is not already present.
        /// </summary>
        /// <returns>The index of the miptexture in the bsp</returns>
        public int AddMipTexture(MipTexture texture) => AddItem<Textures, MipTexture>(texture, MipTextureComparer.Instance);

        public int GetMipTextureIndex(MipTexture texture) => GetItemIndex<Textures, MipTexture>(texture);

        public int GetMipTextureIndex(string name) => GetItemIndex<Textures, MipTexture>(new MipTexture { Name = name ?? "" });

        public MipTexture GetMipTexture(int index) => GetItem<Textures, MipTexture>(index);

        public MipTexture GetMipTexture(string name)
        {
            var dummy = new MipTexture { Name = name };
            var idx = GetItemIndex<Textures, MipTexture>(dummy);
            return idx < 0 ? null : GetMipTexture(idx);
        }

        private class MipTextureComparer : EqualityComparer<MipTexture>
        {
            public static readonly MipTextureComparer Instance = new MipTextureComparer();
            public override bool Equals(MipTexture x, MipTexture y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return StringComparer.InvariantCultureIgnoreCase.Equals(x.Name, y.Name);
            }

            public override int GetHashCode(MipTexture obj)
            {
                return StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Name);
            }
        }

        /// <summary>
        /// Adds a texture info to the bsp, if it's not already present.
        /// </summary>
        /// <returns>The index of the texture info in the bsp</returns>
        public int AddTextureInfo(TextureInfo textureInfo) => AddItem<Texinfo, TextureInfo>(textureInfo);

        /// <summary>
        /// Adds a texture info & associated miptexture to the bsp, if it's not already present.
        /// </summary>
        /// <returns>The index of the texture info in the bsp</returns>
        public int AddTextureInfo(TextureInfo textureInfo, MipTexture mipTexture)
        {
            textureInfo.MipTexture = AddMipTexture(mipTexture);
            return AddItem<Texinfo, TextureInfo>(textureInfo);
        }

        public int GetTextureInfoIndex(TextureInfo textureInfo) => GetItemIndex<Texinfo, TextureInfo>(textureInfo);

        public TextureInfo GetTextureInfo(int index) => GetItem<Texinfo, TextureInfo>(index);
    }
}