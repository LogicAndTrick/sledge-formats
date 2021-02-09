using System;
using System.Collections.Generic;
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

        [TestMethod]
        public void TestSplitWithQuotes_EscapedQuotes()
        {
            CollectionAssert.AreEqual(
                new[] { "Simple" }, 
                "Simple".SplitWithQuotes()
            );
            CollectionAssert.AreEqual(
                new[] { "No", "Quotes" }, 
                "No Quotes".SplitWithQuotes()
            );
            CollectionAssert.AreEqual(
                new[] { "With", "Quotes" }, 
                @"""With"" ""Quotes""".SplitWithQuotes()
            );
            CollectionAssert.AreEqual(
                new[] { "Empty", "" }, 
                @"""Empty"" """"".SplitWithQuotes()
            );
            CollectionAssert.AreEqual(
                new[] { "With", @"""Escaped"" Quotes" }, 
                @"""With"" ""\""Escaped\"" Quotes""".SplitWithQuotes()
            );
            CollectionAssert.AreEqual(
                new[] { "Json", @"{ ""Key"": ""Value"" }" }, 
                @"""Json"" ""{ \""Key\"": \""Value\"" }""".SplitWithQuotes()
            );
        }
    }
}