using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.GameData.Addons.TrenchBroom;
using Sledge.Formats.Tokens;

/*
 * License for these tests: GPL-3.0, per the TrenchBroom repository.
 */

namespace Sledge.Formats.GameData.Tests.TrenchBroom;

[TestClass]
public class TestTrenchBroomExpressions
{
    private static Value Evaluate(string expression, Dictionary<string, Value> values = null)
    {
        var context = new EvaluationContext(values ?? new Dictionary<string, Value>());
        var exp = ExpressionParser.Parse(expression);
        var evaluator = new ExpressionEvaluator();
        return evaluator.Evaluate(exp, context);
    }

    // https://github.com/TrenchBroom/TrenchBroom/blob/master/common/test/src/EL/tst_EL.cpp

    [TestMethod]
    public void TestConstructValues()
    {
        var arrayType = Array.Empty<Value>();
        var mapType = new Dictionary<string, Value>();

        Assert.IsTrue(new Value(true).Type == DataType.Boolean);
        Assert.IsTrue(new Value(false).Type == DataType.Boolean);
        Assert.IsTrue(new Value("test").Type == DataType.String);
        Assert.IsTrue(new Value(1.0).Type == DataType.Number);
        Assert.IsTrue(new Value(arrayType).Type == DataType.Array);
        Assert.IsTrue(new Value(mapType).Type == DataType.Map);
        Assert.IsTrue(new Value().Type == DataType.Null);
    }

    [TestMethod]
    public void TestTypeConversions()
    {
        var arrayType = Array.Empty<Value>();
        var mapType = new Dictionary<string, Value>();

        Assert.AreEqual(new Value(true), new Value(true).ConvertTo(DataType.Boolean));
        Assert.AreEqual(new Value(false), new Value(false).ConvertTo(DataType.Boolean));
        Assert.AreEqual(new Value("true"), new Value(true).ConvertTo(DataType.String));
        Assert.AreEqual(new Value("false"), new Value(false).ConvertTo(DataType.String));
        Assert.AreEqual(new Value(1), new Value(true).ConvertTo(DataType.Number));
        Assert.AreEqual(new Value(0), new Value(false).ConvertTo(DataType.Number));
        Assert.ThrowsException<InvalidCastException>(() => new Value(true).ConvertTo(DataType.Array));
        Assert.ThrowsException<InvalidCastException>(() => new Value(false).ConvertTo(DataType.Array));
        Assert.ThrowsException<InvalidCastException>(() => new Value(true).ConvertTo(DataType.Map));
        Assert.ThrowsException<InvalidCastException>(() => new Value(false).ConvertTo(DataType.Map));
        Assert.ThrowsException<InvalidCastException>(() => new Value(true).ConvertTo(DataType.Range));
        Assert.ThrowsException<InvalidCastException>(() => new Value(false).ConvertTo(DataType.Range));
        Assert.ThrowsException<InvalidCastException>(() => new Value(true).ConvertTo(DataType.Null));
        Assert.ThrowsException<InvalidCastException>(() => new Value(false).ConvertTo(DataType.Null));
        Assert.ThrowsException<InvalidCastException>(() => new Value(true).ConvertTo(DataType.Undefined));
        Assert.ThrowsException<InvalidCastException>(() => new Value(false).ConvertTo(DataType.Undefined));

        Assert.AreEqual(new Value(true), new Value("asdf").ConvertTo(DataType.Boolean));
        Assert.AreEqual(new Value(false), new Value("false").ConvertTo(DataType.Boolean));
        Assert.AreEqual(new Value(false), new Value("").ConvertTo(DataType.Boolean));
        Assert.AreEqual(new Value("asdf"), new Value("asdf").ConvertTo(DataType.String));
        Assert.AreEqual(new Value(2), new Value("2").ConvertTo(DataType.Number));
        Assert.AreEqual(new Value(-2), new Value("-2.0").ConvertTo(DataType.Number));
        Assert.ThrowsException<InvalidCastException>(() => new Value("asdf").ConvertTo(DataType.Number));
        Assert.ThrowsException<InvalidCastException>(() => new Value("asdf").ConvertTo(DataType.Array));
        Assert.ThrowsException<InvalidCastException>(() => new Value("asfd").ConvertTo(DataType.Map));
        Assert.ThrowsException<InvalidCastException>(() => new Value("asdf").ConvertTo(DataType.Range));
        Assert.ThrowsException<InvalidCastException>(() => new Value("asdf").ConvertTo(DataType.Null));
        Assert.ThrowsException<InvalidCastException>(() => new Value("asdf").ConvertTo(DataType.Undefined));

        Assert.AreEqual(new Value(true), new Value(1).ConvertTo(DataType.Boolean));
        Assert.AreEqual(new Value(true), new Value(2).ConvertTo(DataType.Boolean));
        Assert.AreEqual(new Value(true), new Value(-2).ConvertTo(DataType.Boolean));
        Assert.AreEqual(new Value(false), new Value(0).ConvertTo(DataType.Boolean));
        Assert.AreEqual(new Value("1"), new Value(1.0).ConvertTo(DataType.String));
        Assert.AreEqual(new Value("-1"), new Value(-1.0).ConvertTo(DataType.String));
        Assert.AreEqual(new Value("1.1000000000000001"), new Value(1.1).ConvertTo(DataType.String));
        Assert.AreEqual(new Value("-1.1000000000000001"), new Value(-1.1).ConvertTo(DataType.String));
        Assert.AreEqual(new Value(1), new Value(1.0).ConvertTo(DataType.Number));
        Assert.AreEqual(new Value(-1), new Value(-1.0).ConvertTo(DataType.Number));
        Assert.ThrowsException<InvalidCastException>(() => new Value(1).ConvertTo(DataType.Array));
        Assert.ThrowsException<InvalidCastException>(() => new Value(2).ConvertTo(DataType.Map));
        Assert.ThrowsException<InvalidCastException>(() => new Value(3).ConvertTo(DataType.Range));
        Assert.ThrowsException<InvalidCastException>(() => new Value(4).ConvertTo(DataType.Null));
        Assert.ThrowsException<InvalidCastException>(() => new Value(5).ConvertTo(DataType.Undefined));

        Assert.ThrowsException<InvalidCastException>(() => new Value(arrayType).ConvertTo(DataType.Boolean));
        Assert.ThrowsException<InvalidCastException>(() => new Value(arrayType).ConvertTo(DataType.String));
        Assert.ThrowsException<InvalidCastException>(() => new Value(arrayType).ConvertTo(DataType.Number));
        Assert.AreEqual(new Value(arrayType), new Value(arrayType).ConvertTo(DataType.Array));
        Assert.ThrowsException<InvalidCastException>(() => new Value(arrayType).ConvertTo(DataType.Map));
        Assert.ThrowsException<InvalidCastException>(() => new Value(arrayType).ConvertTo(DataType.Range));
        Assert.ThrowsException<InvalidCastException>(() => new Value(arrayType).ConvertTo(DataType.Null));
        Assert.ThrowsException<InvalidCastException>(() => new Value(arrayType).ConvertTo(DataType.Undefined));

        Assert.ThrowsException<InvalidCastException>(() => new Value(mapType).ConvertTo(DataType.Boolean));
        Assert.ThrowsException<InvalidCastException>(() => new Value(mapType).ConvertTo(DataType.String));
        Assert.ThrowsException<InvalidCastException>(() => new Value(mapType).ConvertTo(DataType.Number));
        Assert.ThrowsException<InvalidCastException>(() => new Value(mapType).ConvertTo(DataType.Array));
        Assert.AreEqual(new Value(mapType), new Value(mapType).ConvertTo(DataType.Map));
        Assert.ThrowsException<InvalidCastException>(() => new Value(mapType).ConvertTo(DataType.Range));
        Assert.ThrowsException<InvalidCastException>(() => new Value(mapType).ConvertTo(DataType.Null));
        Assert.ThrowsException<InvalidCastException>(() => new Value(mapType).ConvertTo(DataType.Undefined));

        Assert.AreEqual(new Value(false), Value.Null.ConvertTo(DataType.Boolean));
        Assert.AreEqual(new Value(""), Value.Null.ConvertTo(DataType.String));
        Assert.AreEqual(new Value(0), Value.Null.ConvertTo(DataType.Number));
        Assert.AreEqual(new Value(arrayType), Value.Null.ConvertTo(DataType.Array));
        Assert.AreEqual(new Value(mapType), Value.Null.ConvertTo(DataType.Map));
        Assert.ThrowsException<InvalidCastException>(() => Value.Null.ConvertTo(DataType.Range));
        Assert.AreEqual(Value.Null, Value.Null.ConvertTo(DataType.Null));
        Assert.ThrowsException<InvalidCastException>(() => Value.Null.ConvertTo(DataType.Undefined));

        Assert.ThrowsException<InvalidCastException>(() => Value.Undefined.ConvertTo(DataType.Boolean));
        Assert.ThrowsException<InvalidCastException>(() => Value.Undefined.ConvertTo(DataType.String));
        Assert.ThrowsException<InvalidCastException>(() => Value.Undefined.ConvertTo(DataType.Number));
        Assert.ThrowsException<InvalidCastException>(() => Value.Undefined.ConvertTo(DataType.Array));
        Assert.ThrowsException<InvalidCastException>(() => Value.Undefined.ConvertTo(DataType.Map));
        Assert.ThrowsException<InvalidCastException>(() => Value.Undefined.ConvertTo(DataType.Range));
        Assert.ThrowsException<InvalidCastException>(() => Value.Undefined.ConvertTo(DataType.Null));
        Assert.AreEqual(Value.Undefined, Value.Undefined.ConvertTo(DataType.Undefined));
    }

