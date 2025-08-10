using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Map.Formats;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Tests.Formats;

[TestClass]
public class TestJackhammerFormat
{
    [TestMethod]
    public void TestGroupInEntity()
    {
        const string solidDef = @"
        {
            {
            ( -48 -64 12 ) ( -48 -63 12 ) ( -48 -64 13 ) dev_64e [ 0 -1 0 0 ] [ 0 0 -1 -4 ] 0 1 1
            ( -48 -96 12 ) ( -48 -96 13 ) ( -47 -96 12 ) dev_64e [ 1 0 0 0 ] [ 0 0 -1 -4 ] 0 1 1
            ( -48 -64 8 ) ( -47 -64 8 ) ( -48 -63 8 ) dev_64e [ -1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1
            ( 64 32 16 ) ( 64 33 16 ) ( 65 32 16 ) dev_64e [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1
            ( 64 32 28 ) ( 65 32 28 ) ( 64 32 29 ) dev_64e [ -1 0 0 0 ] [ 0 0 -1 -4 ] 0 1 1
            ( 64 32 28 ) ( 64 32 29 ) ( 64 33 28 ) dev_64e [ 0 1 0 0 ] [ 0 0 -1 -4 ] 0 1 1
            }
        }";

        var qmf = new QuakeMapFormat();
        using var qms = new MemoryStream(Encoding.ASCII.GetBytes(solidDef));
        var qmap = qmf.Read(qms);
        var sourceSolid = qmap.Worldspawn.FindAll().OfType<Solid>().Single();

        // add an entity with a group inside it, with a solid inside that
        var inputMap = new MapFile();
        inputMap.Worldspawn.Children.Add(new Entity
        {
            ClassName = "ent_test",
            Children =
            {
                new Group
                {
                    Children =
                    {
                        sourceSolid
                    }
                }
            }
        });

        // we should expect the group to be dropped since JMF doesn't support groups inside (non-worldspawn) entities
        var format = new JackhammerJmfFormat();
        var outputStream = new MemoryStream();
        format.Write(outputStream, inputMap, "122");
        outputStream.Position = 0;
        var outputMap = format.Read(outputStream);

        Assert.AreEqual(1, outputMap.Worldspawn.Children.Count);
        Assert.IsInstanceOfType(outputMap.Worldspawn.Children[0], typeof(Entity));

        var ent = (Entity)outputMap.Worldspawn.Children[0];
        Assert.AreEqual(1, ent.Children.Count);
        Assert.AreEqual("ent_test", ent.ClassName);
        Assert.IsInstanceOfType(ent.Children[0], typeof(Solid));

        Assert.IsTrue(TestUtils.AreEqualSolid(sourceSolid, (Solid)ent.Children[0]));
    }

    [TestMethod]
    public void TestGroupInWorldspawn()
    {
        const string solidDef = @"
        {
            {
            ( -48 -64 12 ) ( -48 -63 12 ) ( -48 -64 13 ) dev_64e [ 0 -1 0 0 ] [ 0 0 -1 -4 ] 0 1 1
            ( -48 -96 12 ) ( -48 -96 13 ) ( -47 -96 12 ) dev_64e [ 1 0 0 0 ] [ 0 0 -1 -4 ] 0 1 1
            ( -48 -64 8 ) ( -47 -64 8 ) ( -48 -63 8 ) dev_64e [ -1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1
            ( 64 32 16 ) ( 64 33 16 ) ( 65 32 16 ) dev_64e [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1
            ( 64 32 28 ) ( 65 32 28 ) ( 64 32 29 ) dev_64e [ -1 0 0 0 ] [ 0 0 -1 -4 ] 0 1 1
            ( 64 32 28 ) ( 64 32 29 ) ( 64 33 28 ) dev_64e [ 0 1 0 0 ] [ 0 0 -1 -4 ] 0 1 1
            }
        }";

        var qmf = new QuakeMapFormat();
        using var qms = new MemoryStream(Encoding.ASCII.GetBytes(solidDef));
        var qmap = qmf.Read(qms);
        var sourceSolid = qmap.Worldspawn.FindAll().OfType<Solid>().Single();

        // add a group with a solid inside it
        var inputMap = new MapFile();
        inputMap.Worldspawn.Children.Add(new Group
        {
            Children =
            {
                sourceSolid
            }
        });

        var format = new JackhammerJmfFormat();
        var outputStream = new MemoryStream();
        format.Write(outputStream, inputMap, "122");
        outputStream.Position = 0;
        var outputMap = format.Read(outputStream);

        Assert.AreEqual(1, outputMap.Worldspawn.Children.Count);
        Assert.IsInstanceOfType(outputMap.Worldspawn.Children[0], typeof(Group));

        var group = (Group)outputMap.Worldspawn.Children[0];
        Assert.AreEqual(1, group.Children.Count);
        Assert.IsInstanceOfType(group.Children[0], typeof(Solid));

        Assert.IsTrue(TestUtils.AreEqualMap(inputMap, outputMap));
    }

    [TestMethod]
    public void TestJmf121()
    {
        using var file = typeof(TestJackhammerFormat).Assembly.GetManifestResourceStream("Sledge.Formats.Map.Tests.Resources.jmf.default-room-121.jmf");
        var format = new JackhammerJmfFormat();
        var map = format.Read(file);
        Assert.AreEqual("worldspawn", map.Worldspawn.ClassName);
    }

    [TestMethod]
    public void TestJmf122()
    {
        using var file = typeof(TestJackhammerFormat).Assembly.GetManifestResourceStream("Sledge.Formats.Map.Tests.Resources.jmf.default-room-122.jmf");
        var format = new JackhammerJmfFormat();
        var map = format.Read(file);
        Assert.AreEqual("worldspawn", map.Worldspawn.ClassName);
        Assert.AreEqual(3, map.BackgroundImages.Count);

        var front = map.BackgroundImages.Single(x => x.Viewport == ViewportType.OrthographicFront);
        Assert.AreEqual("C:/Test/Viewport.png", front.Path);
        Assert.IsTrue(Math.Abs(front.Scale - 2.5) < 0.0001);
        Assert.AreEqual(175, front.Luminance);
        Assert.AreEqual(FilterMode.Linear, front.Filter);
        Assert.AreEqual(true, front.InvertColours);
        Assert.AreEqual(new Vector2(6, -7), front.Offset);
    }

    [TestMethod]
    [DataRow("default-room-121", "121")]
    [DataRow("default-room-121")]
    [DataRow("default-room-122")]
    public void TestRoundTrip(string filename, string styleHint = "122")
    {
        using var inputStream = typeof(TestJackhammerFormat).Assembly.GetManifestResourceStream($"Sledge.Formats.Map.Tests.Resources.jmf.{filename}.jmf");
        var format = new JackhammerJmfFormat();
        var inputMap = format.Read(inputStream);
        var outputStream = new MemoryStream();
        format.Write(outputStream, inputMap, styleHint);
        outputStream.Position = 0;
        var outputMap = format.Read(outputStream);
        Assert.IsTrue(TestUtils.AreEqualMap(inputMap, outputMap));
    }
}