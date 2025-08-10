using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Sledge.Formats.Texture.Vtf;

namespace Sledge.Formats.Texture.ImageSharp;

public static class VtfExtensions
{
    /// <summary>
    /// Convert a vtf image to an ImageSharp image.
    /// </summary>
    public static Image<T> ToImage<T>(this VtfImage vtfImage) where T : unmanaged, IPixel<T>
    {
        using var conv = Image.LoadPixelData<Bgra32>(vtfImage.GetBgra32Data(), vtfImage.Width, vtfImage.Height);
        return conv.CloneAs<T>();
    }

    /// <summary>
    /// Convert a vtf file to an ImageSharp image. The largest image will be used, for the first face, and the first frame.
    /// </summary>
    public static Image<T> ToImage<T>(this VtfFile vtfFile) where T : unmanaged, IPixel<T>
    {
        return ToImage<T>(vtfFile.Images.OrderByDescending(x => x.Width).ThenBy(x => x.Frame).ThenBy(x => x.Face).First());
    }

    /// <summary>
    /// Convert an ImageSharp image to a vtf image.
    /// </summary>
    public static VtfImage ToVtfImage(this Image image, VtfImageBuilderOptions? options = null)
    {
        var builder = new VtfImageBuilder(options ?? new VtfImageBuilderOptions());
        return builder.CreateImage(image);
    }

    /// <summary>
    /// Convert an ImageSharp image to a vtf file.
    /// </summary>
    public static VtfFile ToVtfFile(this Image image, VtfImageBuilderOptions? options = null)
    {
        var vtf = new VtfFile();
        var builder = new VtfImageBuilder(options ?? new VtfImageBuilderOptions());
        foreach (var img in builder.CreateImages(image)) vtf.AddImage(img);
        return vtf;
    }
}