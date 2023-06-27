﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Precision;

namespace Sledge.Formats.Tests.Geometry;

[TestClass]
public class TestPlane
{
    [TestMethod]
    public void TestPlaneConstructor()
    {
        var v1 = new Vector3(1, 1, 1);
        var v2 = new Vector3(1, 1, 5);
        var v3 = new Vector3(1, 5, 5);

        var actual = Plane.CreateFromVertices(v1, v2, v3);
        Assert.AreEqual(new Vector3(-1, 0, 0), actual.Normal);
        Assert.AreEqual(1, actual.D);
    }

    [TestMethod]
    public void TestOnPlane()
    {
        var v1 = new Vector3(1, 1, 1);
        var v2 = new Vector3(1, 1, 5);
        var v3 = new Vector3(1, 5, 5);
        var plane = Plane.CreateFromVertices(v1, v2, v3);

        Assert.AreEqual(-1, plane.OnPlane(new Vector3(10, 11, 12)));
        Assert.AreEqual(1, plane.OnPlane(new Vector3(-10, -11, -12)));
        Assert.AreEqual(0, plane.OnPlane(v1));
    }

    [TestMethod]
    public void TestGetIntersectionPoint()
    {
        var v1 = new Vector3(1, 1, 1);
        var v2 = new Vector3(1, 1, 5);
        var v3 = new Vector3(1, 5, 5);
        var plane = Plane.CreateFromVertices(v1, v2, v3);

        var l1 = new Vector3(-10, 3, 3);
        var l2 = new Vector3(10, 3, 3);

        Assert.AreEqual(new Vector3(1, 3, 3), plane.GetIntersectionPoint(l1, l2, true, true)!.Value);
        Assert.IsTrue(new Vector3(1, 3, 3).EquivalentTo(plane.GetIntersectionPoint(l1, l2, true, true)!.Value));
        Assert.IsNull(plane.GetIntersectionPoint(l1, l2, false, true));
        Assert.IsNotNull(plane.GetIntersectionPoint(l2, l1, false, true));
    }

    [TestMethod]
    public void TestProject()
    {
        var v1 = new Vector3(1, 1, 1);
        var v2 = new Vector3(1, 1, 5);
        var v3 = new Vector3(1, 5, 5);
        var plane = Plane.CreateFromVertices(v1, v2, v3);

        Assert.AreEqual(new Vector3(1, 10, 5), plane.Project(new Vector3(20, 10, 5)));
    }

    [TestMethod]
    public void TestGetClosestAxisToNormal()
    {
        var v1 = new Vector3(1, 1, 1);
        var v2 = new Vector3(1, 1, 5);
        var v3 = new Vector3(1, 5, 5);
        var plane = Plane.CreateFromVertices(v1, v2, v3);

        Assert.AreEqual(Vector3.UnitX, plane.GetClosestAxisToNormal());
    }

    [TestMethod]
    public void TestIntersect()
    {
        var p1 = Plane.CreateFromVertices(new Vector3(1, 1, 1), new Vector3(2, 1, 1), new Vector3(2, 1, 2));
        var p2 = Plane.CreateFromVertices(new Vector3(1, 1, 1), new Vector3(1, 2, 1), new Vector3(1, 2, 2));
        var p3 = Plane.CreateFromVertices(new Vector3(1, 1, 1), new Vector3(2, 1, 1), new Vector3(2, 2, 1));
        var actual = Plane.Intersect(p1, p2, p3);

        Assert.IsNotNull(actual);
        Assert.AreEqual(new Vector3(1, 1, 1), actual);
    }

    [TestMethod]
    public void TestPlaneFromVertices()
    {
        var v1 = new Vector3(1, 1, 1);
        var v2 = new Vector3(1, 1, 5);
        var v3 = new Vector3(1, 5, 5);

        var actual = Plane.CreateFromVertices(v1, v2, v3);
        var expected = System.Numerics.Plane.CreateFromVertices(v1.ToStandardVector3(), v2.ToStandardVector3(), v3.ToStandardVector3());

        Assert.AreEqual(expected.Normal, actual.Normal.ToStandardVector3());
        Assert.AreEqual(expected.D, (float) actual.D);
    }

    [TestMethod]
    public void TestDotCoordinate()
    {
        var v1 = new Vector3(1, 1, 1);
        var v2 = new Vector3(1, 1, 5);
        var v3 = new Vector3(1, 5, 5);

        var actual = Plane.CreateFromVertices(v1, v2, v3);
        var expected = System.Numerics.Plane.CreateFromVertices(v1.ToStandardVector3(), v2.ToStandardVector3(), v3.ToStandardVector3());
        var eval1 = new Vector3(-20, -1, -7);
        var eval2 = new Vector3(400, 1221, -7);
        var eval3 = v1;

        Assert.AreEqual(System.Numerics.Plane.DotCoordinate(expected, eval1.ToStandardVector3()), actual.DotCoordinate(eval1));
        Assert.AreEqual(System.Numerics.Plane.DotCoordinate(expected, eval2.ToStandardVector3()), actual.DotCoordinate(eval2));
        Assert.AreEqual(System.Numerics.Plane.DotCoordinate(expected, eval3.ToStandardVector3()), actual.DotCoordinate(eval3));
    }
}