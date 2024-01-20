using System;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Map.Formats;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Tests.Formats
{
    [TestClass]
    public class TestJackhammerFormat
    {
        [TestMethod]
        public void TestJmf121()
        {
            using var file = typeof(TestJackhammerFormat).Assembly.GetManifestResourceStream("Sledge.Formats.Map.Tests.Resources.jmf.default-room-121.jmf");
            var format = new JackhammerJmfFormat();
            var map = format.Read(file);
            Assert.AreEqual("worldspawn", map.Worldspawn.ClassName);
        }

        [TestMethod]
        public void TestJmf122()
        {
            using var file = typeof(TestJackhammerFormat).Assembly.GetManifestResourceStream("Sledge.Formats.Map.Tests.Resources.jmf.default-room-122.jmf");
            var format = new JackhammerJmfFormat();
            var map = format.Read(file);
            Assert.AreEqual("worldspawn", map.Worldspawn.ClassName);
            Assert.AreEqual(3, map.BackgroundImages.Count);

            var front = map.BackgroundImages.Single(x => x.Viewport == ViewportType.OrthographicFront);
            Assert.AreEqual("C:/Test/Viewport.png", front.Path);
            Assert.IsTrue(Math.Abs(front.Scale - 2.5) < 0.0001);
            Assert.AreEqual(175, front.Luminance);
            Assert.AreEqual(FilterMode.Linear, front.Filter);
            Assert.AreEqual(true, front.InvertColours);
            Assert.AreEqual(new Vector2(6, -7), front.Offset);
        }
    }
}