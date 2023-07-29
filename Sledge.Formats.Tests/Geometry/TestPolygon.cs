using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Geometric;
using Sledge.Formats.Geometric.Precision;

namespace Sledge.Formats.Tests.Geometry;

[TestClass]
public class TestPolygon
{
    [TestMethod]
    public void TestCreateForPlane()
    {
        var normal = Vector3.UnitZ;
        var dist = 160;
        var plane = new Plane(normal, -dist);
        var polygon = new Polygon(plane, 9000);

        Assert.AreEqual(new Vector3(-9000, 9000, 160), polygon.Vertices[0]);
        Assert.AreEqual(new Vector3(-9000, -9000, 160), polygon.Vertices[1]);
        Assert.AreEqual(new Vector3(9000, -9000, 160), polygon.Vertices[2]);
        Assert.AreEqual(new Vector3(9000, 9000, 160), polygon.Vertices[3]);
    }

    [TestMethod]
    public void TestCreateForPlaned()
    {
        var normal = Vector3.UnitZ;
        var dist = 160;
        var plane = new Plane(normal, -dist);
        var polygon = new Polygond(plane, 9000);

        Assert.AreEqual(new Vector3d(-9000, 9000, 160), polygon.Vertices[0]);
        Assert.AreEqual(new Vector3d(-9000, -9000, 160), polygon.Vertices[1]);
        Assert.AreEqual(new Vector3d(9000, -9000, 160), polygon.Vertices[2]);
        Assert.AreEqual(new Vector3d(9000, 9000, 160), polygon.Vertices[3]);
    }
}