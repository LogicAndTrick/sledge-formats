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

        public int AddItem<TLump, TItem>(TItem item) where TLump : ILump, IList<TItem>
        {
            return AddItem<TLump, TItem>(item, x => x.Equals(item));
        }

        public int AddItem<TLump, TItem>(TItem item, Predicate<TItem> matcher) where TLump : ILump, IList<TItem>
        {
            var lump = GetLump<TLump>();
            for (var i = 0; i < lump.Count; i++)
            {
                if (matcher(lump[i])) return i;
            }

            // wasn't found
            lump.Add(item);
            return lump.Count - 1;
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

        /// <summary>
        /// Adds a plane to the bsp, if it's not already present.
        /// </summary>
        /// <returns>The index of the plane in the bsp</returns>
        public int AddPlane(System.Numerics.Plane plane)
        {
            return AddPlane(new Plane
            {
                Normal = -plane.Normal,
                Distance = -plane.D,
                Type = (-plane.Normal).GetBspPlaneTypeForNormal()
            });
        }

        /// <summary>
        /// Adds a plane to the bsp, if it's not already present.
        /// </summary>
        /// <returns>The index of the plane in the bsp</returns>
        public int AddPlane(Plane plane) => AddItem<Planes, Plane>(plane);

        /// <summary>
        /// Adds a miptexture to the bsp, if one with the same name is not already present.
        /// </summary>
        /// <returns>The index of the miptexture in the bsp</returns>
        public int AddMipTexture(MipTexture texture) => AddItem<Textures, MipTexture>(texture, x => x.Name.Equals(texture.Name, StringComparison.InvariantCultureIgnoreCase));

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
    }
}