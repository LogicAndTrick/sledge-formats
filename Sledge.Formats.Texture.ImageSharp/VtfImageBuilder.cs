using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Sledge.Formats.Texture.ImageSharp.PixelFormats;
using Sledge.Formats.Texture.Vtf;

namespace Sledge.Formats.Texture.ImageSharp;

public class VtfImageBuilder
{
    public VtfImageBuilderOptions Options { get; set; }

    public VtfImageBuilder() : this(new VtfImageBuilderOptions())
    {
        //
    }

    public VtfImageBuilder(VtfImageBuilderOptions options)
    {
        Options = options;
    }

    private static int RoundUpToPowerOfTwo(int value)
    {
        var po2 = 1;
        while (po2 < value) po2 *= 2;
        return po2;
    }

    /// <summary>
    /// Create multiple vtf images using the width and height of the source image.
    /// Frames and mipmaps will be created according to the image builder options.
    /// </summary>
    public IEnumerable<VtfImage> CreateImages(Image image)
    {
        var powWidth = RoundUpToPowerOfTwo(image.Width);
        var powHeight = RoundUpToPowerOfTwo(image.Height);

        if (!Options.AutoResizeToPowerOfTwo && (image.Width != powWidth || image.Height != powHeight))
        {
            throw new InvalidOperationException("Image size is not a power of two and AutoResizeToPowerOfTwo is false, cannot continue.");
        }

        using (image)
        {
            for (var frameNum = 0; frameNum < image.Frames.Count; frameNum++)
            {
                var frame = image.Frames[frameNum];

                // always create the first mip - it's the full-size one
                var mipWidth = powWidth;
                var mipHeight = powHeight;
                var mipIndex = 0;

                while (mipHeight >= Options.MinimumMipmapDimension && mipWidth >= Options.MinimumMipmapDimension)
                {
                    var img = CreateImage(frame, mipWidth, mipHeight);
                    img.Mipmap = mipIndex;
                    img.Frame = frameNum;
                    yield return img;

                    mipWidth /= 2;
                    mipHeight /= 2;
                    mipIndex++;

                    if (!Options.AutoCreateMipmaps) break;
                    if (Options.NumMipmapLevels > 0 && mipIndex >= Options.NumMipmapLevels) break;
                }
            }
        }
    }

    /// <summary>
    /// Create a single vtf image using the width and height of the source image.
    /// </summary>
    public VtfImage CreateImage(Image image) => CreateImage(image, image.Width, image.Height);

    /// <summary>
    /// Create a single vtf image with the given width and height.
    /// </summary>
    public VtfImage CreateImage(Image image, int width, int height)
    {
        var powWidth = RoundUpToPowerOfTwo(width);
        var powHeight = RoundUpToPowerOfTwo(height);

        if (!Options.AutoResizeToPowerOfTwo && (width != powWidth || height != powHeight))
        {
            throw new InvalidOperationException("Image size is not a power of two and AutoResizeToPowerOfTwo is false, cannot continue.");
        }

        return CreateImage(image.Frames[0], powWidth, powHeight);
    }

