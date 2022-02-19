using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Tests
{
    [TestClass]
    public class TestGoldsourceBsp
    {
        private static MemoryStream GetFile(string name)
        {
            var assem = Assembly.GetExecutingAssembly();
            var files = new List<(string name, Stream stream)>();

            name = assem.GetName().Name + ".Resources." + name.Replace('/', '.') + ".gz";

            using var gz = new GZipStream(assem.GetManifestResourceStream(name)!, CompressionMode.Decompress, false);
            var ms = new MemoryStream();
            gz.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
        
        [TestMethod]
        public void TestStandardFormat()
        {
            using var file = GetFile("goldsource/aaa.bsp");
            var bsp = new BspFile(file);
            Assert.AreEqual(false, bsp.Options.UseBlueShiftFormat);
            Assert.AreEqual(5, bsp.GetLump<Entities>().Count);
        }
        
        [TestMethod]
        public void TestStandardFormatWithWrongFlag()
        {
            Assert.ThrowsException<Exception>(() => {
                try
                {
                    using var file = GetFile("goldsource/aaa.bsp");
                    var bsp = new BspFile(file, new BspFileOptions { AutodetectBlueShiftFormat = false, UseBlueShiftFormat = true });
                }
                catch
                {
                    throw new Exception();
                }
            });
        }
        
        [TestMethod]
        public void TestBlueShiftFormat()
        {
            using var file = GetFile("goldsource/ba_hazard6.bsp");
            var bsp = new BspFile(file);
            Assert.AreEqual(true, bsp.Options.UseBlueShiftFormat);
            Assert.AreEqual(173, bsp.GetLump<Entities>().Count);
        }
        
        [TestMethod]
        public void TestBlueShiftFormatWithWrongFlag()
        {
            Assert.ThrowsException<Exception>(() => {
                try
                {
                    using var file = GetFile("goldsource/ba_hazard6.bsp");
                    var bsp = new BspFile(file, new BspFileOptions { AutodetectBlueShiftFormat = false, UseBlueShiftFormat = false });
                }
                catch
                {
                    throw new Exception();
                }
            });
        }
        
        [TestMethod]
        public void TestThatOneFileWithANegativeTextureOffest()
        {
            using var file = GetFile("goldsource/grunts2.bsp");
            var bsp = new BspFile(file);
        }

        [TestMethod]
        public void TestSwitchingBetweenFormats()
        {
            using var file = GetFile("goldsource/aaa.bsp");
            var bsp = new BspFile(file);
            Assert.AreEqual(5, bsp.GetLump<Entities>().Count);

            bsp.Options.UseBlueShiftFormat = false;
            var origStream = new MemoryStream();
            bsp.WriteToStream(origStream, Version.Goldsource);
            origStream.Position = 0;
            var orig = origStream.ToArray();

            bsp.Options.UseBlueShiftFormat = true;
            var bshiftStream = new MemoryStream();
            bsp.WriteToStream(bshiftStream, Version.Goldsource);
            bshiftStream.Position = 0;
            var bshi = bshiftStream.ToArray();
            
            CollectionAssert.AreEqual(orig[..4], bshi[..4]);
            CollectionAssert.AreEqual(orig[4..8], bshi[4..8]); // offset (124 for both)
            CollectionAssert.AreEqual(orig[8..12], bshi[16..20]); // length (swapped)
            Assert.AreEqual(
                BitConverter.ToInt32(orig[12..16]),
                BitConverter.ToInt32(orig[4..8]) + BitConverter.ToInt32(orig[8..12])
            ); // offset (first)
            Assert.AreEqual(
                BitConverter.ToInt32(bshi[12..16]),
                BitConverter.ToInt32(bshi[4..8]) + BitConverter.ToInt32(bshi[8..12])
            ); // offset (second)
            CollectionAssert.AreEqual(orig[16..20], bshi[8..12]); // length (swapped)
            CollectionAssert.AreEqual(orig[20..124], bshi[20..124]); // rest of the header is the same
            Assert.AreEqual(orig.Length, bshi.Length); // length is the same
        }
    }
}