    [TestMethod]
    public void TestSerializeValues()
    {
        Assert.AreEqual("16", new Value(16.0).AsString());
    }

    [TestMethod]
    public void TestSubscriptOperator()
    {
        Assert.ThrowsException<InvalidOperationException>(() => new Value(true).ValueAtIndex(new Value(0)));
        Assert.ThrowsException<InvalidOperationException>(() => new Value(1).ValueAtIndex(new Value(0)));
        Assert.ThrowsException<InvalidOperationException>(() => new Value().ValueAtIndex(new Value(0)));

        Assert.AreEqual(new Value("t"), new Value("test").ValueAtIndex(new Value(0)));
        Assert.AreEqual(new Value("e"), new Value("test").ValueAtIndex(new Value(1)));
        Assert.AreEqual(new Value("s"), new Value("test").ValueAtIndex(new Value(2)));
        Assert.AreEqual(new Value("t"), new Value("test").ValueAtIndex(new Value(3)));
        Assert.AreEqual(new Value("s"), new Value("test").ValueAtIndex(new Value(-2)));
        Assert.AreEqual(new Value(""), new Value("test").ValueAtIndex(new Value(4)));

        Assert.AreEqual(new Value("e"), new Value("test").ValueAtIndex(new Value(new[] { new Value(1) })));
        Assert.AreEqual(new Value("te"), new Value("test").ValueAtIndex(new Value(new[] { new Value(0), new Value(1) })));
        Assert.AreEqual(new Value("es"), new Value("test").ValueAtIndex(new Value(new[] { new Value(1), new Value(2) })));
        Assert.AreEqual(new Value("tt"), new Value("test").ValueAtIndex(new Value(new[] { new Value(0), new Value(3) })));
        Assert.AreEqual(new Value("test"), new Value("test").ValueAtIndex(new Value(new[] { new Value(0), new Value(1), new Value(2), new Value(3) })));
        Assert.AreEqual(new Value(""), new Value("test").ValueAtIndex(new Value(new[] { new Value(4) })));
        Assert.AreEqual(new Value("t"), new Value("test").ValueAtIndex(new Value(new[] { new Value(0), new Value(4) })));

        var arrayValue = new Value(new[] { new Value(1), new Value("test") });

        Assert.AreEqual(new Value(1), arrayValue.ValueAtIndex(new Value(0)));
        Assert.AreEqual(new Value("test"), arrayValue.ValueAtIndex(new Value(1)));
        Assert.AreEqual(new Value("test"), arrayValue.ValueAtIndex(new Value(-1)));
        Assert.AreEqual(new Value(1), arrayValue.ValueAtIndex(new Value(-2)));

        Assert.ThrowsException<InvalidOperationException>(() => arrayValue.ValueAtIndex(new Value(2)));
        Assert.ThrowsException<InvalidOperationException>(() => arrayValue.ValueAtIndex(new Value(-3)));
        Assert.ThrowsException<InvalidOperationException>(() => arrayValue.ValueAtIndex(new Value("asdf")));
        Assert.ThrowsException<InvalidOperationException>(() => arrayValue.ValueAtIndex(new Value("")));

        Assert.AreEqual(new Value(new[] { new Value(1) }), arrayValue.ValueAtIndex(new Value(new[] { new Value(0) })));
        Assert.AreEqual(new Value(new[] { new Value("test") }), arrayValue.ValueAtIndex(new Value(new[] { new Value(1) })));
        Assert.AreEqual(new Value(new[] { new Value(1), new Value("test") }), arrayValue.ValueAtIndex(new Value(new[] { new Value(0), new Value(1) })));
        Assert.ThrowsException<InvalidOperationException>(() => arrayValue.ValueAtIndex(new Value(new[] { new Value(2) })));
        Assert.ThrowsException<InvalidOperationException>(() => arrayValue.ValueAtIndex(new Value(new[] { new Value(1), new Value(2) })));
        Assert.ThrowsException<InvalidOperationException>(() => arrayValue.ValueAtIndex(new Value(new[] { new Value("test") })));
        Assert.ThrowsException<InvalidOperationException>(() => arrayValue.ValueAtIndex(new Value(new[] { new Value(0), new Value("test") })));

        var mapValue = new Value(new Dictionary<string, Value>
        {
            { "test", new Value(1) },
            { "huhu", new Value("yeah") },
        });

        Assert.AreEqual(new Value(1), mapValue.ValueAtIndex(new Value("test")));
        Assert.AreEqual(new Value("yeah"), mapValue.ValueAtIndex(new Value("huhu")));
        Assert.AreEqual(Value.Undefined, mapValue.ValueAtIndex(new Value("huu")));
        Assert.AreEqual(Value.Undefined, mapValue.ValueAtIndex(new Value("")));

        var exp1 = new Value(new Dictionary<string, Value> { { "test", mapValue.ValueAtIndex(new Value("test")) } });
        var exp2 = new Value(new Dictionary<string, Value> { { "huhu", mapValue.ValueAtIndex(new Value("huhu")) } });

        Assert.AreEqual(exp1, mapValue.ValueAtIndex(new Value(new[] { new Value("test") })));
        Assert.AreEqual(exp2, mapValue.ValueAtIndex(new Value(new[] { new Value("huhu") })));
        Assert.AreEqual(mapValue, mapValue.ValueAtIndex(new Value(new[] { new Value("test"), new Value("huhu") })));
        Assert.AreEqual(mapValue, mapValue.ValueAtIndex(new Value(new[] { new Value("huhu"), new Value("test") })));
        Assert.AreEqual(new Value(new Dictionary<string, Value>()), mapValue.ValueAtIndex(new Value(new[] { new Value("asdf") })));
        Assert.AreEqual(exp1, mapValue.ValueAtIndex(new Value(new[] { new Value("test"), new Value("asdf") })));
        Assert.ThrowsException<InvalidCastException>(() => mapValue.ValueAtIndex(new Value(new[] { new Value(0) })));
        Assert.ThrowsException<InvalidCastException>(() => mapValue.ValueAtIndex(new Value(new[] { new Value("test"), new Value(0) })));
    }

