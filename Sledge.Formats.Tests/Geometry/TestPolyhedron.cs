using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Precision;

namespace Sledge.Formats.Tests.Geometry;

[TestClass]
public class TestPolyhedron
{
    [TestMethod]
    public void TestConstructor()
    {
        // a cube
        var verts = new[]
        {
            new[] { new Vector3(64, 32, 32), new Vector3(64, 32, 16), new Vector3(48, 32, 16) },
            new[] { new Vector3(48, 48, 32), new Vector3(48, 48, 16), new Vector3(64, 48, 16) },
            new[] { new Vector3(48, 32, 32), new Vector3(48, 32, 16), new Vector3(48, 48, 16) },
            new[] { new Vector3(64, 48, 32), new Vector3(64, 48, 16), new Vector3(64, 32, 16) },
            new[] { new Vector3(48, 48, 32), new Vector3(64, 48, 32), new Vector3(64, 32, 32) },
            new[] { new Vector3(64, 48, 16), new Vector3(48, 48, 16), new Vector3(48, 32, 16) },
        };

        var planes = verts.Select(x => Plane.CreateFromVertices(x[2], x[1], x[0]));
        var poly = new Polyhedron(planes);
        Assert.AreEqual(6, poly.Polygons.Count);

        var f1 = poly.Polygons[0];

        Assert.AreEqual(4, f1.Vertices.Count);
        Assert.IsTrue(new Vector3(56, 32, 24).EquivalentTo(f1.Origin), $"new Vector3(56, 32, 24).EquivalentTo({f1.Origin})");
        Assert.AreEqual(-Vector3.UnitY, f1.Plane.Normal);
        Assert.AreEqual(32, f1.Plane.D);
    }
}