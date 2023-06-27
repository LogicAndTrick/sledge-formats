using System.Drawing;
using System.Globalization;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sledge.Formats.Tests
{
    [TestClass]
    public class TestNumericsExtensions
    {
        [TestMethod]
        public void TestToVector3()
        {
            Assert.AreEqual(
                new Vector3(1, 2, 0),
                new Vector2(1, 2).ToVector3()
            );
        }

        [TestMethod]
        public void TestEquivalentTo()
        {
            Assert.IsTrue(new Vector3(1, 2, 3).EquivalentTo(new Vector3(1, 2, 3)));
            Assert.IsTrue(new Vector3(1, 2, 3.0001f).EquivalentTo(new Vector3(1, 2, 3)));
            Assert.IsTrue(new Vector3(1, 2, 3.01f).EquivalentTo(new Vector3(1, 2, 3), 0.01f));
            Assert.IsFalse(new Vector3(1, 2, 3.1f).EquivalentTo(new Vector3(1, 2, 3), 0.01f));
            Assert.IsFalse(new Vector3(1, 2, 3.01f).EquivalentTo(new Vector3(1, 2, 3)));
            Assert.IsFalse(new Vector3(1, 2, 3.0001f).EquivalentTo(new Vector3(1, 2, 3), 0.00001f));
        }

        [TestMethod]
        public void TestParse()
        {
            Assert.AreEqual(
                NumericsExtensions.Parse("1", "2", "3", NumberStyles.Float, CultureInfo.InvariantCulture),
                new Vector3(1, 2, 3)
            );
            Assert.AreEqual(
                NumericsExtensions.Parse("1,01", "2,02", "3,03", NumberStyles.Float, CultureInfo.GetCultureInfo("es-ES")),
                new Vector3(1.01f, 2.02f, 3.03f)
            );
        }

        [TestMethod]
        public void TestNormalise()
        {
            Assert.AreEqual(Vector3.Normalize(new Vector3(1, 2, 3)), new Vector3(1, 2, 3).Normalise());
        }

        [TestMethod]
        public void TestAbsolute()
        {
            Assert.AreEqual(Vector3.Abs(new Vector3(-1, 2, -3)), new Vector3(-1, 2, -3).Absolute());
        }

        [TestMethod]
        public void TestDot()
        {
            Assert.AreEqual(
                Vector3.Dot(new Vector3(1, 2, 3), new Vector3(4, 5, 6)),
                new Vector3(1, 2, 3).Dot(new Vector3(4, 5, 6))
            );
        }

        [TestMethod]
        public void TestCross()
        {
            Assert.AreEqual(
                Vector3.Cross(new Vector3(1, 2, 3), new Vector3(4, 5, 6)),
                new Vector3(1, 2, 3).Cross(new Vector3(4, 5, 6))
            );
        }

        [TestMethod]
        public void TestRound()
        {
            Assert.AreEqual(
                new Vector3(1.01f, 2.02f, 3.03f),
                new Vector3(1.0099999f, 2.02111111f, 3.034f).Round(2)
            );
        }

        [TestMethod]
        public void TestClosestAxis()
        {
            Assert.AreEqual(Vector3.UnitZ, new Vector3(1, 2, 3).ClosestAxis());
            Assert.AreEqual(Vector3.UnitX, new Vector3(1, 1, 1).ClosestAxis());
            Assert.AreEqual(Vector3.UnitY, new Vector3(1, 2, 2).ClosestAxis());
        }

        [TestMethod]
        public void TestToPrecisionVector3()
        {
            Assert.AreEqual(
                new Precision.Vector3d(1, 2, 3),
                new Vector3(1, 2, 3).ToPrecisionVector3()
            );
        }

        [TestMethod]
        public void TestToVector2()
        {
            Assert.AreEqual(
                new Vector2(1, 2),
                new Vector3(1, 2, 3).ToVector2()
            );
        }

        [TestMethod]
        public void TestToVector4()
        {
            Assert.AreEqual(
                new Vector4(1, 0, 0, 1),
                Color.Red.ToVector4()
            );
        }

        [TestMethod]
        public void TestToColor4()
        {
            Assert.AreEqual(
                Color.Red.ToArgb(),
                new Vector4(1, 0, 0, 1).ToColor().ToArgb()
            );
        }

        [TestMethod]
        public void TestToColor3()
        {
            Assert.AreEqual(
                Color.Red.ToArgb(),
                new Vector3(1, 0, 0).ToColor().ToArgb()
            );
        }

        [TestMethod]
        public void Transform()
        {
            Assert.AreEqual(
                Vector3.Transform(new Vector3(1, 2, 3), Matrix4x4.CreateRotationX(2)),
                Matrix4x4.CreateRotationX(2).Transform(new Vector3(1, 2, 3))
            );
        }
    }
}
