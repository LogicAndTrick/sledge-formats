using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sledge.Formats.GameData.Tests.Fgd;

[TestClass]
public class TestFgdSyntaxErrorRecovery
{
    [TestMethod]
    public void TestIncorrectlyCommentedClass()
    {
        const string fgd = @"
@SolidClass = valid1 []
//@SolidClass = invalid1 [
//    test(choices) =
//    [
        0 : ""0""
		1 : ""1"" : ""one""
//    ]
//@SolidClass = invalid2
[
	test(string) : ""Test""
]
@SolidClass = valid2 []";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(2, def.Classes.Count);
    }

    [TestMethod]
    public void TestUnquotedDefaultValue()
    {
        const string fgd = @"
@PointClass = test
[
	model(studio) : ""Model"" : models/test-model.mdl : ""Select a model file.""
]";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        Assert.AreEqual("models/test-model.mdl", def.Classes[0].Properties[0].DefaultValue);
    }

    [TestMethod]
    public void TestDefaultValueMissingColon()
    {
        const string fgd = @"
@PointClass = test
[
	test(choices) : ""Test"" 0 =
	[
		0: ""zero""
		1: ""one""
	]
]";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        Assert.AreEqual("0", def.Classes[0].Properties[0].DefaultValue);
    }

    [TestMethod]
    public void TestUnterminatedString()
    {
        const string fgd = @"
// from spirit.fgd
@PointClass = test
[
	frags(choices) : ""If outside range"" : 0 =
	[
		//* e.g. if the range were 0%-100%, and the value were 120%, the result would be 100%.
		0 : ""Pick nearest value""
		//* In the case above, the result would be 20%.
		1 : ""Wrap around""
		//* In the case above, the result would be 80%.
		2 : ""Bounce back""
		//* Treated as 0. Or you can catch this failure with calc_fallback.
		3 : ""Fail
	]
]";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        Assert.AreEqual("Fail", def.Classes[0].Properties[0].Options.Last().Description);
    }

    //[TestMethod]
    public void TestDecimalDefault5()
    {
        const string fgd = @"
@PointClass = test
[
	test(integer) : ""Description"" : 1.5
]";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
    }
}