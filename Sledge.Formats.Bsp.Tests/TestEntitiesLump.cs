using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Tests;

[TestClass]
public class TestEntitiesLump
{
    [TestMethod]
    public void TestBasicEntities()
    {
        var lump = new Entities
        {
            new()
            {
                ClassName = "test",
                Model = 1,
                SortedKeyValues =
                {
                    new KeyValuePair<string, string>("test", "123")
                }
            }
        };
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

    [TestMethod]
    public void TestEntitiesKeyOrdering()
    {
        var lump = new Entities
        {
            new()
            {
                SortedKeyValues =
                {
                    new KeyValuePair<string, string>("test1", "123"),
                    new KeyValuePair<string, string>("test2", "456"),
                    new KeyValuePair<string, string>("test3", "789")
                }
            }
        };
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
        Assert.AreEqual("test1", lump2[0].SortedKeyValues[0].Key);
        Assert.AreEqual("123", lump2[0].SortedKeyValues[0].Value);
        Assert.AreEqual("test2", lump2[0].SortedKeyValues[1].Key);
        Assert.AreEqual("456", lump2[0].SortedKeyValues[1].Value);
        Assert.AreEqual("test3", lump2[0].SortedKeyValues[2].Key);
        Assert.AreEqual("789", lump2[0].SortedKeyValues[2].Value);
    }

    [TestMethod]
    public void TestEntitiesWithComments()
    {
        var data = """
                   // comment1
                   {
                       "test1" "123"
                       // "test2" "456"
                       "com//ment" "val//ue"
                   }
                   // comment2
                   """;
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.WriteFixedLengthString(Encoding.ASCII, data.Length, data);

        ms.Position = 0;
        using var br = new BinaryReader(ms);
        var lump2 = new Entities();
        lump2.Read(br, new Blob { Offset = 0, Length = (int)ms.Length }, Version.Goldsource);

        Assert.AreEqual(1, lump2.Count);
        Assert.AreEqual(2, lump2[0].KeyValues.Count);
        Assert.AreEqual("test1", lump2[0].SortedKeyValues[0].Key);
        Assert.AreEqual("123", lump2[0].SortedKeyValues[0].Value);
        Assert.AreEqual("com//ment", lump2[0].SortedKeyValues[1].Key);
        Assert.AreEqual("val//ue", lump2[0].SortedKeyValues[1].Value);
    }

    [TestMethod]
    public void TestEntitiesWithDoubleSlashInKeyValue()
    {
        var lump = new Entities
        {
            new()
            {
                SortedKeyValues =
                {
                    new KeyValuePair<string, string>("a//b", "c//d")
                }
            }
        };
        Assert.AreEqual(1, lump[0].KeyValues.Count);

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        lump.Write(bw, Version.Goldsource);

        ms.Position = 0;
        using var br = new BinaryReader(ms);
        var lump2 = new Entities();
        lump2.Read(br, new Blob { Offset = 0, Length = (int)ms.Length }, Version.Goldsource);

        Assert.AreEqual(1, lump2.Count);
        Assert.AreEqual(1, lump2[0].KeyValues.Count);
        Assert.AreEqual("a//b", lump2[0].SortedKeyValues[0].Key);
        Assert.AreEqual("c//d", lump2[0].SortedKeyValues[0].Value);
    }
}