    // https://github.com/TrenchBroom/TrenchBroom/blob/master/common/test/src/EL/tst_Expression.cpp

    [TestMethod]
    public void TestValueLiterals()
    {
        Assert.AreEqual(Evaluate("null"), Value.Null);
        Assert.AreEqual(Evaluate("true"), new Value(true));
        Assert.AreEqual(Evaluate("false"), new Value(false));
        Assert.AreEqual(Evaluate("'asdf'"), new Value("asdf"));
        Assert.AreEqual(Evaluate("2"), new Value(2));
        Assert.AreEqual(Evaluate("2.2"), new Value(2.2m));
        Assert.AreEqual(Evaluate("-2"), new Value(-2));
        Assert.AreEqual(Evaluate("[2, 3]"), new Value(new []{ new Value(2), new Value(3) }));
        Assert.AreEqual(Evaluate("{k1:2, k2:3}"), new Value(new Dictionary<string, Value> { { "k1", new Value(2) }, { "k2", new Value(3) } }));
    }

    [TestMethod]
    public void TestVariableExpression()
    {
        Assert.AreEqual(Evaluate("x", new Dictionary<string, Value> { { "x", new Value(true) } }), new Value(true));
        Assert.AreEqual(Evaluate("x", new Dictionary<string, Value> { { "y", new Value(true) } }), Value.Undefined);
        Assert.AreEqual(Evaluate("x", new Dictionary<string, Value> { { "x", new Value(7) } }), new Value(7));
        Assert.AreEqual(Evaluate("x", new Dictionary<string, Value>()), Value.Undefined);
    }

    [TestMethod]
    public void TestArrayExpression()
    {
        Assert.AreEqual(Evaluate("[]"), new Value(Array.Empty<Value>()));
        Assert.AreEqual(Evaluate("[1, 2, 3]"), new Value(new[] { new Value(1), new Value(2), new Value(3) }));
        Assert.AreEqual(Evaluate("[1, 2, x]", new Dictionary<string, Value> { { "x", new Value("test") } }), new Value(new[] { new Value(1), new Value(2), new Value("test") }));
    }

    [TestMethod]
    public void TestMapExpression()
    {
        Assert.AreEqual(Evaluate("{}"), new Value(new Dictionary<string, Value>()));
        Assert.AreEqual(Evaluate("{k: true}"), new Value(new Dictionary<string, Value> { { "k", new Value(true)} }));
        Assert.AreEqual(Evaluate("{'k': true}"), new Value(new Dictionary<string, Value> { { "k", new Value(true)} }));
        Assert.AreEqual(Evaluate("{\"k\": true}"), new Value(new Dictionary<string, Value> { { "k", new Value(true)} }));
        Assert.AreEqual(Evaluate("{k1: true, k2: 3, k3: 3 + 7}"), new Value(new Dictionary<string, Value> { { "k1", new Value(true) }, { "k2", new Value(3) }, { "k3", new Value(10) } }));
        Assert.AreEqual(Evaluate("{k1: 'asdf', k2: x}", new Dictionary<string, Value> { { "x", new Value(55) } }), new Value(new Dictionary<string, Value> { { "k1", new Value("asdf") }, { "k2", new Value(55) } }));
    }

