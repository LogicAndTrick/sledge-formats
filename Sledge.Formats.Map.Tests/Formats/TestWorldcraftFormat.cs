using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Map.Formats;
using Sledge.Formats.Map.Objects;
using Path = System.IO.Path;

namespace Sledge.Formats.Map.Tests.Formats;

[TestClass]
public class TestWorldcraftFormat
{
    [DataRow("08")]
    [DataRow("09")]
    [DataRow("14")]
    [DataRow("16")]
    [DataRow("18")]
    [DataRow("22")]
    [DataTestMethod]
    public void TestRmfVersions(string version)
    {
        using var file = typeof(TestWorldcraftFormat).Assembly.GetManifestResourceStream($"Sledge.Formats.Map.Tests.Resources.rmf.{version}.rmf");
        var format = new WorldcraftRmfFormat();
        var map = format.Read(file);
        Assert.AreEqual(0, map.Paths.Count);
        Assert.AreEqual(1, map.Worldspawn.FindAll().OfType<Group>().Count());
        Assert.AreEqual(2, map.Worldspawn.FindAll().OfType<Entity>().Count(x => x is not Worldspawn));
        Assert.AreEqual(3, map.Worldspawn.FindAll().OfType<Solid>().Count());
        Assert.AreEqual(2, map.Visgroups.Count);
    }

    [TestMethod]
    public void TestRmf08Visgroups()
    {
        using var file = typeof(TestWorldcraftFormat).Assembly.GetManifestResourceStream($"Sledge.Formats.Map.Tests.Resources.rmf.08_visgroups.rmf");
        var format = new WorldcraftRmfFormat();
        var map = format.Read(file);
    }
}