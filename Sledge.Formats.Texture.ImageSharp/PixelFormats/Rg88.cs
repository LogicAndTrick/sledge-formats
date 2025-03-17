using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;

namespace Sledge.Formats.Texture.ImageSharp.PixelFormats;

public struct Rg88 : IPixel<Rg88>, IPackedVector<ushort>
{
    public ushort PackedValue { get; set; }

    public Rg88(Vector3 vector)
    {
        PackedValue = Pack(vector);
    }

    private static ushort Pack(Vector3 vector)
    {
        vector = Vector3.Clamp(vector, Vector3.Zero, Vector3.One);

        var r = (int)(vector.X * byte.MaxValue);
        var g = (int)(vector.Y * byte.MaxValue);

        return (ushort)(g << 8 | r);
    }

    public readonly PixelOperations<Rg88> CreatePixelOperations()
    {
        return new PixelOperations<Rg88>();
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
        var r = (PackedValue & 0xFF);
        var g = (PackedValue & 0xFF00) >> 8;
        return new Vector4(r / (float) byte.MaxValue, g / (float) byte.MaxValue, 1, 1);
    }

    public readonly bool Equals(Rg88 other)
    {
        return PackedValue == other.PackedValue;
    }

    public readonly override bool Equals(object? obj)
    {
        return obj is Rg88 other && Equals(other);
    }

    public readonly override int GetHashCode()
    {
        return PackedValue.GetHashCode();
    }

    public static bool operator ==(Rg88 left, Rg88 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rg88 left, Rg88 right)
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