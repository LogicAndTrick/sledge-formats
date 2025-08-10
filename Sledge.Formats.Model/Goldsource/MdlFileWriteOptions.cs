using System.Linq;

namespace Sledge.Formats.Model.Goldsource
{
    public class MdlFileWriteOptions
    {
        /// <summary>
        /// Force internal or external textures. Set as null to use automatic behaviour.
        /// </summary>
        public bool? SplitTextures { get; set; }

        /// <summary>
        /// Determine if textures should be split. If <see cref="SplitTextures"/> is set, that value is used. Otherwise, automatic behaviour is used.
        /// Automatic behaviour is to split textures if the model has external sequence files, if there's more than 64 textures, or if texture data is > 10mb.
        /// </summary>
        /// <param name="file">Model file to check</param>
        /// <returns>True to split textures, false otherwise</returns>
        public bool ShouldSplitTextures(MdlFile file)
        {
            const int maxTextures = 64;
            const int maxTextureSize = 10 * 1024 * 1024;

            if (SplitTextures.HasValue) return SplitTextures.Value;
            if (file.Textures.Count > maxTextures) return true;
            if (file.SequenceGroups.Count > 1) return true;
            if (file.Textures.Sum(t => t.Data.Length) > maxTextureSize) return true;
            return false;
        }
    }
}