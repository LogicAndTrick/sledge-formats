using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Sledge.Formats.Texture.ImageSharp.PixelFormats;

namespace Sledge.Formats.Texture.Tests.Vtf;

[TestClass]
public class TestPixelFormats
{
    // sanity check for imagesharp native impl
    [TestMethod]
    public void TestBgr565ImageSharpNative()
    {
        var pixel8888 = Rgba32.ParseHex("123456");
        var expectedData = new byte[] { 0xAA, 0x11 };

        using var src = new Image<Rgba32>(1, 1, pixel8888);
        using var con = src.CloneAs<Bgr565>();

        var spn = new byte[2];
        con.CopyPixelDataTo(spn);
        CollectionAssert.AreEqual(expectedData, spn);
    }

    [TestMethod]
    public void TestRgb565CustomFormat()
    {
        var pixel8888 = Rgba32.ParseHex("123456");
        var expectedData = new byte[] { 0xA2, 0x51 };

        using var src = new Image<Rgba32>(1, 1, pixel8888);
        using var con = src.CloneAs<Rgb565>();

        var spn = new byte[2];
        con.CopyPixelDataTo(spn);
        CollectionAssert.AreEqual(expectedData, spn);
    }

    [TestMethod]
    public void TestRg88CustomFormat()
    {
        var pixel8888 = Rgba32.ParseHex("123456");
        var expectedData = new byte[] { 0x12, 0x34 };

        using var src = new Image<Rgba32>(1, 1, pixel8888);
        src.SaveAsPng(@"D:\Downloads\test.png");
        using var con = src.CloneAs<Rg88>();

        var spn = new byte[2];
        con.CopyPixelDataTo(spn);
        CollectionAssert.AreEqual(expectedData, spn);
    }
}