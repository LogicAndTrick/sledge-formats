using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Sledge.Formats.Texture.Vtf;

namespace Sledge.Formats.Texture.ImageSharp;

public class VtfImageBuilderOptions
{
    /// <summary>
    /// The image format to use when creating vtf images.
    /// The default value is Rgba8888.
    /// </summary>
    public VtfImageFormat ImageFormat { get; set; } = VtfImageFormat.Rgba8888;

    /// <summary>
    /// True to create mipmaps when creating the vtf images.
    /// The default value is true.
    /// </summary>
    public bool AutoCreateMipmaps { get; set; } = true;

    /// <summary>
    /// The maximum number of mipmap levels to create. Set to -1 to have no limit.
    /// The default value is -1.
    /// <see cref="AutoCreateMipmaps">AutoCreateMipmaps</see> must be true to create mipmaps.
    /// </summary>
    public int NumMipmapLevels { get; set; } = -1;

    /// <summary>
    /// The minimum dimension of either width or height before stopping mipmap creation.
    /// The default value is 1.
    /// <see cref="AutoCreateMipmaps">AutoCreateMipmaps</see> must be true to create mipmaps.
    /// </summary>
    public int MinimumMipmapDimension { get; set; } = 1;

    /// <summary>
    /// If the image has multiple frames, they will be added to the vtf image.
    /// The default value is true.
    /// </summary>
    public bool CreateMultipleFramesIfPresent { get; set; } = true;

    /// <summary>
    /// True to automatically resize the image to the closest power of two, if not already.
    /// The default value is false.
    /// </summary>
    public bool AutoResizeToPowerOfTwo { get; set; } = false;

    /// <summary>
    /// Resampler to use when resizing the image, either to power of two dimensions, or for mipmaps.
    /// The default value is <see cref="BicubicResampler"/>.
    /// </summary>
    public IResampler Resampler { get; set; } = new BicubicResampler();
}