using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Tokens;

namespace Sledge.Formats.Tests.Tokens
{
    [TestClass]
    public class TestTokenParsing
    {
        [TestMethod]
        public void TestParseInteger()
        {
            var tokeniser = new Tokeniser(new[] {'-', '+', '.'});
            AssertNumber("1");
            AssertNumber("123");
            AssertNumber("-123");
            AssertNumber("+123");
            
            AssertFailure("");
            AssertFailure("A");
            AssertFailure(".");
            AssertFailure("-.");
            AssertFailure("+.");

            void AssertNumber(string text)
            {
                using var it = tokeniser.Tokenise(text).GetEnumerator();
                it.MoveNext();
                var expected = int.Parse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
                var actual = TokenParsing.ParseInteger(it);
                Assert.AreEqual(expected, actual);
            }

            void AssertFailure(string text)
            {
                using var it = tokeniser.Tokenise(text).GetEnumerator();
                it.MoveNext();
                Assert.ThrowsException<Exception>(() => TokenParsing.ParseDecimal(it));
            }
        }

        [TestMethod]
        public void TestParseDecimal()
        {
            var tokeniser = new Tokeniser(new[] {'-', '+', '.'});
            AssertNumber("1");
            AssertNumber("123");
            AssertNumber("1.23");
            AssertNumber("-123");
            AssertNumber("-1.23");
            AssertNumber("+123");
            AssertNumber("+1.23");
            AssertNumber(".1");
            AssertNumber("-.1");
            AssertNumber("+.1");
            AssertNumber("-0.");
            AssertNumber("+0.");
            AssertNumber("1.2345e012");
            AssertNumber("-1.2345e012");
            AssertNumber("1.2345e-012");
            AssertNumber("-1.2345e-012");
            
            AssertFailure("");
            AssertFailure("A");
            AssertFailure(".");
            AssertFailure("-.");
            AssertFailure("+.");

            void AssertNumber(string text)
            {
                using var it = tokeniser.Tokenise(text).GetEnumerator();
                it.MoveNext();
                var expected = decimal.Parse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent);
                var actual = TokenParsing.ParseDecimal(it);
                Assert.AreEqual(expected, actual);
            }

            void AssertFailure(string text)
            {
                using var it = tokeniser.Tokenise(text).GetEnumerator();
                it.MoveNext();
                Assert.ThrowsException<Exception>(() => TokenParsing.ParseDecimal(it));
            }
        }

        [TestMethod]
        public void TestBalanceBrackets()
        {
            var tokeniser = new Tokeniser(new[] {'(', ',', ')', '{', '}'});
            
            AssertBrackets("");
            AssertBrackets("()", "(", ")");
            AssertBrackets("({})", "(", "{", "}", ")");
            AssertBrackets("({,})", "(", "{", ",", "}", ")");
            AssertBrackets("(hello,world)", "(", "hello", ",", "world", ")");
            
            AssertBrackets("()()()", "(", ")");
            AssertBrackets("((()))", "(", "(", "(", ")", ")", ")");
            AssertBrackets("(()())", "(", "(", ")", "(", ")", ")");

            AssertFailure("(");

            void AssertBrackets(string text, params string[] expected)
            {
                using var it = tokeniser.Tokenise(text).GetEnumerator();
                it.MoveNext();
                var actual = TokenParsing.BalanceBrackets(it, '(', ')').Select(x => x.Value).ToList();
                CollectionAssert.AreEqual(expected, actual);
            }

            void AssertFailure(string text)
            {
                using var it = tokeniser.Tokenise(text).GetEnumerator();
                it.MoveNext();
                Assert.ThrowsException<Exception>(() => TokenParsing.BalanceBrackets(it, '(', ')'));
            }
        }

        [TestMethod]
        public void TestAppendedString()
        {
            var tokeniser = new Tokeniser(new[] {'+'});

            AssertString("\"Test\"", "Test");
            AssertString("\"Test\" \"Test\"", "Test");
            AssertString("\"Test\" + \"Test\"", "TestTest");
            AssertString("\"Test\" + \"Test\" +", "TestTest");
            AssertString("\"Test\" + \"Test\" + +", "TestTest");
            AssertString("\"Test\" + \"Test\" + \"Test\"", "TestTestTest");

            AssertString("1", "");

            void AssertString(string text, string expected)
            {
                using var it = tokeniser.Tokenise(text).GetEnumerator();
                it.MoveNext();
                var actual = TokenParsing.ParseAppendedString(it);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
