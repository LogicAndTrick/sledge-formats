using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sledge.Formats.Tests
{
    [TestClass]
    public class TestStringExtensions
    {
        [TestMethod]
        public void TestSplitWithQuotes()
        {
            CollectionAssert.AreEqual(
                new  [] { "a", "b", "c" },
                "a b c".SplitWithQuotes()
            );
            CollectionAssert.AreEqual(
                new  [] { "a", "b", "c" },
                "axbxc".SplitWithQuotes(new []{ 'x' })
            );
            CollectionAssert.AreEqual(
                new  [] { "axb", "c" },
                "1axb1xc".SplitWithQuotes(new []{ 'x' }, '1')
            );
            CollectionAssert.AreEqual(
                new  [] { "a", "b", "c" },
                "\"a\" b c".SplitWithQuotes()
            );
            CollectionAssert.AreEqual(
                new  [] { "a b", "c" },
                "\"a b\" c".SplitWithQuotes()
            );
            CollectionAssert.AreEqual(
                new  [] { "a b", "c" },
                "\"a b\" \"c\"".SplitWithQuotes()
            );
            CollectionAssert.AreEqual(
                new  [] { "a b", "c" },
                "\"a b\"\t\"c\"".SplitWithQuotes()
            );
        }
    }
}