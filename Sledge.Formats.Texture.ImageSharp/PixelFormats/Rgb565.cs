using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;

namespace Sledge.Formats.Texture.ImageSharp.PixelFormats;

public struct Rgb565 : IPixel<Rgb565>, IPackedVector<ushort>
{
    public ushort PackedValue { get; set; }

    public Rgb565(Vector3 vector)
    {
        PackedValue = Pack(vector);
    }

    private static ushort Pack(Vector3 vector)
    {
        vector = Vector3.Clamp(vector, Vector3.Zero, Vector3.One);

        return (ushort)(
            (((int)Math.Round(vector.Z * 31F) & 0x1F) << 11)
            | (((int)Math.Round(vector.Y * 63F) & 0x3F) << 5)
            | ((int)Math.Round(vector.X * 31F) & 0x1F)
        );
    }

    public readonly Vector3 ToVector3() => new(
        (PackedValue & 0x1F) * (1F / 31F),
       ((PackedValue >> 5) & 0x3F) * (1F / 63F),
       ((PackedValue >> 11) & 0x1F) * (1F / 31F)
    );

    public readonly PixelOperations<Rgb565> CreatePixelOperations()
    {
        return new PixelOperations<Rgb565>();
    }

    public void FromScaledVector4(Vector4 vector)
    {
        FromVector4(vector);
    }

    public readonly Vector4 ToScaledVector4()
    {
        return ToVector4();
    }

    public void FromVector4(Vector4 vector)
    {
        PackedValue = Pack(new Vector3(vector.X, vector.Y, vector.Z));
    }

    public readonly Vector4 ToVector4()
    {
        return new Vector4(ToVector3(), 1);
    }

    public readonly bool Equals(Rgb565 other)
    {
        return PackedValue == other.PackedValue;
    }

    public readonly override bool Equals(object? obj)
    {
        return obj is Rgb565 other && Equals(other);
    }

    public readonly override int GetHashCode()
    {
        return PackedValue.GetHashCode();
    }

    public static bool operator ==(Rgb565 left, Rgb565 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rgb565 left, Rgb565 right)
    {
        return !left.Equals(right);
    }

    public void FromArgb32(Argb32 source) => throw new NotImplementedException();
    public void FromBgra5551(Bgra5551 source) => throw new NotImplementedException();
    public void FromBgr24(Bgr24 source) => throw new NotImplementedException();
    public void FromBgra32(Bgra32 source) => throw new NotImplementedException();
    public void FromAbgr32(Abgr32 source) => throw new NotImplementedException();
    public void FromL8(L8 source) => throw new NotImplementedException();
    public void FromL16(L16 source) => throw new NotImplementedException();
    public void FromLa16(La16 source) => throw new NotImplementedException();
    public void FromLa32(La32 source) => throw new NotImplementedException();
    public void FromRgb24(Rgb24 source) => throw new NotImplementedException();
    public void FromRgba32(Rgba32 source) => throw new NotImplementedException();
    public void ToRgba32(ref Rgba32 dest) => throw new NotImplementedException();
    public void FromRgb48(Rgb48 source) => throw new NotImplementedException();
    public void FromRgba64(Rgba64 source) => throw new NotImplementedException();
}