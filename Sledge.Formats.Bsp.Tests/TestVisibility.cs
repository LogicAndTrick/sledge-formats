using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Bsp.Lumps;

namespace Sledge.Formats.Bsp.Tests;

[TestClass]
public class TestVisibility
{
    private static MemoryStream GetFile(string name)
    {
        // Walk up the folder tree until we hit Sledge.Formats.Bsp.Tests
        var dir = Environment.CurrentDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "Resources", name)))
        {
            dir = Path.GetDirectoryName(dir);
        }
        var file = Path.Combine(dir, "Resources", name);
        using var res = File.OpenRead(file);

        var ms = new MemoryStream();
        res.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    private static bool[][] GetVisibility()
    {
        // file format
        /*
        maps/e1m3.bsp       // file name
        1689                // number of leafs
        1 0 0 0 0 0 0 0 ... // leaf 0
        1 0 0 0 0 0 0 0 ... // leaf 1
        ................... // etc
        */
        using var file = GetFile("quake/e1m3.vis.gz");
        using var gz = new GZipStream(file, CompressionMode.Decompress);
        using var ms = new MemoryStream();
        gz.CopyTo(ms);

        var lines = Encoding.ASCII.GetString(ms.ToArray()).Split("\n").Select(x => x.Trim()).ToList();
        var mapname = lines[0];
        Assert.AreEqual("maps/e1m3.bsp", mapname);

        var numleafs = int.Parse(lines[1]);
        Assert.AreEqual(1689, numleafs);

        var result = new bool[numleafs][];
        for (var i = 0; i < numleafs; i++)
        {
            result[i] = lines[i + 2].Split(' ').Where(x => x == "0" || x == "1").Select(x => x == "1").ToArray();
        }
        return result;
    }

    [TestMethod]
    public void TestVisibilitySelf()
    {
        using var file = GetFile("quake/e1m3.bsp");
        var bsp = new BspFile(file);
        var leaf = bsp.Leaves.First();
        Assert.IsTrue(bsp.Visibility.IsVisible(bsp.Leaves, 1, 1));
    }

    [TestMethod]
    public void TestVisibilityBasic()
    {
        using var file = GetFile("quake/e1m3.bsp");
        var bsp = new BspFile(file);
        Assert.IsFalse(bsp.Visibility.IsVisible(bsp.Leaves, 4, 1));
        Assert.IsFalse(bsp.Visibility.IsVisible(bsp.Leaves, 4, 2));
        Assert.IsFalse(bsp.Visibility.IsVisible(bsp.Leaves, 4, 3));
        Assert.IsTrue(bsp.Visibility.IsVisible(bsp.Leaves, 4, 4));
        Assert.IsTrue(bsp.Visibility.IsVisible(bsp.Leaves, 4, 5));
        Assert.IsTrue(bsp.Visibility.IsVisible(bsp.Leaves, 4, 6));
        Assert.IsTrue(bsp.Visibility.IsVisible(bsp.Leaves, 4, 7));
        Assert.IsFalse(bsp.Visibility.IsVisible(bsp.Leaves, 4, 8));
        Assert.IsFalse(bsp.Visibility.IsVisible(bsp.Leaves, 4, 9));
        Assert.IsTrue(bsp.Visibility.IsVisible(bsp.Leaves, 4, 10));
        Assert.IsTrue(bsp.Visibility.IsVisible(bsp.Leaves, 4, 11));
        Assert.IsTrue(bsp.Visibility.IsVisible(bsp.Leaves, 4, 12));
        Assert.IsTrue(bsp.Visibility.IsVisible(bsp.Leaves, 4, 13));
        Assert.IsFalse(bsp.Visibility.IsVisible(bsp.Leaves, 4, 14));
    }

    [TestMethod] // well, 100 is close to full...sort of
    public void TestVisibilityFull()
    {
        using var file = GetFile("quake/e1m3.bsp");
        var bsp = new BspFile(file);
        var vis = GetVisibility();
        Assert.AreEqual(vis.Length, bsp.Leaves.Count);
        for (var i = 1; i < 1000; i++)
        {
            var list = vis[i];
            Assert.AreEqual(list.Length, bsp.Leaves.Count);
            var visible = bsp.Visibility.GetVisibleLeaves(bsp.Leaves.Count, bsp.Leaves[i]).ToList();
            for (var j = 1; j < bsp.Leaves.Count; j++)
            {
                Assert.AreEqual(list[j-1], visible.Contains(j), $"{i} x {j}");
            }
        }
    }
}