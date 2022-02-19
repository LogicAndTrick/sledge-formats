using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Bsp.Lumps;
using Sledge.Formats.Bsp.Objects;

namespace Sledge.Formats.Bsp.Tests
{
    [TestClass]
    public class TestEntitiesLump
    {
        [TestMethod]
        public void TestBasicEntities()
        {
            var lump = new Entities();
            lump.Add(new Entity
            {
                ClassName = "test",
                Model = 1,
                KeyValues =
                {
                    { "test", "123" }
                }
            });
            Assert.AreEqual(3, lump[0].KeyValues.Count);

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            lump.Write(bw, Version.Goldsource);

            ms.Position = 0;
            using var br = new BinaryReader(ms);
            var lump2 = new Entities();
            lump2.Read(br, new Blob { Offset = 0, Length = (int) ms.Length }, Version.Goldsource);

            Assert.AreEqual(1, lump2.Count);
            Assert.AreEqual(3, lump2[0].KeyValues.Count);
            Assert.AreEqual("test", lump2[0].Get("classname", ""));
            Assert.AreEqual("123", lump2[0].Get("test", ""));
            Assert.AreEqual("*1", lump2[0].Get("model", ""));
            Assert.AreEqual("test", lump2[0].ClassName);
            Assert.AreEqual(1, lump2[0].Model);
        }
    }
}
