using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Geometric.Precision;

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
            new[] { new Vector3d(64, 32, 32), new Vector3d(64, 32, 16), new Vector3d(48, 32, 16) },
            new[] { new Vector3d(48, 48, 32), new Vector3d(48, 48, 16), new Vector3d(64, 48, 16) },
            new[] { new Vector3d(48, 32, 32), new Vector3d(48, 32, 16), new Vector3d(48, 48, 16) },
            new[] { new Vector3d(64, 48, 32), new Vector3d(64, 48, 16), new Vector3d(64, 32, 16) },
            new[] { new Vector3d(48, 48, 32), new Vector3d(64, 48, 32), new Vector3d(64, 32, 32) },
            new[] { new Vector3d(64, 48, 16), new Vector3d(48, 48, 16), new Vector3d(48, 32, 16) },
        };

        var planes = verts.Select(x => Planed.CreateFromVertices(x[2], x[1], x[0]));
        var poly = new Polyhedrond(planes);
        Assert.AreEqual(6, poly.Polygons.Count);

        var f1 = poly.Polygons[0];

        Assert.AreEqual(4, f1.Vertices.Count);
        Assert.IsTrue(new Vector3d(56, 32, 24).EquivalentTo(f1.Origin), $"new Vector3(56, 32, 24).EquivalentTo({f1.Origin})");
        Assert.AreEqual(-Vector3d.UnitY, f1.Plane.Normal);
        Assert.AreEqual(32, f1.Plane.D);
    }

    [TestMethod]
    public void TestStandardPrecision()
    {
        // a brush that was causing me some issues
        var verts = new[]
        {
            new[] { new Vector3(-61, 409, -240), new Vector3(-64, 402, -240), new Vector3(-41, 382, -240) },
            new[] { new Vector3(-64, 402, -204), new Vector3(-61, 409, -204), new Vector3(-35, 388, -204) },
            new[] { new Vector3(-64, 402, -240), new Vector3(-61, 409, -240), new Vector3(-61, 409, -204) },
            new[] { new Vector3(-35, 388, -240), new Vector3(-41, 382, -240), new Vector3(-41, 382, -204) },
            new[] { new Vector3(-61, 409, -240), new Vector3(-35, 388, -240), new Vector3(-35, 388, -204) },
            new[] { new Vector3(-41, 382, -240), new Vector3(-64, 402, -240), new Vector3(-64, 402, -204) },
        };

        var planes = verts.Select(x => Plane.CreateFromVertices(x[2], x[1], x[0]));
        var polyh = new Polyhedrond(planes);
        Assert.AreEqual(6, polyh.Polygons.Count);

        foreach (var polyg in polyh.Polygons)
        {
            // each point in the polygon should have 2 other matching points on other polygons in the solid, for a total of 3
            foreach (var v in polyg.Vertices)
            {
                var matching = polyh.Polygons
                    .SelectMany(x => x.Vertices)
                    .Where(x => x.EquivalentTo(v))
                    .ToList();
                Assert.AreEqual(3, matching.Count, $"Not enough matching points for plane ({polyg.Vertices[0]}, {polyg.Vertices[1]}, {polyg.Vertices[2]})");
            }
        }
    }

    [TestMethod]
    public void TestStandardPrecision_SingleSplit()
    {
        var verts = new[]
        {
            // 3 faces which are intersecting oddly
            new [] { new Vector3(-35, 388, -204), new Vector3(-35, 388, -240), new Vector3(-61, 409, -240), new Vector3(-61, 409, -204) },
            new [] { new Vector3(-35, 388, -204), new Vector3(-61, 409, -204), new Vector3(-64, 402, -204), new Vector3(-41, 382, -204) },
            new [] { new Vector3(-41, 382, -204), new Vector3(-41, 382, -240), new Vector3(-35, 388, -240), new Vector3(-35, 388, -204) },
            // the other 3 faces
            new [] { new Vector3(-41, 382, -240), new Vector3(-64, 402, -240), new Vector3(-61, 409, -240), new Vector3(-35, 388, -240) },
            new [] { new Vector3(-61, 409, -204), new Vector3(-61, 409, -240), new Vector3(-64, 402, -240), new Vector3(-64, 402, -204) },
            new [] { new Vector3(-64, 402, -204), new Vector3(-64, 402, -240), new Vector3(-41, 382, -240), new Vector3(-41, 382, -204) },
        };

        var planes = verts.Select(x => Planed.CreateFromVertices(x[0], x[1], x[2])).ToList();
        var poly = new Polygond(planes[0]);

        poly.Split(planes[1], out poly, out _);
        poly.Split(planes[2], out poly, out _);
        poly.Split(planes[3], out poly, out _);
        poly.Split(planes[4], out poly, out _);
        poly.Split(planes[5], out poly, out _);

        foreach (var v in poly.Vertices)
        {
            var match = verts[0].FirstOrDefault(x => x.EquivalentTo(v));
            Assert.IsTrue(match.EquivalentTo(v), $"Cannot find point in original face that's equivalent to {v}");
        }
    }
}