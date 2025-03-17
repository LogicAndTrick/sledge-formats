using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Map.Formats;
using Sledge.Formats.Map.Objects;

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

    [TestMethod]
    public void TestRmfQuakeToValveTextureConversion()
    {
        var format = new WorldcraftRmfFormat();

        using var file16 = typeof(TestWorldcraftFormat).Assembly.GetManifestResourceStream($"Sledge.Formats.Map.Tests.Resources.rmf.test-cube-1.6.rmf");
        using var file22 = typeof(TestWorldcraftFormat).Assembly.GetManifestResourceStream($"Sledge.Formats.Map.Tests.Resources.rmf.test-cube-2.2.rmf");

        var map16 = format.Read(file16);
        var map22 = format.Read(file22);

        var solid16 = (Solid)map16.Worldspawn.Children[0];
        var solid22 = (Solid)map22.Worldspawn.Children[0];

        foreach (var face22 in solid22.Faces)
        {
            var face16 = solid16.Faces.Single(x => x.Plane.Equals(face22.Plane));

            Assert.AreEqual(face22.UAxis, face16.UAxis);
            Assert.AreEqual(face22.VAxis, face16.VAxis);
            Assert.AreEqual(face22.XScale, face16.XScale);
            Assert.AreEqual(face22.YScale, face16.YScale);
            Assert.AreEqual(face22.XShift, face16.XShift);
            Assert.AreEqual(face22.YShift, face16.YShift);
            Assert.AreEqual(face22.Rotation, face16.Rotation);
            Assert.AreEqual(face22.ContentFlags, face16.ContentFlags);
            Assert.AreEqual(face22.SurfaceFlags, face16.SurfaceFlags);
            Assert.AreEqual(face22.Value, face16.Value);
            Assert.AreEqual(face22.LightmapScale, face16.LightmapScale);
            Assert.AreEqual(face22.SmoothingGroups, face16.SmoothingGroups);
        }
    }
}