    private VtfImage CreateImage(ImageFrame frame, int width, int height)
    {
        if ((width & (width - 1)) != 0) throw new ArgumentException("Width must be a power of two.", nameof(width));
        if ((height & (height - 1)) != 0) throw new ArgumentException("Height must be a power of two.", nameof(height));

        using var image = new Image<Rgba32>(frame.Width, frame.Height, new Rgba32(1, 1, 1, 0));
        image.Frames.AddFrame(frame);
        image.Frames.RemoveFrame(0);

        using var resized = image.Clone(context => context.Resize(width, height, Options.Resampler));
        var data = Options.ImageFormat switch
        {
            VtfImageFormat.None => throw new ArgumentException("Can't create an image with a format of None."),
            VtfImageFormat.Rgba8888 => GetImageData<Rgba32>(resized),
            VtfImageFormat.Abgr8888 => GetImageData<Abgr32>(resized),
            VtfImageFormat.Rgb888 => GetImageData<Rgb24>(resized),
            VtfImageFormat.Bgr888 => GetImageData<Bgr24>(resized),
            VtfImageFormat.Rgb565 => GetImageData<Rgb565>(resized),
            VtfImageFormat.I8 => GetImageData<L8>(resized),
            VtfImageFormat.Ia88 => GetImageData<La16>(resized),
            VtfImageFormat.P8 => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            VtfImageFormat.A8 => GetImageData<A8>(resized),
            VtfImageFormat.Rgb888Bluescreen => GetImageData<Rgb24>(resized, Bluescreen),
            VtfImageFormat.Bgr888Bluescreen => GetImageData<Bgr24>(resized, Bluescreen),
            VtfImageFormat.Argb8888 => GetImageData<Argb32>(resized),
            VtfImageFormat.Bgra8888 => GetImageData<Bgra32>(resized),
            VtfImageFormat.Dxt1 => EncodeDxt(resized, CompressionFormat.Bc1),
            VtfImageFormat.Dxt3 => EncodeDxt(resized, CompressionFormat.Bc2),
            VtfImageFormat.Dxt5 => EncodeDxt(resized, CompressionFormat.Bc3),
            VtfImageFormat.Bgrx8888 => GetImageData<Bgra32>(resized, DiscardAlphaChannel),
            VtfImageFormat.Bgr565 => GetImageData<Bgr565>(resized),
            VtfImageFormat.Bgrx5551 => GetImageData<Bgra5551>(resized, DiscardAlphaChannel),
            VtfImageFormat.Bgra4444 => GetImageData<Bgra4444>(resized),
            VtfImageFormat.Dxt1Onebitalpha => EncodeDxt(resized, CompressionFormat.Bc1WithAlpha),
            VtfImageFormat.Bgra5551 => GetImageData<Bgra5551>(resized),
            VtfImageFormat.Uv88 => GetImageData<Rg88>(resized),
            VtfImageFormat.Uvwq8888 => GetImageData<Rgba32>(resized),
            VtfImageFormat.Rgba16161616F => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            VtfImageFormat.Rgba16161616 => GetImageData<Rgba64>(resized),
            VtfImageFormat.Uvlx8888 => GetImageData<Rgba32>(resized),
            VtfImageFormat.R32F => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            VtfImageFormat.Rgb323232F => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            VtfImageFormat.Rgba32323232F => GetImageData<RgbaVector>(resized),
            VtfImageFormat.NvDst16 => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            VtfImageFormat.NvDst24 => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            VtfImageFormat.NvIntz => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            VtfImageFormat.NvRawz => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            VtfImageFormat.AtiDst16 => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            VtfImageFormat.AtiDst24 => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            VtfImageFormat.NvNull => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            VtfImageFormat.Ati2N => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            VtfImageFormat.Ati1N => throw new NotSupportedException($"Format not supported: {Options.ImageFormat}"),
            _ => throw new IndexOutOfRangeException()
        };

        return new VtfImage
        {
            Format = Options.ImageFormat,
            Width = width,
            Height = height,
            Data = data
        };
    }

    private static void Bluescreen(IImageProcessingContext context)
    {
        context.BackgroundColor(Color.Blue);
    }

    private static void DiscardAlphaChannel(IImageProcessingContext context)
    {
        context.ProcessPixelRowsAsVector4(row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                row[i].W = 1;
            }
        });
    }

    private static byte[] GetImageData<T>(Image image, Action<IImageProcessingContext>? transform = null) where T : unmanaged, IPixel<T>
    {
        // depending on the type of T we might need to clone the image to convert it to a different format
        // if we do that we'll need to dispose of it later, so track if we need to
        var dispose = false;

        Image<T> sourceImage;
        if (typeof(T) == typeof(Rgba32))
        {
            // we know that image uses the same pixel type as T so we can do a cast here
            sourceImage = (Image<T>) image;
        }
        else
        {
            sourceImage = image.CloneAs<T>();
            dispose = true;
        }

        // transform the image if required
        if (transform != null)
        {
            var transformedImage = sourceImage.Clone(transform);
            if (dispose) sourceImage.Dispose(); // source image is a clone, dispose of it
            // our transformation is now the source image, we should dispose of it at the end
            sourceImage = transformedImage;
            dispose = true;
        }

        var buffer = new byte[sourceImage.Width * sourceImage.Height * sourceImage.PixelType.BitsPerPixel / 8];
        sourceImage.CopyPixelDataTo(buffer);

        if (dispose) sourceImage.Dispose();
        return buffer;
    }

    private static byte[] EncodeDxt(Image<Rgba32> image, CompressionFormat format)
    {
        var encoder = new BcEncoder(format);
        return encoder.EncodeToRawBytes(image, 0, out _, out _);
    }
}