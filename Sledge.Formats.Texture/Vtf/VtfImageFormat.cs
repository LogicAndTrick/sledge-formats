namespace Sledge.Formats.Texture.Vtf
{
    // comments from https://developer.valvesoftware.com/wiki/VTF_(Valve_Texture_Format)
    public enum VtfImageFormat
    {
        None = -1,

        /// <summary>Uncompressed texture with 8-bit alpha</summary>
        Rgba8888 = 0,

        /// <summary>Uncompressed texture with 8-bit alpha</summary>
        Abgr8888,

        /// <summary>Uncompressed opaque texture, full color depth</summary>
        Rgb888,

        /// <summary>Uncompressed opaque texture, full color depth</summary>
        Bgr888,

        /// <summary>Uncompressed texture, limited color depth, similar to Bgr565. Not properly supported in all branches; prefer Bgr565 instead, which always works.</summary>
        Rgb565,

        /// <summary>Luminance (Grayscale), no alpha</summary>
        I8,

        /// <summary>Luminance (Grayscale), 8-bit alpha</summary>
        Ia88,

        /// <summary>256-color paletted</summary>
        P8,

        /// <summary>No color (fully black), 8-bit alpha</summary>
        A8,

        /// <summary>Same as Bgr888, but blue pixels (hex color #0000ff) are rendered transparent instead.</summary>
        Rgb888Bluescreen,

        /// <summary>Same as Rgb888, but blue pixels (hex color #0000ff) are rendered transparent instead.</summary>
        Bgr888Bluescreen,

        /// <summary>Uncompressed texture with 8-bit alpha</summary>
        Argb8888,

        /// <summary>Compressed HDR texture with no alpha or uncompressed SDR texture with 8-bit alpha</summary>
        Bgra8888,

        /// <summary>Standard compression, optional 1-bit alpha (recommended for opaque).</summary>
        Dxt1,

        /// <summary>Standard compression, uninterpolated 4-bit Alpha</summary>
        Dxt3,

        /// <summary>Standard compression, interpolated 8-bit alpha (recommended for transparent/translucent)</summary>
        Dxt5,

        /// <summary>Like Bgra8888, but the alpha channel is always set to 255, making it functionally equivalent to Bgr888.</summary>
        Bgrx8888,

        /// <summary>Uncompressed opaque texture, limited color depth</summary>
        Bgr565,

        /// <summary>Like Bgra5551, but the alpha channel is always set to 255.</summary>
        Bgrx5551,

        /// <summary>Uncompressed texture with alpha, half color depth</summary>
        Bgra4444,

        /// <summary>Dxt1Onebitalpha format does not properly work; use regular Dxt1 with 1-bit alpha flag enabled instead.</summary>
        Dxt1Onebitalpha,

        /// <summary>Uncompressed texture, limited color depth, 1-bit alpha</summary>
        Bgra5551,

        /// <summary>Uncompressed du/dv Format</summary>
        Uv88,

        /// <summary>??</summary>
        Uvwq8888,

        /// <summary>Floating Point HDR Format</summary>
        Rgba16161616F,

        /// <summary>Integer HDR Format</summary>
        Rgba16161616,

        /// <summary>??</summary>
        Uvlx8888,

        /// <summary>??</summary>
        R32F,

        /// <summary>??</summary>
        Rgb323232F,

        /// <summary>??</summary>
        Rgba32323232F,

        /// <summary>??</summary>
        NvDst16,

        /// <summary>??</summary>
        NvDst24,

        /// <summary>??</summary>
        NvIntz,

        /// <summary>??</summary>
        NvRawz,

        /// <summary>??</summary>
        AtiDst16,

        /// <summary>??</summary>
        AtiDst24,

        /// <summary>??</summary>
        NvNull,

        /// <summary>??</summary>
        Ati2N,

        /// <summary>??</summary>
        Ati1N,
    }

}
