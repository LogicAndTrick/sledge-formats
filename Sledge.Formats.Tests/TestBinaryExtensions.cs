using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sledge.Formats.Tests
{
    [TestClass]
    public class TestBinaryExtensions
    {
        [TestMethod]
        public void TestReadFixedLengthString()
        {
            var ms = new MemoryStream(new byte[]
            {
                97, 97, 97, 0,
                0 , 0 ,  0, 0,
                0 , 0 ,  0, 0,
                0 , 0 ,  0, 0,
            });
            using (var br = new BinaryReader(ms))
            {
                var fls = br.ReadFixedLengthString(Encoding.ASCII, 8);
                Assert.AreEqual("aaa", fls);
                Assert.AreEqual(8, ms.Position);
            }
        }

        [TestMethod]
        public void TestWriteFixedLengthString()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms))
            {
                bw.WriteFixedLengthString(Encoding.ASCII, 8, "aaa");
                Assert.AreEqual(8, ms.Position);
                CollectionAssert.AreEqual(
                    new byte[] {97, 97, 97, 0, 0, 0, 0, 0},
                    ms.ToArray()
                );
            }
        }

        [TestMethod]
        public void TestReadNullTerminatedString()
        {
            var ms = new MemoryStream(new byte[]
            {
                97, 97, 97, 0,
                0 , 0 ,  0, 0,
                0 , 0 ,  0, 0,
                0 , 0 ,  0, 0,
            });
            using (var br = new BinaryReader(ms))
            {
                var fls = br.ReadNullTerminatedString();
                Assert.AreEqual("aaa", fls);
                Assert.AreEqual(4, ms.Position);
            }
        }

        [TestMethod]
        public void TestWriteNullTerminatedString()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms))
            {
                bw.WriteNullTerminatedString("aaa");
                Assert.AreEqual(4, ms.Position);
                CollectionAssert.AreEqual(
                    new byte[] { 97, 97, 97, 0 },
                    ms.ToArray()
                );
            }
        }

        [TestMethod]
        public void TestReadCString()
        {
            var ms = new MemoryStream(new byte[]
            {
                4, 97, 97, 97,
                0 , 0 ,  0, 0,
                0 , 0 ,  0, 0,
                0 , 0 ,  0, 0,
            });
            using (var br = new BinaryReader(ms))
            {
                var fls = br.ReadCString();
                Assert.AreEqual("aaa", fls);
                Assert.AreEqual(5, ms.Position);
            }
        }

        [TestMethod]
        public void TestWriteCString()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms))
            {
                bw.WriteCString("aaa", 256);
                Assert.AreEqual(5, ms.Position);
                CollectionAssert.AreEqual(
                    new byte[] { 4, 97, 97, 97, 0 },
                    ms.ToArray()
                );
            }
        }

        [TestMethod]
        public void TestWriteCString_MaxLength()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms))
            {
                bw.WriteCString("aaa", 2);
                Assert.AreEqual(3, ms.Position);
                CollectionAssert.AreEqual(
                    new byte[] { 2, 97, 0 },
                    ms.ToArray()
                );
            }
        }

        [TestMethod]
        public void TestReadUshortArray()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.ASCII, true))
            {
                bw.Write((ushort) 123);
                bw.Write((ushort) 456);
                bw.Write((ushort) 789);
            }
            ms.Position = 0;

            using (var br = new BinaryReader(ms))
            {
                var a = br.ReadUshortArray(2);
                CollectionAssert.AreEqual(new ushort[] { 123, 456 }, a);
                Assert.AreEqual(4, ms.Position);
            }
        }

        [TestMethod]
        public void TestReadShortArray()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.ASCII, true))
            {
                bw.Write((short) 123);
                bw.Write((short) -456);
                bw.Write((short) 789);
            }
            ms.Position = 0;

            using (var br = new BinaryReader(ms))
            {
                var a = br.ReadShortArray(2);
                CollectionAssert.AreEqual(new short[] { 123, -456 }, a);
                Assert.AreEqual(4, ms.Position);
            }
        }

        [TestMethod]
        public void TestReadIntArray()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.ASCII, true))
            {
                bw.Write(123);
                bw.Write(-456);
                bw.Write(789);
            }
            ms.Position = 0;

            using (var br = new BinaryReader(ms))
            {
                var a = br.ReadIntArray(2);
                CollectionAssert.AreEqual(new [] { 123, -456 }, a);
                Assert.AreEqual(8, ms.Position);
            }
        }

        [TestMethod]
        public void TestReadSingleArrayAsDecimal()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.ASCII, true))
            {
                bw.Write(123f);
                bw.Write(456f);
                bw.Write(789f);
            }
            ms.Position = 0;

            using (var br = new BinaryReader(ms))
            {
                var a = br.ReadSingleArrayAsDecimal(2);
                CollectionAssert.AreEqual(new[] { 123m, 456m }, a);
                Assert.AreEqual(8, ms.Position);
            }
        }

        [TestMethod]
        public void TestReadSingleArray()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.ASCII, true))
            {
                bw.Write(123f);
                bw.Write(456f);
                bw.Write(789f);
            }
            ms.Position = 0;

            using (var br = new BinaryReader(ms))
            {
                var a = br.ReadSingleArray(2);
                CollectionAssert.AreEqual(new[] { 123f, 456f }, a);
                Assert.AreEqual(8, ms.Position);
            }
        }

        [TestMethod]
        public void TestReadSingleAsDecimal()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.ASCII, true))
            {
                bw.Write(123f);
                bw.Write(456f);
                bw.Write(789f);
            }
            ms.Position = 0;

            using (var br = new BinaryReader(ms))
            {
                var a = br.ReadSingleAsDecimal();
                Assert.AreEqual(123m, a);
                Assert.AreEqual(4, ms.Position);
            }
        }

        [TestMethod]
        public void TestWriteDecimalAsSingle()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms))
            {
                bw.WriteDecimalAsSingle(123m);
                Assert.AreEqual(4, ms.Position);
                CollectionAssert.AreEqual(BitConverter.GetBytes(123f), ms.ToArray());
            }
        }

        [TestMethod]
        public void TestReadRGBColour()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.ASCII, true))
            {
                bw.Write((byte) 255);
                bw.Write((byte) 0);
                bw.Write((byte) 0);
                bw.Write((byte) 0);
            }
            ms.Position = 0;

            using (var br = new BinaryReader(ms))
            {
                var a = br.ReadRGBColour();
                Assert.AreEqual(Color.Red.ToArgb(), a.ToArgb());
                Assert.AreEqual(3, ms.Position);
            }
        }

        [TestMethod]
        public void TestWriteRGBColour()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms))
            {
                bw.WriteRGBColour(Color.Red);
                Assert.AreEqual(3, ms.Position);
                CollectionAssert.AreEqual(new byte [] { 255, 0, 0 }, ms.ToArray());
            }
        }

        [TestMethod]
        public void TestReadRGBAColour()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.ASCII, true))
            {
                bw.Write((byte)255);
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write((byte)255);
            }
            ms.Position = 0;

            using (var br = new BinaryReader(ms))
            {
                var a = br.ReadRGBAColour();
                Assert.AreEqual(Color.Red.ToArgb(), a.ToArgb());
                Assert.AreEqual(4, ms.Position);
            }
        }

        [TestMethod]
        public void TestWriteRGBAColour()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms))
            {
                bw.WriteRGBAColour(Color.Red);
                Assert.AreEqual(4, ms.Position);
                CollectionAssert.AreEqual(new byte[] { 255, 0, 0, 255 }, ms.ToArray());
            }
        }

        [TestMethod]
        public void TestReadVector3Array()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.ASCII, true))
            {
                foreach (var n in Enumerable.Range(1, 9)) bw.Write((float) n);
            }
            ms.Position = 0;

            using (var br = new BinaryReader(ms))
            {
                var a = br.ReadVector3Array(2);
                CollectionAssert.AreEqual(new Vector3[] { new Vector3(1, 2, 3), new Vector3(4, 5, 6) }, a);
                Assert.AreEqual(24, ms.Position);
            }
        }

        [TestMethod]
        public void TestReadVector3()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.ASCII, true))
            {
                foreach (var n in Enumerable.Range(1, 9)) bw.Write((float)n);
            }
            ms.Position = 0;

            using (var br = new BinaryReader(ms))
            {
                var a = br.ReadVector3();
                Assert.AreEqual(new Vector3(1, 2, 3), a);
                Assert.AreEqual(12, ms.Position);
            }
        }

        [TestMethod]
        public void TestWriteVector3()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms))
            {
                bw.WriteVector3(new Vector3(1, 2, 3));
                Assert.AreEqual(12, ms.Position);
                var exp = new List<byte>();
                exp.AddRange(BitConverter.GetBytes(1f));
                exp.AddRange(BitConverter.GetBytes(2f));
                exp.AddRange(BitConverter.GetBytes(3f));
                CollectionAssert.AreEqual(exp, ms.ToArray());
            }
        }
    }
}
