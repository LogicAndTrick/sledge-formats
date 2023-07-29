using System.Numerics;

namespace Sledge.Formats.Bsp.Objects
{
    public class TextureInfo
    {
        public Vector4 S { get; set; }
        public Vector4 T { get; set; }

        /// <summary>
        /// Index of mip texture in the miptextures lump. Only relevant for Goldsource and Quake 1 format bsps.
        /// </summary>
        public int MipTexture { get; set; }

        public TextureFlags Flags { get; set; }

        /// <summary>
        /// Value of texture. Only relevant for Quake 2 format bsps.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Name of texture. Only relevant for Quake 2 format bsps.
        /// </summary>
        public string TextureName { get; set; }

        /// <summary>
        /// Next texture info index. Only relevant for Quake 2 format bsps.
        /// </summary>
        public int NextTextureInfo { get; set; }

        private bool Equals(TextureInfo other)
        {
            return S.Equals(other.S) && T.Equals(other.T) && MipTexture == other.MipTexture && Flags == other.Flags && Value == other.Value && TextureName == other.TextureName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextureInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = S.GetHashCode();
                hashCode = (hashCode * 397) ^ T.GetHashCode();
                hashCode = (hashCode * 397) ^ MipTexture;
                hashCode = (hashCode * 397) ^ (int) Flags;
                hashCode = (hashCode * 397) ^ Value;
                hashCode = (hashCode * 397) ^ (TextureName != null ? TextureName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}