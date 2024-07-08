using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Geometric;

namespace Sledge.Formats.Tests.Geometry;

[TestClass]
public class TestBox
{
    [TestMethod]
    public void TestConstructors()
    {
        var a = new Vector3(-10, -20, -30);
        var b = new Vector3(10, 20, 30);
        var box = new Box(a, b);
        Assert.AreEqual(a, box.Start);
        Assert.AreEqual(b, box.End);
        Assert.AreEqual(box, new Box(a with { X = b.X }, b with { X = a.X }));
        Assert.AreEqual(box, new Box(a with { Y = b.Y }, b with { Y = a.Y }));
        Assert.AreEqual(box, new Box(a with { Z = b.Z }, b with { Z = a.Z }));
        Assert.AreEqual(box, new Box(new[] { box, box, box }));

        Assert.AreEqual(Vector3.Zero, box.Center);
        Assert.AreEqual(b.X * 2, box.Width);
        Assert.AreEqual(b.Y * 2, box.Length);
        Assert.AreEqual(b.Z * 2, box.Height);
        Assert.AreEqual(b.X * 2, box.SmallestDimension);
        Assert.AreEqual(b.Z * 2, box.LargestDimension);
        Assert.AreEqual(b * 2, box.Dimensions);
    }

    [TestMethod]
    public void TestIsEmpty()
    {
        Assert.IsTrue(Box.Empty.IsEmpty());
        Assert.IsTrue(new Box(Vector3.One, Vector3.One).IsEmpty());
        Assert.IsFalse(new Box(Vector3.One, Vector3.One * 2).IsEmpty());
        Assert.IsFalse(new Box(Vector3.One, Vector3.One * 2).IsEmpty(1));
        Assert.IsTrue(new Box(Vector3.One, Vector3.One * 2).IsEmpty(1.01f));
    }

    [TestMethod]
    public void TestGetBoxPoints()
    {
        var a = new Vector3(-10, -20, -30);
        var b = new Vector3(10, 20, 30);
        var box = new Box(a, b);
        var expected = new[]
        {
            a, b,
            a with { X = b.X },
            a with { Y = b.Y },
            a with { Z = b.Z },
            b with { X = a.X },
            b with { Y = a.Y },
            b with { Z = a.Z },
        };
        CollectionAssert.AreEquivalent(expected, box.GetBoxPoints().ToList());
    }

    [TestMethod]
    public void TestGetBoxPlanes()
    {
        var a = new Vector3(-10, -20, -30);
        var b = new Vector3(10, 20, 30);
        var box = new Box(a, b);
        var expected = new[]
        {
            new Plane(-Vector3.UnitX, -b.X),
            new Plane(-Vector3.UnitY, -b.Y),
            new Plane(-Vector3.UnitZ, -b.Z),
            new Plane(Vector3.UnitX, a.X),
            new Plane(Vector3.UnitY, a.Y),
            new Plane(Vector3.UnitZ, a.Z),
        };
        CollectionAssert.AreEquivalent(expected, box.GetBoxPlanes().ToList());
    }
}