    [TestMethod]
    public void TestSubscriptExpression()
    {
        var vals = new Dictionary<string, Value>
        {
            { "arr", new Value(new [] { new Value(1), new Value(2), new Value(3) })},
            { "map", new Value(new Dictionary<string, Value> { { "k1", new Value(1) }, { "k2", new Value(2) }, { "k3", new Value(3) } })},
        };
        Assert.AreEqual(new Value(1), Evaluate("arr[0]", vals));
        Assert.AreEqual(new Value(2), Evaluate("arr[1]", vals));
        Assert.AreEqual(new Value(3), Evaluate("arr[2]", vals));

        Assert.AreEqual(new Value(1), Evaluate("map[\"k1\"]", vals));
        Assert.AreEqual(new Value(2), Evaluate("map[\"k2\"]", vals));
        Assert.AreEqual(new Value(3), Evaluate("map[\"k3\"]", vals));

        Assert.AreEqual(new Value(new[] { new Value(1), new Value(2) }), Evaluate("arr[0..1]", vals));
        Assert.AreEqual(new Value(new[] { new Value(1), new Value(2) }), Evaluate("arr[..1]", vals));
        Assert.AreEqual(new Value(new[] { new Value(2), new Value(3) }), Evaluate("arr[1..]", vals));
    }

