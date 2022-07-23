using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Map.Formats;
using Sledge.Formats.Map.Objects;
using Path = System.IO.Path;

namespace Sledge.Formats.Map.Tests.Formats
{
    [TestClass]
    public class TestWorldcraftFormat
    {
        [TestMethod]
        public void TestRmfFormatLoading()
        {
            var format = new WorldcraftRmfFormat();
            foreach (var file in Directory.GetFiles(@"D:\Downloads\formats\rmf"))
            {
                using (var r = File.OpenRead(file))
                {
                    try
                    {
                        format.Read(r);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail($"Unable to read file: {Path.GetFileName(file)}. {ex.Message}");
                    }
                }
            }
        }

        [TestMethod]
        public void TestRmfVersion22()
        {
            var format = new WorldcraftRmfFormat();
            var map = format.ReadFromFile(@"D:\Downloads\worldcraft\rmfs\test22.rmf");
            Assert.AreEqual(0, map.Paths.Count);
            Assert.AreEqual(0, map.Worldspawn.FindAll().OfType<Group>().Count());
            Assert.AreEqual(1, map.Worldspawn.FindAll().OfType<Entity>().Count());
            Assert.AreEqual(1, map.Worldspawn.FindAll().OfType<Solid>().Count());
        }

        [TestMethod]
        public void TestRmfVersion18()
        {
            var format = new WorldcraftRmfFormat();
            var map = format.ReadFromFile(@"D:\Downloads\worldcraft\rmfs\test18.rmf");
            Assert.AreEqual(0, map.Paths.Count);
            Assert.AreEqual(0, map.Worldspawn.FindAll().OfType<Group>().Count());
            Assert.AreEqual(1, map.Worldspawn.FindAll().OfType<Entity>().Count());
            Assert.AreEqual(1, map.Worldspawn.FindAll().OfType<Solid>().Count());
        }

        [TestMethod]
        public void TestRmfVersion16()
        {
            var format = new WorldcraftRmfFormat();
            var map = format.ReadFromFile(@"D:\Downloads\worldcraft\rmfs\test16.rmf");
            Assert.AreEqual(0, map.Paths.Count);
            Assert.AreEqual(0, map.Worldspawn.FindAll().OfType<Group>().Count());
            Assert.AreEqual(1, map.Worldspawn.FindAll().OfType<Entity>().Count());
            Assert.AreEqual(1, map.Worldspawn.FindAll().OfType<Solid>().Count());
        }
    }
}