using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Valve;

namespace Sledge.Formats.Tests.Valve
{
    [TestClass]
    public class TestSerialisedObject
    {
        private static Stream Streamify(string s)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(s));
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private const string Q = "\"";

        [TestMethod]
        public void TestLoadingSimple()
        {
            var fmt = new SerialisedObjectFormatter();
            using var input = Streamify($@"Test
{{
    {Q}Key1{Q} {Q}Value1{Q}
    {Q}Key2{Q} {Q}Value2{Q}
}}
");
            var output = fmt.Deserialize(input).ToList();
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("Test", output[0].Name);
            Assert.AreEqual(0, output[0].Children.Count);
            Assert.AreEqual(2, output[0].Properties.Count);
            Assert.AreEqual("Key1", output[0].Properties[0].Key);
            Assert.AreEqual("Key2", output[0].Properties[1].Key);
            Assert.AreEqual("Value1", output[0].Properties[0].Value);
            Assert.AreEqual("Value2", output[0].Properties[1].Value);
        }

        [TestMethod]
        public void TestLoadingKeyOrder()
        {
            var fmt = new SerialisedObjectFormatter();
            using var input = Streamify($@"Test
{{
    {Q}Key{Q} {Q}1{Q}
    {Q}Key{Q} {Q}3{Q}
    {Q}Key{Q} {Q}2{Q}
}}
");
            var output = fmt.Deserialize(input).ToList();
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("Test", output[0].Name);
            Assert.AreEqual(0, output[0].Children.Count);
            Assert.AreEqual(3, output[0].Properties.Count);
            Assert.AreEqual("Key", output[0].Properties[0].Key);
            Assert.AreEqual("Key", output[0].Properties[1].Key);
            Assert.AreEqual("Key", output[0].Properties[2].Key);
            Assert.AreEqual("1", output[0].Properties[0].Value);
            Assert.AreEqual("3", output[0].Properties[1].Value);
            Assert.AreEqual("2", output[0].Properties[2].Value);
        }

        [TestMethod]
        public void TestLoadingChildren()
        {
            var fmt = new SerialisedObjectFormatter();
            using var input = Streamify($@"Test1
{{
    {Q}A{Q} {Q}1{Q}
    Test2
    {{
        {Q}B{Q} {Q}2{Q}
        Test3
        {{
            {Q}C{Q} {Q}3{Q}
        }}
    }}
    Test2
    {{
    }}
    Test2
    {{
        {Q}D{Q} {Q}4{Q}
    }}
}}
Test4
{{
    {Q}E{Q} {Q}5{Q}
}}
");
            var output = fmt.Deserialize(input).ToList();
            Assert.AreEqual(2, output.Count);

            Assert.AreEqual("Test1", output[0].Name);
            Assert.AreEqual(3, output[0].Children.Count);
            Assert.AreEqual("A", output[0].Properties[0].Key);
            
            Assert.AreEqual("Test2", output[0].Children[0].Name);
            Assert.AreEqual(1, output[0].Children[0].Children.Count);
            Assert.AreEqual(1, output[0].Children[0].Properties.Count);
            Assert.AreEqual("B", output[0].Children[0].Properties[0].Key);
            Assert.AreEqual("2", output[0].Children[0].Properties[0].Value);

            Assert.AreEqual("Test3", output[0].Children[0].Children[0].Name);
            Assert.AreEqual(0, output[0].Children[0].Children[0].Children.Count);
            Assert.AreEqual(1, output[0].Children[0].Children[0].Properties.Count);
            Assert.AreEqual("C", output[0].Children[0].Children[0].Properties[0].Key);
            Assert.AreEqual("3", output[0].Children[0].Children[0].Properties[0].Value);

            Assert.AreEqual("Test2", output[0].Children[1].Name);
            Assert.AreEqual(0, output[0].Children[1].Children.Count);
            Assert.AreEqual(0, output[0].Children[1].Properties.Count);

            Assert.AreEqual("Test2", output[0].Children[2].Name);
            Assert.AreEqual("D", output[0].Children[2].Properties[0].Key);
            Assert.AreEqual("4", output[0].Children[2].Properties[0].Value);

            Assert.AreEqual("Test4", output[1].Name);
            Assert.AreEqual(0, output[1].Children.Count);
            Assert.AreEqual("E", output[1].Properties[0].Key);
            Assert.AreEqual("5", output[1].Properties[0].Value);
        }

        [TestMethod]
        public void TestEscapedQuotes()
        {
            var fmt = new SerialisedObjectFormatter();
            using var input = Streamify($@"Test
{{
    {Q}Key\{Q}With\{Q}Quotes{Q} {Q}Quoted\{Q}Value{Q}
}}
");
            var output = fmt.Deserialize(input).ToList();
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("Test", output[0].Name);
            Assert.AreEqual(0, output[0].Children.Count);
            Assert.AreEqual(1, output[0].Properties.Count);
            Assert.AreEqual("Key\"With\"Quotes", output[0].Properties[0].Key);
            Assert.AreEqual("Quoted\"Value", output[0].Properties[0].Value);
        }
    }
}