    [DataTestMethod]
    [DataRow(32, 5)]
    [DataRow(16, 4)]
    [DataRow(8, 3)]
    [DataRow(4, 2)]
    [DataRow(2, 1)]
    [DataRow(1, 0)]
    public void TestSwitchExpression(int spawnflags, int expectedFrame)
    {
        var expression = @"{{
  spawnflags & 32 -> { ""path"": "":models/deadbods/dude/tris.md2"", ""frame"": 5 },
  spawnflags & 16 -> { ""path"": "":models/deadbods/dude/tris.md2"", ""frame"": 4 },
  spawnflags &  8 -> { ""path"": "":models/deadbods/dude/tris.md2"", ""frame"": 3 },
  spawnflags &  4 -> { ""path"": "":models/deadbods/dude/tris.md2"", ""frame"": 2 },
  spawnflags &  2 -> { ""path"": "":models/deadbods/dude/tris.md2"", ""frame"": 1 },
                                 "":models/deadbods/dude/tris.md2""
  }}";

        var result = Evaluate(expression, new Dictionary<string, Value> { { "spawnflags", new Value(spawnflags) } });
        if (expectedFrame == 0)
        {
            Assert.AreEqual(DataType.String, result.Type);
        }
        else
        {
            Assert.AreEqual(DataType.Map, result.Type);
            var value = result.ValueAtIndex(new Value("frame"));
            Assert.AreEqual(expectedFrame, value.IntValue);
        }
    }

    [DataTestMethod]
    [DataRow(2, "models/props/flag/flag1.md2")]
    [DataRow(1, "models/props/flag/flag3.md2")]
    [DataRow(0, "models/props/flag/tris.md2")]
    public void TestSwitchExpression2(int style, string expectedValue)
    {
        var expression = @"{{
            style &  2 -> ""models/props/flag/flag1.md2"",
            style &  1 -> ""models/props/flag/flag3.md2"",
                          ""models/props/flag/tris.md2""
        }}";

        var result = Evaluate(expression, new Dictionary<string, Value> { { "style", new Value(style) } });
        Assert.AreEqual(DataType.String, result.Type);
        Assert.AreEqual(expectedValue, result.StringValue);
    }

    // testOperators
    [TestMethod]
    public void TestOperators()
    {
        var exceptionType = typeof(Exception);
        var testCases = new Dictionary<string, object>
        {
            // Unary plus
            { "+true", new Value(1) },
            { "+false", new Value(0) },
            { "+1", new Value(1) },
            { "+'test'", exceptionType },
            { "+'2'", new Value(2) },
            { "+null", exceptionType },
            { "+[]", exceptionType },
            { "+{}", exceptionType },

            // Unary minus
            { "-true", new Value(-1) },
            { "-false", new Value(0) },
            { "-1", new Value(-1) },
            { "-'2'", new Value(-2) },
            { "-'test'", exceptionType },
            { "-null", exceptionType },
            { "-[]", exceptionType },
            { "-{}", exceptionType },
                
            // Addition
            { "true + true", new Value(2) },
            { "false + 3", new Value(3) },
            { "true + 'test'", exceptionType },
            { "true + '1.23'", new Value(2.23) },
            { "true + null", exceptionType },
            { "true + []", exceptionType },
            { "true + {}", exceptionType },
            { "1 + true", new Value(2) },
            { "3 + -1", new Value(2) },
            { "1 + '1.23'", new Value(2.23) },
            { "1 + 'test'", exceptionType },
            { "1 + null", exceptionType },
            { "1 + []", exceptionType },
            { "1 + {}", exceptionType },
                
            { "'test' + true", exceptionType },
            { "'test' + 2", exceptionType },
            { "'1.23' + 2", new Value(3.23) },
            { "'this' + 'test'", new Value("thistest") },
            { "'this' + '1.23'", new Value("this1.23") },
            { "'test' + null", exceptionType },
            { "'test' + []", exceptionType },
            { "'test' + {}", exceptionType },

            { "null + true", exceptionType },
            { "null + 2", exceptionType },
            { "null + 'test'", exceptionType },
            { "null + null", exceptionType },
            { "null + []", exceptionType },
            { "null + {}", exceptionType },

            { "[] + true", exceptionType },
            { "[] + 2", exceptionType },
            { "[] + 'test'", exceptionType },
            { "[] + null", exceptionType },
            { "[1, 2] + [2, 3]", new Value(new[] { new Value(1), new Value(2), new Value(2), new Value(3) }) },
            { "[] + {}", exceptionType },

            { "{} + true", exceptionType },
            { "{} + 2", exceptionType },
            { "{} + 'test'", exceptionType },
            { "{} + null", exceptionType },
            { "{} + []", exceptionType },
            {
                "{k1: 1, k2: 2, k3: 3} + {k3: 4, k4: 5}", new Value(
                    new Dictionary<string, Value>
                    {
                        { "k1", new Value(1) },
                        { "k2", new Value(2) },
                        { "k3", new Value(4) },
                        { "k4", new Value(5) },
                    })
            },
                
            // Subtraction
            { "true - true", new Value(0) },
            { "false - 3", new Value(-3) },
            { "true - 'test'", exceptionType },
            { "true - '2.0'", new Value(-1.0) },
            { "true - null", exceptionType },
            { "true - []", exceptionType },
            { "true - {}", exceptionType },

            { "1 - true", new Value(0) },
            { "3 - 1", new Value(2) },
            { "1 - 'test'", exceptionType },
            { "1 - '2.23'", new Value(-1.23) },
            { "1 - null", exceptionType },
            { "1 - []", exceptionType },
            { "1 - {}", exceptionType },

            { "'test' - true", exceptionType },
            { "'test' - 2", exceptionType },
            { "'3.23' - 2", new Value(1.23) },
            { "'this' - 'test'", exceptionType },
            { "'test' - null", exceptionType },
            { "'test' - []", exceptionType },
            { "'test' - {}", exceptionType },

            { "null - true", exceptionType },
            { "null - 2", exceptionType },
            { "null - 'test'", exceptionType },
            { "null - null", exceptionType },
            { "null - []", exceptionType },
            { "null - {}", exceptionType },

            { "[] - true", exceptionType },
            { "[] - 2", exceptionType },
            { "[] - 'test'", exceptionType },
            { "[] - null", exceptionType },
            { "[] - []", exceptionType },
            { "[] - {}", exceptionType },

            { "{} - true", exceptionType },
            { "{} - 2", exceptionType },
            { "{} - 'test'", exceptionType },
            { "{} - null", exceptionType },
            { "{} - []", exceptionType },
            { "{} - {}", exceptionType },
                
            // Multiplication
            { "true * true", new Value(1) },
            { "true * false", new Value(0) },
            { "true * 3", new Value(3) },
            { "true * '3'", new Value(3) },
            { "true * 'test'", exceptionType },
            { "true * null", exceptionType },
            { "true * []", exceptionType },
            { "true * {}", exceptionType },

            { "1 * true", new Value(1) },
            { "3 * 2", new Value(6) },
            { "1 * 'test'", exceptionType },
            { "2 * '2.23'", new Value(4.46) },
            { "1 * null", exceptionType },
            { "1 * []", exceptionType },
            { "1 * {}", exceptionType },

            { "'test' * true", exceptionType },
            { "'test' * 2", exceptionType },
            { "'1.23' * 2", new Value(2.46) },
            { "'this' * 'test'", exceptionType },
            { "'test' * null", exceptionType },
            { "'test' * []", exceptionType },
            { "'test' * {}", exceptionType },

            { "null * true", exceptionType },
            { "null * 2", exceptionType },
            { "null * 'test'", exceptionType },
            { "null * null", exceptionType },
            { "null * []", exceptionType },
            { "null * {}", exceptionType },

            { "[] * true", exceptionType },
            { "[] * 2", exceptionType },
            { "[] * 'test'", exceptionType },
            { "[] * null", exceptionType },
            { "[] * []", exceptionType },
            { "[] * {}", exceptionType },

            { "{} * true", exceptionType },
            { "{} * 2", exceptionType },
            { "{} * 'test'", exceptionType },
            { "{} * null", exceptionType },
            { "{} * []", exceptionType },
            { "{} * {}", exceptionType },

            // Division
            { "true / true", new Value(1) },
            { "true / false", new Value(double.PositiveInfinity) },
            { "true / 3", new Value(1.0 / 3.0) },
            { "true / '2'", new Value(1.0 / 2.0) },
            { "true / 'test'", exceptionType },
            { "true / null", exceptionType },
            { "true / []", exceptionType },
            { "true / {}", exceptionType },

            { "1 / true", new Value(1) },
            { "3 / 2", new Value(1.5) },
            { "1 / 'test'", exceptionType },
            { "1 / '3.0'", new Value(1.0 / 3.0) },
            { "1 / null", exceptionType },
            { "1 / []", exceptionType },
            { "1 / {}", exceptionType },

            { "'test' / true", exceptionType },
            { "'test' / 2", exceptionType },
            { "'3' / 2", new Value(3.0 / 2.0) },
            { "'this' / 'test'", exceptionType },
            { "'test' / null", exceptionType },
            { "'test' / []", exceptionType },
            { "'test' / {}", exceptionType },

            { "null / true", exceptionType },
            { "null / 2", exceptionType },
            { "null / 'test'", exceptionType },
            { "null / null", exceptionType },
            { "null / []", exceptionType },
            { "null / {}", exceptionType },

            { "[] / true", exceptionType },
            { "[] / 2", exceptionType },
            { "[] / 'test'", exceptionType },
            { "[] / null", exceptionType },
            { "[] / []", exceptionType },
            { "[] / {}", exceptionType },

            { "{} / true", exceptionType },
            { "{} / 2", exceptionType },
            { "{} / 'test'", exceptionType },
            { "{} / null", exceptionType },
            { "{} / []", exceptionType },
            { "{} / {}", exceptionType },
                
            // Modulus
            { "true % true", new Value(0) },
            { "true % -2", new Value(1) },
            { "true % 'test'", exceptionType },
            { "true % '1'", new Value(0) },
            { "true % null", exceptionType },
            { "true % []", exceptionType },
            { "true % {}", exceptionType },

            { "3 % -2", new Value(1) },
            { "3 % '-2'", new Value(1) },
            { "1 % 'test'", exceptionType },
            { "1 % null", exceptionType },
            { "1 % []", exceptionType },
            { "1 % {}", exceptionType },

            { "'test' % true", exceptionType },
            { "'test' % 2", exceptionType },
            { "'3' % 2", new Value(1) },
            { "'this' % 'test'", exceptionType },
            { "'test' % null", exceptionType },
            { "'test' % []", exceptionType },
            { "'test' % {}", exceptionType },

            { "null % true", exceptionType },
            { "null % 2", exceptionType },
            { "null % 'test'", exceptionType },
            { "null % null", exceptionType },
            { "null % []", exceptionType },
            { "null % {}", exceptionType },

            { "[] % true", exceptionType },
            { "[] % 2", exceptionType },
            { "[] % 'test'", exceptionType },
            { "[] % null", exceptionType },
            { "[] % []", exceptionType },
            { "[] % {}", exceptionType },

            { "{} % true", exceptionType },
            { "{} % 2", exceptionType },
            { "{} % 'test'", exceptionType },
            { "{} % null", exceptionType },
            { "{} % []", exceptionType },
            { "{} % {}", exceptionType },
                
            // Logical negation
            { "!true", new Value(false) },
            { "!false", new Value(true) },
            { "!1", exceptionType },
            { "!'test'", exceptionType },
            { "!null", exceptionType },
            { "![]", exceptionType },
            { "!{}", exceptionType },
                
            // Logical conjunction
            { "false && false", new Value(false) },
            { "false && true", new Value(false) },
            { "true && false", new Value(false) },
            { "true && true", new Value(true) },
                
            // Logical disjunction
            { "false || false", new Value(false) },
            { "false || true", new Value(true) },
            { "true || false", new Value(true) },
            { "true || true", new Value(true) },

            // Logical short circuit evaluation 
            { "false && x[-1]", new Value(false) },
            { "true || x[-1]", new Value(true) },
                
            // Bitwise negation
            { "~23423", new Value(~23423) },
            { "~23423.1", new Value(~23423) },
            { "~23423.8", new Value(~23423) },
            { "~true", exceptionType },
            { "~'23423'", new Value(~23423) },
            { "~'asdf'", exceptionType },
            { "~null", exceptionType },
            { "~[]", exceptionType },
            { "~{}", exceptionType },
                
            // Bitwise and
            { "0 & 0", new Value(0 & 0) },
            { "123 & 456", new Value(123 & 456) },
            { "true & 123", new Value(1 & 123) },
            { "123 & true", new Value(123 & 1) },
            { "'asdf' & 123", exceptionType },
            { "'456' & 123", new Value(456 & 123) },
            { "123 & 'asdf'", exceptionType },
            { "123 & '456'", new Value(123 & 456) },
            { "null & 123", new Value(0 & 123) },
            { "123 & null", new Value(123 & 0) },
            { "[] & 123", exceptionType },
            { "123 & []", exceptionType },
            { "{} & 123", exceptionType },
            { "123 & {}", exceptionType },

            // Bitwise or
            { "0 | 0", new Value(0 | 0) },
            { "123 | 456", new Value(123 | 456) },
            { "true | 123", new Value(1 | 123) },
            { "123 | true", new Value(123 | 1) },
            { "'asdf' | 123", exceptionType },
            { "'456' | 123", new Value(456 | 123) },
            { "123 | 'asdf'", exceptionType },
            { "123 | '456'", new Value(123 | 456) },
            { "null | 123", new Value(0 | 123) },
            { "123 | null", new Value(123 | 0) },
            { "[] | 123", exceptionType },
            { "123 | []", exceptionType },
            { "{} | 123", exceptionType },
            { "123 | {}", exceptionType },

            // Bitwise xor
            { "0 ^ 0", new Value(0 ^ 0) },
            { "123 ^ 456", new Value(123 ^ 456) },
            { "true ^ 123", new Value(1 ^ 123) },
            { "123 ^ true", new Value(123 ^ 1) },
            { "'asdf' ^ 123", exceptionType },
            { "'456' ^ 123", new Value(456 ^ 123) },
            { "123 ^ 'asdf'", exceptionType },
            { "123 ^ '456'", new Value(123 ^ 456) },
            { "null ^ 123", new Value(0 ^ 123) },
            { "123 ^ null", new Value(123 ^ 0) },
            { "[] ^ 123", exceptionType },
            { "123 ^ []", exceptionType },
            { "{} ^ 123", exceptionType },
            { "123 ^ {}", exceptionType },
                
            // ReSharper disable ShiftExpressionZeroLeftOperand
            // Bitwise shift left
            { "1 << 2", new Value(1 << 2) },
            { "true << 2", new Value(1 << 2) },
            { "1 << false", new Value(1 << 0) },
            { "'asdf' << 2", exceptionType },
            { "'1' << 2", new Value(1 << 2) },
            { "1 << 'asdf'", exceptionType },
            { "1 << '2'", new Value(1 << 2) },
            { "null << 2", new Value(0 << 2) },
            { "1 << null", new Value(1 << 0) },
            { "[] << 2", exceptionType },
            { "1 << []", exceptionType },
            { "{} << 2", exceptionType },
            { "1 << {}", exceptionType },
            // ReSharper restore ShiftExpressionZeroLeftOperand
                
            // ReSharper disable ShiftExpressionZeroLeftOperand
            // ReSharper disable ShiftExpressionResultEqualsZero
            // Bitwise shift right
            { "1 >> 2", new Value(1 >> 2) },
            { "true >> 2", new Value(1 >> 2) },
            { "1 >> false", new Value(1 >> 0) },
            { "'asdf' >> 2", exceptionType },
            { "'1' >> 2", new Value(1 >> 2) },
            { "1 >> 'asdf'", exceptionType },
            { "1 >> '2'", new Value(1 >> 2) },
            { "null >> 2", new Value(0 >> 2) },
            { "1 >> null", new Value(1 >> 0) },
            { "[] >> 2", exceptionType },
            { "1 >> []", exceptionType },
            { "{} >> 2", exceptionType },
            { "1 >> {}", exceptionType },
            // ReSharper restore ShiftExpressionResultEqualsZero
            // ReSharper restore ShiftExpressionZeroLeftOperand
                
            // Comparison
            { "false < false", new Value(false) },
            { "false < true", new Value(true) },
            { "true < false", new Value(false) },
            { "true < true", new Value(false) },
                
            { "false < 0", new Value(false) },
            { "false < 1", new Value(true) },
            { "false < 'true'", new Value(true) },
            { "false < 'false'", new Value(false) },
            { "false < ''", new Value(false) },
            { "false < null", new Value(false) },
            { "false < []", exceptionType },
            { "false < {}", exceptionType },
                
            { "0 < 0", new Value(false) },
            { "0 < 1", new Value(true) },
            { "0 < 'true'", exceptionType },
            { "0 < 'false'", exceptionType },
            { "0 < ''", new Value(false) },
            { "0 < '0'", new Value(false) },
            { "0 < '1'", new Value(true) },
            { "0 < null", new Value(false) },
            { "0 < []", exceptionType },
            { "0 < {}", exceptionType },
                
            { "'a' < 0", exceptionType },
            { "'a' < 1", exceptionType },
            { "'a' < 'true'", new Value(true) },
            { "'a' < 'false'", new Value(true) },
            { "'a' < ''", new Value(false) },
            { "'a' < 'b'", new Value(true) },
            { "'a' < 'a'", new Value(false) },
            { "'aa' < 'ab'", new Value(true) },
            { "'a' < null", new Value(false) },
            { "'a' < []", exceptionType },
            { "'a' < {}", exceptionType },
            { "'0' < 1", new Value(true) },
            { "'1' < 0", new Value(false) },
                
            { "null < true", new Value(true) },
            { "null < false", new Value(true) },
            { "null < 0", new Value(true) },
            { "null < 1", new Value(true) },
            { "null < ''", new Value(true) },
            { "null < 'a'", new Value(true) },
            { "null < null", new Value(false) },
            { "null < []", new Value(true) },
            { "null < {}", new Value(true) },
                
            { "[] < true", exceptionType },
            { "[] < false", exceptionType },
            { "[] < 0", exceptionType },
            { "[] < 1", exceptionType },
            { "[] < ''", exceptionType },
            { "[] < 'a'", exceptionType },
            { "[] < null", new Value(false) },
            { "[] < []", new Value(false) },
            { "[1] < [1]", new Value(false) },
            { "[1] < [2]", new Value(true) },
            { "[1] < [1,2]", new Value(true) },
            { "[1,2] < [1,2]", new Value(false) },
            { "[1,2] < [1,2,3]", new Value(true) },
            { "[1,2,3] < [1,2]", new Value(false) },
            { "[] < {}", exceptionType },
                
            { "{} < true", exceptionType },
            { "{} < false", exceptionType },
            { "{} < 0", exceptionType },
            { "{} < 1", exceptionType },
            { "{} < ''", exceptionType },
            { "{} < 'a'", exceptionType },
            { "{} < null", new Value(false) },
            { "{} < []", exceptionType },
            { "{} < {}", new Value(false) },
            { "{k1:1} < {k1:1}", new Value(false) },
            { "{k1:1} < {k2:1}", new Value(true) },
            { "{k2:1} < {k1:1}", new Value(false) },
            { "{k1:1} < {k1:2}", new Value(true) },
            { "{k1:1} < {k1:1, k2:2}", new Value(true) },
            { "{k1:1} < {k1:2, k2:2}", new Value(true) },
                
            { "false == false", new Value(true) },
            { "false == true", new Value(false) },
            { "true == false", new Value(false) },
            { "true == true", new Value(true) },

            { "false == 0", new Value(true) },
            { "false == 1", new Value(false) },
            { "false == 'true'", new Value(false) },
            { "false == 'false'", new Value(true) },
            { "false == ''", new Value(true) },
            { "false == null", new Value(false) },
            { "false == []", exceptionType },
            { "false == {}", exceptionType },

            { "0 == 0", new Value(true) },
            { "0 == 1", new Value(false) },
            { "0 == 'true'", exceptionType },
            { "0 == 'false'", exceptionType },
            { "0 == ''", new Value(true) },
            { "0 == '0'", new Value(true) },
            { "0 == '1'", new Value(false) },
            { "0 == null", new Value(false) },
            { "0 == []", exceptionType },
            { "0 == {}", exceptionType },

            { "'a' == 0", exceptionType },
            { "'a' == 1", exceptionType },
            { "'a' == 'b'", new Value(false) },
            { "'a' == 'a'", new Value(true) },
            { "'aa' == 'ab'", new Value(false) },
            { "'a' == null", new Value(false) },
            { "'a' == []", exceptionType },
            { "'a' == {}", exceptionType },
            { "'0' == 0", new Value(true) },
            { "'0' == 1", new Value(false) },

            { "null == true", new Value(false) },
            { "null == false", new Value(false) },
            { "null == 0", new Value(false) },
            { "null == 1", new Value(false) },
            { "null == ''", new Value(false) },
            { "null == 'a'", new Value(false) },
            { "null == null", new Value(true) },
            { "null == []", new Value(false) },
            { "null == {}", new Value(false) },

            { "[] == true", exceptionType },
            { "[] == false", exceptionType },
            { "[] == 0", exceptionType },
            { "[] == 1", exceptionType },
            { "[] == ''", exceptionType },
            { "[] == 'a'", exceptionType },
            { "[] == null", new Value(false) },
            { "[] == []", new Value(true) },
            { "[1] == [1]", new Value(true) },
            { "[1] == [2]", new Value(false) },
            { "[1] == [1,2]", new Value(false) },
            { "[1,2] == [1,2]", new Value(true) },
            { "[1,2] == [1,2,3]", new Value(false) },
            { "[1,2,3] == [1,2]", new Value(false) },
            { "[] == {}", exceptionType },

            { "{} == true", exceptionType },
            { "{} == false", exceptionType },
            { "{} == 0", exceptionType },
            { "{} == 1", exceptionType },
            { "{} == ''", exceptionType },
            { "{} == 'a'", exceptionType },
            { "{} == null", new Value(false) },
            { "{} == []", exceptionType },
            { "{} == {}", new Value(true) },
            { "{k1:1} == {k1:1}", new Value(true) },
            { "{k1:1} == {k2:1}", new Value(false) },
            { "{k2:1} == {k1:1}", new Value(false) },
            { "{k1:1} == {k1:2}", new Value(false) },
            { "{k1:1} == {k1:1, k2:2}", new Value(false) },
            { "{k1:1} == {k1:2, k2:2}", new Value(false) },
                
            { "true -> 'asdf'", new Value("asdf") },
            { "false -> 'asdf'", Value.Undefined },
            { "false -> x[-1]", Value.Undefined },
        };

        foreach (var (expression, result) in testCases)
        {
            if (result as Type == exceptionType)
            {
                try
                {
                    Evaluate(expression);
                    Assert.Fail($"Expected exception thrown by expression: {expression}");
                }
                catch (Exception ex)
                {
                    // good
                }
            }
            else if (result is Value v)
            {
                try
                {
                    Assert.AreEqual(v, Evaluate(expression), $"Failure: {expression}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Valid expression throw exception: {expression}", ex);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            Console.WriteLine($"Success: {expression}");
        }
    }

    [TestMethod]
    public void TestOperatorPrecedence()
    {
        Assert.AreEqual(new Value(1.0 + 2.0 - 3.0), Evaluate("1 + 2 - 3"));
        Assert.AreEqual(new Value(1.0 - 2.0 + 3.0), Evaluate("1 - 2 + 3"));
        Assert.AreEqual(new Value(2.0 * 3.0 + 4.0), Evaluate("2 * 3 + 4"));
        Assert.AreEqual(new Value(2.0 + 3.0 * 4.0), Evaluate("2 + 3 * 4"));
        Assert.AreEqual(new Value(2.0 * 3.0 - 4.0), Evaluate("2 * 3 - 4"));
        Assert.AreEqual(new Value(2.0 - 3.0 * 4.0), Evaluate("2 - 3 * 4"));
        Assert.AreEqual(new Value(6.0 / 2.0 + 4), Evaluate("6 / 2 + 4"));
        Assert.AreEqual(new Value(6.0 + 2.0 / 4.0), Evaluate("6 + 2 / 4"));
        Assert.AreEqual(new Value(6.0 / 2.0 - 4.0), Evaluate("6 / 2 - 4"));
        Assert.AreEqual(new Value(6.0 - 2.0 / 4.0), Evaluate("6 - 2 / 4"));
        Assert.AreEqual(new Value(2.0 * 6.0 / 4.0), Evaluate("2 * 6 / 4"));
        Assert.AreEqual(new Value(2.0 / 6.0 * 4.0), Evaluate("2 / 6 * 4"));
        Assert.AreEqual(new Value(2 + 3 * 4 + 5), Evaluate("2 + 3 * 4 + 5"));
        Assert.AreEqual(new Value(2 * 3 + 4 + 5), Evaluate("2 * 3 + 4 + 5"));
        Assert.AreEqual(new Value(2 * 3 + 4 & 5), Evaluate("2 * 3 + 4 & 5"));
        Assert.AreEqual(new Value(true), Evaluate("false && false || true"));
        Assert.AreEqual(new Value(true), Evaluate("!true && !true || !false"));
        Assert.AreEqual(new Value(true), Evaluate("3 < 10 || 10 > 2"));
        Assert.AreEqual(new Value(true), Evaluate("2 + 3 < 2 + 4"));
        Assert.AreEqual(Value.Undefined, Evaluate("true && false -> true"));
        Assert.AreEqual(new Value(false), Evaluate("true && true -> false"));
        Assert.AreEqual(new Value(1), Evaluate("2 + 3 < 2 + 4 -> 6 % 5"));
    }

    // testOptimize
    // not important for basic implementation. excluded.

    // https://github.com/TrenchBroom/TrenchBroom/blob/master/common/test/src/EL/tst_Interpolator.cpp

    private static string Interpolate(string input, Dictionary<string, Value> values = null)
    {
        var context = new EvaluationContext(values ?? new Dictionary<string, Value>());
        var exp = ExpressionParser.ParseInterpolatedString(input);
        var evaluator = new ExpressionEvaluator();
        return evaluator.Evaluate(exp, context).StringValue;
    }

    [TestMethod]
    public void TestInterpolateEmptyString()
    {
        Assert.AreEqual("", Interpolate(""));
        Assert.AreEqual("   ", Interpolate("   "));
    }

    [TestMethod]
    public void TestInterpolateStringWithoutExpression()
    {
        Assert.AreEqual(" asdfasdf  sdf ", Interpolate(" asdfasdf  sdf "));
    }

    [TestMethod]
    public void TestInterpolateStringWithSimpleExpression()
    {
        Assert.AreEqual(" asdfasdf asdf  sdf ", Interpolate(" asdfasdf ${'asdf'}  sdf "));
        Assert.AreEqual(" asdfasdf asdf AND  sdf ", Interpolate(" asdfasdf ${'asdf'} ${'AND'}  sdf "));
        Assert.AreEqual(" asdfasdf asdf AND  sdf ", Interpolate(" asdfasdf ${'asdf'}${' AND'}  sdf "));
        Assert.AreEqual(" true ", Interpolate(" ${ true } "));
        Assert.AreEqual(" this and that ", Interpolate(" ${ 'this'+' and ' }${'that'} "));
    }

    [TestMethod]
    public void TestInterpolateStringWithNestedExpression()
    {
        Assert.AreEqual(" asdfasdf nested ${TEST} expression  sdf ", Interpolate(" asdfasdf ${ 'nested ${TEST} expression' }  sdf "));
    }

    [TestMethod]
    public void TestInterpolateStringWithVariable()
    {
        Assert.AreEqual(" an interesting expression", Interpolate(" an ${TEST} expression", new Dictionary<string, Value> { { "TEST", new Value("interesting") } }));
    }

    [TestMethod]
    public void TestInterpolateStringWithBackslashAndVariable()
    {
        Assert.AreEqual(" an \\interesting expression", Interpolate(" an \\${TEST} expression", new Dictionary<string, Value> { { "TEST", new Value("interesting") } }));
    }
        
    [TestMethod]
    public void TestInterpolateStringWithUnknownVariable()
    {
        Assert.ThrowsException<InvalidCastException>(() => Interpolate(" an \\${TEST} expression"));
    }
        
    [TestMethod]
    public void TestInterpolateStringWithUnterminatedEl()
    {
        Assert.ThrowsException<TokenParsingException>(() => Interpolate(" an ${TEST"));
        Assert.ThrowsException<TokenParsingException>(() => Interpolate(" an ${TEST expression"));
    }

    private static Value EvaluateInterpolate(string expression, Dictionary<string, Value> values = null)
    {
        var context = new EvaluationContext(values ?? new Dictionary<string, Value>());
        var exp = ExpressionParser.Parse(expression, new ExpressionParseOptions { AutomaticInterpolation = true });
        var evaluator = new ExpressionEvaluator();
        return evaluator.Evaluate(exp, context);
    }

    [TestMethod]
    public void TestExpressionWithAutomaticInterpolation()
    {
        var values = new Dictionary<string, Value>
        {
            { "a", new Value("a") },
            { "b", new Value(1) },
            { "c", new Value(true) },
        };
        Assert.AreEqual(new Value("expected-a"), EvaluateInterpolate(@"true -> ""expected-${a}""", values));
        Assert.AreEqual(new Value("expected-1"), EvaluateInterpolate(@"true -> ""expected-${b}""", values));
        Assert.AreEqual(new Value("expected-true"), EvaluateInterpolate(@"true -> ""expected-${c}""", values));
    }
}