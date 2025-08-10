using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Texture.Vtf;
using Sledge.Formats.Texture.Vtf.Resources;

namespace Sledge.Formats.Texture.Tests.Vtf
{
    [TestClass]
    public class TestVtf
    {
        [TestMethod]
        public void TestRoundTrip72()
        {
            var create = new VtfFile();
            create.AddImage(new VtfImage
            {
                Format = VtfImageFormat.Rgba8888,
                Width = 1,
                Height = 1,
                Mipmap = 0,
                Frame = 0,
                Face = 0,
                Slice = 0,
                Data = new byte[]
                {
                    0x11, 0x22, 0x33, 0xFF
                }
            });

            using var ms = new MemoryStream();
            create.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);

            var read = new VtfFile(ms);
            Assert.AreEqual(create.Images.Count, read.Images.Count);
            Assert.AreEqual(create.Images[0].Width, read.Images[0].Width);
            Assert.AreEqual(create.Images[0].Height, read.Images[0].Height);
            Assert.AreEqual(create.Images[0].Mipmap, read.Images[0].Mipmap);
            Assert.AreEqual(create.Images[0].Frame, read.Images[0].Frame);
            Assert.AreEqual(create.Images[0].Face, read.Images[0].Face);
            Assert.AreEqual(create.Images[0].Slice, read.Images[0].Slice);
            Assert.AreEqual(create.Images[0].Data.Length, read.Images[0].Data.Length);
            CollectionAssert.AreEqual(create.Images[0].Data, read.Images[0].Data);
        }
        [TestMethod]
        public void TestRoundTrip73()
        {
            var create = new VtfFile(7.3m);
            create.AddImage(new VtfImage
            {
                Format = VtfImageFormat.Rgba8888,
                Width = 1,
                Height = 1,
                Mipmap = 0,
                Frame = 0,
                Face = 0,
                Slice = 0,
                Data = new byte[]
                {
                    0x11, 0x22, 0x33, 0xFF
                }
            });
            create.AddResource(new VtfValueResource { Type = VtfResourceType.Crc, Value = 0x12345 });

            using var ms = new MemoryStream();
            create.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);

            var read = new VtfFile(ms);
            Assert.AreEqual(create.Images.Count, read.Images.Count);
            Assert.AreEqual(create.Images[0].Width, read.Images[0].Width);
            Assert.AreEqual(create.Images[0].Height, read.Images[0].Height);
            Assert.AreEqual(create.Images[0].Mipmap, read.Images[0].Mipmap);
            Assert.AreEqual(create.Images[0].Frame, read.Images[0].Frame);
            Assert.AreEqual(create.Images[0].Face, read.Images[0].Face);
            Assert.AreEqual(create.Images[0].Slice, read.Images[0].Slice);
            Assert.AreEqual(create.Images[0].Data.Length, read.Images[0].Data.Length);
            CollectionAssert.AreEqual(create.Images[0].Data, read.Images[0].Data);

            Assert.AreEqual(create.Resources.Count, read.Resources.Count);
            Assert.AreEqual(create.Resources[0].Type, read.Resources[0].Type);
            Assert.IsInstanceOfType(read.Resources[0], typeof(VtfValueResource));
            Assert.AreEqual(((VtfValueResource)create.Resources[0]).Value, ((VtfValueResource)read.Resources[0]).Value);
        }
    }
}