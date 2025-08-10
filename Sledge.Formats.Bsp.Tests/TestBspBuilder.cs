using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Bsp.Builder;
using Sledge.Formats.Bsp.Objects;
using Sledge.Formats.Id;
using Plane = Sledge.Formats.Bsp.Objects.Plane;

namespace Sledge.Formats.Bsp.Tests;

[TestClass]
public class TestBspBuilder
{
    [TestMethod]
    public void TestAddingPlanes()
    {
        var builder = new BspBuilder(Version.Goldsource);

        var plane1 = new Plane
        {
            Normal = Vector3.UnitX,
            Distance = 10,
            Type = PlaneType.X
        };
        var plane2 = new Plane
        {
            Normal = Vector3.UnitY,
            Distance = 20,
            Type = PlaneType.Y
        };
        var plane3 = new Plane // intentionally the same as plane1
        {
            Normal = Vector3.UnitX,
            Distance = 10,
            Type = PlaneType.X
        };

        builder.AddPlane(plane1, false);
        builder.AddPlane(plane2, false);
        builder.AddPlane(plane3, false);

        Assert.AreEqual(2, builder.BspFile.Planes.Count);
        Assert.AreEqual(plane1, builder.BspFile.Planes[0]);
        Assert.AreEqual(plane2, builder.BspFile.Planes[1]);
    }

    [TestMethod]
    public void TestAddingEntities()
    {
        var builder = new BspBuilder(Version.Goldsource);
        var e1 = new Entity { ClassName = "a" };
        var e2 = new Entity { ClassName = "b" };
        var e3 = new Entity { ClassName = "c" };

        builder.AddEntity(e1);
        builder.AddEntity(e2);
        builder.AddEntity(e3);
        builder.AddEntity(e3);

        Assert.AreEqual(3, builder.BspFile.Entities.Count);
        Assert.AreEqual(e1, builder.BspFile.Entities[0]);
        Assert.AreEqual(e2, builder.BspFile.Entities[1]);
        Assert.AreEqual(e3, builder.BspFile.Entities[2]);
    }

    [TestMethod]
    public void TestAddingTextureInfos()
    {
        var builder = new BspBuilder(Version.Goldsource);
        var ti1 = new TextureInfo { S = Vector4.UnitW, T = Vector4.UnitY };
        var ti2 = new TextureInfo { S = Vector4.UnitX, T = Vector4.UnitY };
        var ti3 = new TextureInfo { S = Vector4.UnitW, T = Vector4.UnitY }; // same as ti1

        builder.AddTextureInfo(ti1);
        builder.AddTextureInfo(ti2);
        builder.AddTextureInfo(ti3);

        Assert.AreEqual(2, builder.BspFile.Texinfo.Count);
        Assert.AreEqual(ti1, builder.BspFile.Texinfo[0]);
        Assert.AreEqual(ti2, builder.BspFile.Texinfo[1]);
    }

    [TestMethod]
    public void TestAddingMipTextures()
    {
        var builder = new BspBuilder(Version.Goldsource);
        var m1 = new MipTexture { Width = 10, Height = 20, Name = "a", NumMips = 0 };
        var m2 = new MipTexture { Width = 20, Height = 40, Name = "b", NumMips = 0 };
        var m3 = new MipTexture { Width = 30, Height = 60, Name = "a", NumMips = 0 }; // same name

        builder.AddMipTexture(m1);
        builder.AddMipTexture(m2);
        builder.AddMipTexture(m3);

        Assert.AreEqual(2, builder.BspFile.Textures.Count);
        Assert.AreEqual(m1, builder.BspFile.Textures[0]);
        Assert.AreEqual(m2, builder.BspFile.Textures[1]);
    }
}